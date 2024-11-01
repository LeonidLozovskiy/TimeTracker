// Decompiled with JetBrains decompiler
// Type: TimeTracker.MainWindow
// Assembly: TimeTracker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 171B3FE6-2A84-4D57-B757-BB4CEB626B4D
// Assembly location: C:\TimeTracker\TimeTracker.dll

using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Markup;
using MahApps.Metro.Controls;

#nullable disable
namespace TimeTracker
{
    public partial class MainWindow : MetroWindow, IComponentConnector
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
            this.DataContext = _viewModel;
            this.InitializeComponent();
            foreach (var calendarDateRange in new List<CalendarDateRange>(this._viewModel.CalendarHistoryDates
                .Where<DateTime>(x => x.Date != DateTime.Today.Date)
                .Select<DateTime, CalendarDateRange>(x => new CalendarDateRange(x))))
            {
                try
                {
                    this.Test.BlackoutDates.Add(calendarDateRange);
                }
                catch
                {
                    Console.WriteLine(calendarDateRange);
                }
            }
        }

        public static string GetPropertyDisplayName(object descriptor)
        {
            if (descriptor is PropertyDescriptor propertyDescriptor)
            {
                if (propertyDescriptor.Attributes[typeof(DisplayNameAttribute)] is DisplayNameAttribute attribute
                    && attribute != DisplayNameAttribute.Default)
                {
                    return attribute.DisplayName;
                }
            }
            else
            {
                var propertyInfo = descriptor as PropertyInfo;
                if (propertyInfo != null)
                {
                    foreach (var customAttribute in propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true))
                    {
                        if (customAttribute is DisplayNameAttribute displayNameAttribute
                            && displayNameAttribute != DisplayNameAttribute.Default)
                        {
                            return displayNameAttribute.DisplayName;
                        }
                    }
                }
            }

            return null;
        }
    }
}
