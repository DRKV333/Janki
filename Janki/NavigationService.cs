using JankiBusiness;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Janki
{
    public class NavigationService : INavigationService
    {
        public Frame Frame { get; set; }

        private readonly Dictionary<Type, Type> vmToPage = new Dictionary<Type, Type>()
        {
            [typeof(StudyPageViewModel)] = typeof(StudyPage),
            [typeof(DashboardPageViewModel)] = typeof(MainPage)
        };

        public bool NavigateToVM(Type vm, object parameter)
        {
            return Frame.NavigateToType(vmToPage[vm], parameter, new FrameNavigationOptions() { IsNavigationStackEnabled = false });
        }
    }
}