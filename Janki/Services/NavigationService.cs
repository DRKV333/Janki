using Janki.Pages;
using JankiBusiness.Services;
using JankiBusiness.ViewModels.Study;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Janki.Services
{
    public class NavigationService : INavigationService
    {
        public Frame Frame { get; set; }

        public Dictionary<Type, Type> VmToPage => vmToPage;

        private readonly Dictionary<Type, Type> vmToPage = new Dictionary<Type, Type>()
        {
            [typeof(StudyPageViewModel)] = typeof(StudyPage),
            [typeof(DashboardPageViewModel)] = typeof(MainPage)
        };

        public bool NavigateToVM(Type vm, object parameter)
        {
            return Frame.NavigateToType(VmToPage[vm], parameter, new FrameNavigationOptions() { IsNavigationStackEnabled = false });
        }
    }
}