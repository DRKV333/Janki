using JankiBusiness.Services;
using JankiCards.Janki.Context;
using System;

namespace JankiBusiness.ViewModels.Web
{
    public class JankiWebPageViewModel : ContentPageViewModel
    {
        private class NavigationService : INavigationService
        {
            private readonly JankiWebPageViewModel webPage;

            public NavigationService(JankiWebPageViewModel webPage) => this.webPage = webPage;

            public bool NavigateToVM(Type vm, object parameter)
            {
                if (vm == typeof(LoginPageViewModel))
                    webPage.SetContent(webPage.loginPage.Value, parameter);
                else if (vm == typeof(SyncPageViewModel))
                    webPage.SetContent(webPage.syncPage.Value, parameter);
                else if (vm == typeof(BundlePageViewModel))
                    webPage.SetContent(webPage.bundlePage.Value, parameter);

                return true;
            }
        }

        public ILastSyncTimeAccessor LastSyncTimeAccessor { get; set; }
        public IJankiContextProvider ContextProvider { get; set; }

        private readonly JankiWebClient client = new JankiWebClient();

        private readonly Lazy<LoginPageViewModel> loginPage;
        private readonly Lazy<SyncPageViewModel> syncPage;
        private readonly Lazy<BundlePageViewModel> bundlePage;

        protected override PageViewModel DefaultPage => loginPage.Value;

        public JankiWebPageViewModel()
        {
            NavigationService navigation = new NavigationService(this);

            loginPage = new Lazy<LoginPageViewModel>(() => new LoginPageViewModel(client, navigation));
            syncPage = new Lazy<SyncPageViewModel>(() => new SyncPageViewModel(LastSyncTimeAccessor, ContextProvider, client, navigation));
            bundlePage = new Lazy<BundlePageViewModel>(() => new BundlePageViewModel(client, navigation));
        }
    }
}