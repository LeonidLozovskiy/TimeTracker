// Decompiled with JetBrains decompiler
// Type: TimeTracker.ViewModelEntities.TaskViewModel
// Assembly: TimeTracker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 171B3FE6-2A84-4D57-B757-BB4CEB626B4D
// Assembly location: C:\TimeTracker\TimeTracker.dll

using System.Windows.Input;

#nullable disable
namespace TimeTracker
{
    public class TaskViewModel : ViewModelBase
    {
        private string name;
        private int customTime;
        private string comment = string.Empty;
        private int timeInMinutes;
        private bool isLogged;
        private readonly Action recalculateTime;
        private bool canBeDeleted;
        private readonly Action<TaskViewModel> deleteCommandAction;

        public TaskViewModel(
          string name,
          Action<TaskViewModel> deleteCommandAction,
          Action recalculateTime,
          bool canBeDeleted = true,
          int customTime = 0,
          string comment = "")
        {
            this.name = name;
            this.deleteCommandAction = deleteCommandAction;
            this.recalculateTime = recalculateTime;
            this.canBeDeleted = canBeDeleted;
            this.customTime = customTime;
            this.comment = comment;
            DeleteCommand = new RelayCommand(DeleteCommandClick);
        }

        public string Name
        {
            get => name;
            set => Set(ref name, value, nameof(Name));
        }

        public bool CanBeDeleted
        {
            get => canBeDeleted;
            set => Set(ref canBeDeleted, value, nameof(CanBeDeleted));
        }

        public bool IsLogged
        {
            get => isLogged;
            set => Set(ref isLogged, value, nameof(IsLogged));
        }

        public int CustomTime
        {
            get => customTime;

            set
            {
                Set(ref customTime, value, nameof(CustomTime));
                recalculateTime();
            }
        }

        public string Comment
        {
            get => comment;
            set => Set(ref comment, value, nameof(Comment));
        }

        public int TimeInMinutes
        {
            get => timeInMinutes;
            set => Set(ref timeInMinutes, value, nameof(TimeInMinutes));
        }

        public ICommand DeleteCommand { get; set; }

        private void DeleteCommandClick(object o)
        {
            deleteCommandAction(this);
        }
    }
}
