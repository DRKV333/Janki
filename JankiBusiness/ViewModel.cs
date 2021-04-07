using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JankiBusiness
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        protected void Set<T>(ref T field, T value, [CallerMemberName] string caller = "")
        {
            field = value;
            RaisePropertyChanged(caller);
        }
    }
}