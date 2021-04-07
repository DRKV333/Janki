using System.Threading.Tasks;

namespace JankiBusiness
{
    public abstract class GenericCommand : ViewModel
    {
        private bool canExecute = true;

        public bool CanExecute
        {
            get => canExecute;
            set => Set(ref canExecute, value);
        }

        public abstract Task ExecuteAsync(object parameter);
    }
}