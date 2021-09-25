using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JankiAvalonia.Services;
using JankiBusiness.ViewModels.Navigation;

namespace JankiAvalonia.Pages
{
    [ViewForPage(typeof(NavigationViewModel))]
    public partial class NavigationPage : UserControl
    {
        public NavigationPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}