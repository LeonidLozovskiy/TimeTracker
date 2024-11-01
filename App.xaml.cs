using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.ViewModelEntities;

namespace TimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() : base()
        {
            ServiceCollection services = new();
            this.Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            this.ConfigureServices(services);
            this.ServiceProvider = services.BuildServiceProvider();
            
        }

        public IServiceProvider ServiceProvider { get; }
        public IConfiguration Configuration { get; private set; }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(TimeTracker.MainWindow));
            services.AddSingleton(typeof(MainWindowViewModel));
            Settings.Authorization = this.Configuration.GetSection("Settings")["Authorization"];
            Settings.AdministrativeTask = this.Configuration.GetSection("Settings")["AdministrativeTask"];
            Settings.Jira = this.Configuration.GetSection("Settings")["Jira"];

            
            ////this.ServiceProvider.GetService<TimeTracker.MainWindow>().Show();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.Configuration = new ConfigurationBuilder().Build();
            ServiceProviderFactory.SetContainer(this.ServiceProvider);
            var mainWindow = this.ServiceProvider.GetRequiredService<TimeTracker.MainWindow>();
            mainWindow.Show();
        }

        //protected void Run(object sender, StartupEventArgs startupEventArgs)
        //{
        //    this.Configuration = new ConfigurationBuilder().Build();
        //    ServiceProviderFactory.SetContainer(this.ServiceProvider);
        //    this.ServiceProvider.GetService<TimeTracker.MainWindow>().Show();
        //}
    }
}
