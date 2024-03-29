﻿using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Janki.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DashboardItem.IsSelected = true;
            DeckEditorItem.IsSelected = false;
            CardTypeEditorItem.IsSelected = false;

            ContentFrame.NavigateToType(typeof(DashboardPage), null, FrameNavOptions());
        }

        private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            FrameNavigationOptions options = FrameNavOptions();
            options.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;

            if (args.InvokedItemContainer == DashboardItem)
                ContentFrame.NavigateToType(typeof(DashboardPage), null, options);
            else if (args.InvokedItemContainer == DeckEditorItem)
                ContentFrame.NavigateToType(typeof(DeckEditorPage), null, options);
            else if (args.InvokedItemContainer == CardTypeEditorItem)
                ContentFrame.NavigateToType(typeof(CardTypeEditorPage), null, options);
        }

        private FrameNavigationOptions FrameNavOptions() => new FrameNavigationOptions()
        {
            IsNavigationStackEnabled = false
        };
    }
}