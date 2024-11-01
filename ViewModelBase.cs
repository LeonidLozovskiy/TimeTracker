// Decompiled with JetBrains decompiler
// Type: TimeTracker.Common.ViewModelBase
// Assembly: TimeTracker.Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0F11B473-0BE4-4486-8918-EA14CB4A070C
// Assembly location: C:\TimeTracker\TimeTracker.Common.dll

using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace TimeTracker
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(ref T field, T newValue = default, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged == null)
            {
                return;
            }

            propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
