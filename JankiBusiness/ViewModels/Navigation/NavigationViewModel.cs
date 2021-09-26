using JankiBusiness.Abstraction;
using JankiBusiness.Services;
using JankiBusiness.ViewModels.CardTypeEditor;
using JankiBusiness.ViewModels.DeckEditor;
using JankiBusiness.ViewModels.Study;
using LibAnkiCards.Context;
using LibAnkiCards.Importing;
using System;
using System.Collections.Generic;

namespace JankiBusiness.ViewModels.Navigation
{
    public class NavigationViewModel : ContentPageViewModel
    {
        public IAnkiContextProvider ContextProvider { get; set; }
        public IDialogService DialogService { get; set; }
        public INavigationService NavigationService { get; set; }
        public IMediaImporter MediaImporter { get; set; }

        private readonly Lazy<List<NavigationItem>> navigationItems;
        public IReadOnlyList<NavigationItem> NavigationItems => navigationItems.Value;

        private NavigationItem selectedItem;
        public NavigationItem SelectedItem
        {
            get => selectedItem;
            private set => Set(ref selectedItem, value);
        }

        public GenericCommand NavigateToItem { get; }

        protected override PageViewModel DefaultPage
        {
            get
            {
                SelectedItem = NavigationItems[0];
                return NavigationItems[0].Content;
            }
        }

        public NavigationViewModel()
        {
            navigationItems = new Lazy<List<NavigationItem>>(
                () => new List<NavigationItem>()
                {
                    new NavigationItem("Dashboard", "Home", new DashboardPageViewModel()
                    {
                        ContextProvider = ContextProvider,
                        NavigationService = NavigationService
                    }),
                    new NavigationItem("Deck Editor", "CalendarDay", new DeckEditorPageViewModel()
                    {
                        ContextProvider = ContextProvider,
                        DialogService = DialogService,
                        MediaImporter = MediaImporter
                    }),
                    new NavigationItem("Card Type Editor", "PreviewLink", new CardTypeEditorPageViewModel()
                    {
                        Provider = ContextProvider,
                        DialogService = DialogService
                    })
                });

            NavigateToItem = new GenericDelegateCommand(x =>
            {
                SelectedItem = (NavigationItem)x;
                return SetContent(SelectedItem.Content, null);
            });
        }
    }
}