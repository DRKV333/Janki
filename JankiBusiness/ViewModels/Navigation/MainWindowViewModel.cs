using JankiBusiness.Services;
using JankiBusiness.ViewModels.Study;
using JankiCards.Importing;
using JankiCards.Janki.Context;
using System;

namespace JankiBusiness.ViewModels.Navigation
{
    public class MainWindowViewModel : ContentPageViewModel
    {
        private class NavigationService : INavigationService
        {
            private readonly MainWindowViewModel mainWindow;

            public NavigationService(MainWindowViewModel mainWindow) => this.mainWindow = mainWindow;

            public bool NavigateToVM(Type vm, object parameter)
            {
                if (vm == typeof(StudyPageViewModel))
                    mainWindow.SetContent(mainWindow.studyPageViewModel.Value, parameter);
                else if (vm == typeof(DashboardPageViewModel))
                    mainWindow.SetContent(mainWindow.navigationViewModel.Value, parameter);
                else
                    throw new ArgumentException("Invalid VM type.", nameof(vm));

                return true;
            }
        }

        public IJankiContextProvider ContextProvider { get; set; }
        public IDialogService DialogService { get; set; }
        public IMediaImporter MediaImporter { get; set; }

        private readonly INavigationService navigationService;

        private readonly Lazy<NavigationViewModel> navigationViewModel;
        private readonly Lazy<StudyPageViewModel> studyPageViewModel;

        protected override PageViewModel DefaultPage => navigationViewModel.Value;

        public MainWindowViewModel()
        {
            navigationService = new NavigationService(this);

            navigationViewModel = new Lazy<NavigationViewModel>(
                () => new NavigationViewModel()
                {
                    ContextProvider = ContextProvider,
                    DialogService = DialogService,
                    NavigationService = navigationService,
                    MediaImporter = MediaImporter
                });

            studyPageViewModel = new Lazy<StudyPageViewModel>(
                () => new StudyPageViewModel()
                {
                    ContextProvider = ContextProvider,
                    NavigationService = navigationService,
                    DialogService = DialogService
                });
        }
    }
}