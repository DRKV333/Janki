using JankiBusiness.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Janki.Pages
{
    public sealed partial class DeckEditorPage : Page
    {
        public DeckEditorPage()
        {
            InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            AddCardFlyout.Hide();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            (DataContext as PageViewModel)?.OnNavigatedTo(e.Parameter);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            (DataContext as PageViewModel)?.OnNavigatedFrom();
        }
    }
}