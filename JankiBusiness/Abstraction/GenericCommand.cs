using JankiBusiness.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JankiBusiness.Abstraction
{
    public abstract class GenericCommand : ViewModel, ICommand
    {
        private bool canExecute = true;

        public bool CanExecute
        {
            get => canExecute;
            set { Set(ref canExecute, value); CanExecuteChanged?.Invoke(this, new EventArgs()); }
        }

        public abstract Task ExecuteAsync(object parameter);

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) => ExecuteAsync(parameter);

        bool ICommand.CanExecute(object parameter) => CanExecute;
    }
}