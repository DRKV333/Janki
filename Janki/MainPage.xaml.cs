using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Janki
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            FrameNavigationOptions options = new FrameNavigationOptions();
            options.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            options.IsNavigationStackEnabled = false;

            if (args.InvokedItemContainer == DashboardItem)
                ContentFrame.NavigateToType(typeof(DashboardPage), null, options);
            else if (args.InvokedItemContainer == DeckEditorItem)
                ContentFrame.NavigateToType(typeof(DeckEditorPage), null, options);
            else if (args.InvokedItemContainer == CardTypeEditorItem)
                ContentFrame.NavigateToType(typeof(CardTypeEditorPage), null, options);
        }
    }
}