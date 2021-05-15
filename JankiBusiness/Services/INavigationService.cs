using System;

namespace JankiBusiness.Services
{
    public interface INavigationService
    {
        bool NavigateToVM(Type vm, object parameter);
    }
}