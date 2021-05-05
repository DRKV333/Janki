using System;

namespace JankiBusiness
{
    public interface INavigationService
    {
        bool NavigateToVM(Type vm, object parameter);
    }
}