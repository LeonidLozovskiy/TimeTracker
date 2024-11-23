using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;
using LiteDB;
using TimeTracker.ViewModelEntities;

#nullable disable
namespace TimeTracker
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private ObservableCollection<TaskViewModel> tasks;
        private ObservableCollection<string> tags = [];
        private readonly string AdmTaskName = Settings.AdministrativeTask;
        private string addTaskName = string.Empty;
        private TimeSpan timeBeforeAutoLog = TimeSpan.FromHours(0.0);
        private Task timer;
        private CancellationTokenSource cancellationTokenSource = new();
        private static HttpClient Client;
        private string comment;
        private int totalWorkTimeForDay;
        private string selectedTag;
        private string loadingMessage;
        private bool isLogingInProgress;
        private DateTime logDate = DateTime.UtcNow;
        private HashSet<DateTime> _calendarHistoryDates;

        public ObservableCollection<History> Histories { get; set; } = [];

        public MainWindowViewModel()
        {
            tasks = ([
                new TaskViewModel(
                    AdmTaskName,
                    new Action<TaskViewModel>(DeleteTask),
                    new Action(RecalculateTime),
                    false,
                    120,
                    Settings.AdministrativeMessage),
            ]);
            Tags = new ObservableCollection<string>(Settings.Tags);
            TotalWorkTimeForDay = 480;
            LoadingMessage = "Загрузка...";
            AddCommand = new RelayCommand(AddTaskClick);
            AddMiscCommand = new RelayCommand(AddMiscTaskClick);
            StartTimerCommand = new RelayCommand(StartTimer);
            SendLogsNowCommand = new RelayCommand(SendLogsNow);
            RecalculateTime();
            GetHistory();

            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly);
            var cert = store.Certificates.FirstOrDefault(x => x.Subject.Contains("mercury", StringComparison.InvariantCultureIgnoreCase));
            if (cert == null)
            {
                IsLogingInProgress = true;
                LoadingMessage = $"Сертификат не найден,{Environment.NewLine}логирование невозможно";
            }

            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);
            Client = new HttpClient(handler);
        }

        public ICommand SendLogsNowCommand { get; set; }

        private void DeleteTask(TaskViewModel task)
        {
            tasks.Remove(task);
            RecalculateTime();
        }

        private void RecalculateTime()
        {
            int num1 = tasks.Count(x => x.CustomTime <= 0);
            if (num1 <= 0)
            {
                num1 = 1;
            }

            int num2 = totalWorkTimeForDay - tasks.Where(x => x.CustomTime != 0).Sum(x => x.CustomTime);
            int num3 = num2 / num1;
            int num4 = num2;
            foreach (var task in (Collection<TaskViewModel>)tasks)
            {
                if (task.CustomTime != 0)
                {
                    task.TimeInMinutes = task.CustomTime;
                }
                else
                {
                    task.TimeInMinutes = num3;
                    num4 -= num3;
                }
            }

          (tasks.LastOrDefault(x => x.CustomTime == 0) ?? tasks.Last()).TimeInMinutes += num4;
        }

        private void GetHistory()
        {
            using LiteDatabase liteDatabase = new("History.db");
            ILiteCollection<History> collection = liteDatabase.GetCollection<History>("Histories", BsonAutoId.ObjectId);
            Histories.Clear();
            foreach (var history in (IEnumerable<History>)collection
                .FindAll()
                .Where(x => x.Date > DateTime.Today.AddDays(-Settings.HistoryDepth))
                .OrderByDescending(x => x.Date))
            {
                Histories.Add(history);
            }

            CalendarHistoryDates = Histories.Where(x => x.TimeSum == 8).Select(x => x.Date).ToHashSet();
        }

        private void AddTaskClick(object o)
        {
            tasks.Add(new TaskViewModel(addTaskName,
                new Action<TaskViewModel>(DeleteTask),
                new Action(RecalculateTime),
                comment: $"#{SelectedTag}: "));
            AddTaskName = string.Empty;
            RecalculateTime();
        }

        private void AddMiscTaskClick(object o)
        {
            tasks.Add(new TaskViewModel(Settings.MiscTask,
                new Action<TaskViewModel>(DeleteTask),
                new Action(RecalculateTime),
                comment: $"#{SelectedTag}: "));
            RecalculateTime();
        }

        private void StartTimer(object o)
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;
            TimeBeforeAutoLog = TimeSpan.FromHours(8.0);
            timer = Timer(token);
        }

        private async Task Timer(CancellationToken token)
        {
            while (timeBeforeAutoLog.TotalSeconds > 0.0 && DateTime.Now.Hour < 23 && !token.IsCancellationRequested)
            {
                TimeBeforeAutoLog -= TimeSpan.FromSeconds(1.0);
                await Task.Delay(1000, token);
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            _ = LogTime();
        }

        private void SendLogsNow(object o)
        {
            StopTimer();
            _ = LogTime();
        }

        private void StopTimer()
        {
            cancellationTokenSource.Cancel();
        }

        private async Task LogTime()
        {
            try
            {
                IsLogingInProgress = true;
                foreach (var task in tasks.Where(x => !x.IsLogged))
                {
                    try
                    {
                        using HttpRequestMessage request = new(
                        new HttpMethod("POST"),
                        "https://" + Settings.Jira + "/rest/api/2/issue/" + task.Name + "/worklog");

                        request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + Settings.Authorization);
                        request.Content = new StringContent(string.Format(
                            "{{                \n\"timeSpentSeconds\": \"{0}\",\n\"comment\": \"{1}\",\n\"started\": \"{2}.301+0300\"\n}}",
                            task.TimeInMinutes * 60,
                            task.Comment,
                            LogDate.ToString("s", CultureInfo.InvariantCulture)));
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        if ((await Client.SendAsync(request).ConfigureAwait(true)).IsSuccessStatusCode)
                        {
                            task.IsLogged = true;
                            LogHistory(LogDate, task);
                            GetHistory();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            finally
            {
                await Task.Delay(1000);
                IsLogingInProgress = false;
            }
        }

        private static void LogHistory(DateTime logDate, TaskViewModel task)
        {
            using LiteDatabase liteDatabase = new("History.db");
            ILiteCollection<History> collection = liteDatabase.GetCollection<History>("Histories", BsonAutoId.ObjectId);
            History entity = collection.FindOne(x => x.Date == logDate);
            if (entity == null)
            {
                entity = new History()
                {
                    Date = logDate,
                    Rows = []
                };
                collection.Insert(entity);
            }

            entity.Rows.Add(new HistoryRow()
            {
                TaskId = task.Name,
                TrackedTime = task.TimeInMinutes,
                Comment = task.Comment
            });
            collection.Update(entity);
        }

        public RelayCommand AddCommand { get; set; }
        public RelayCommand AddMiscCommand { get; set; }
        public RelayCommand StartTimerCommand { get; set; }

        public ObservableCollection<TaskViewModel> Tasks
        {
            get => tasks;
            set => Set(ref tasks, value, nameof(Tasks));
        }

        public ObservableCollection<string> Tags
        {
            get => tags;
            set => Set(ref tags, value, nameof(Tags));
        }

        public string SelectedTag
        {
            get => selectedTag;
            set => Set(ref selectedTag, value, nameof(SelectedTag));
        }

        public string LoadingMessage
        {
            get => loadingMessage;
            set => Set(ref loadingMessage, value, nameof(LoadingMessage));
        }

        public bool IsLogingInProgress
        {
            get => isLogingInProgress;
            set => Set(ref isLogingInProgress, value, nameof(IsLogingInProgress));
        }

        public int TotalWorkTimeForDay
        {
            get => totalWorkTimeForDay;

            set
            {
                Set(ref totalWorkTimeForDay, value, nameof(TotalWorkTimeForDay));
                RecalculateTime();
            }
        }

        public TimeSpan TimeBeforeAutoLog
        {
            get => timeBeforeAutoLog;
            set => Set(ref timeBeforeAutoLog, value, nameof(TimeBeforeAutoLog));
        }

        public string AddTaskName
        {
            get => addTaskName;
            set => Set(ref addTaskName, value, nameof(AddTaskName));
        }

        public string Comment
        {
            get => comment;
            set => Set(ref comment, value, nameof(Comment));
        }

        public DateTime LogDate
        {
            get => logDate;
            set => Set(ref logDate, value.AddHours(13.0), nameof(LogDate));
        }

        public HashSet<DateTime> CalendarHistoryDates
        {
            get => _calendarHistoryDates;
            set => Set(ref _calendarHistoryDates, value, nameof(CalendarHistoryDates));
        }

        public void Dispose()
        {
        }
    }
}
