using System;
using System.Threading.Tasks;

namespace JankiBusiness.Abstraction
{
    public class GenericDelegateCommand : GenericCommand
    {
        private readonly Func<object, Task> action;

        public GenericDelegateCommand(Func<object, Task> action)
        {
            this.action = action;
        }

        public override Task ExecuteAsync(object parameter) => action(parameter);
    }
}