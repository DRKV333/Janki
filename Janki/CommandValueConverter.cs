using JankiBusiness;
using System;
using System.Windows.Input;
using Windows.UI.Xaml.Data;

namespace Janki
{
    public class CommandValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new WrapperCommand((GenericCommand)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private class WrapperCommand : ICommand
        {
            private readonly GenericCommand command;

            public WrapperCommand(GenericCommand command)
            {
                this.command = command;
                command.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(GenericCommand.CanExecute))
                        CanExecuteChanged?.Invoke(s, e);
                };
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => command.CanExecute;

            public void Execute(object parameter) => command.ExecuteAsync(parameter);
        }
    }
}