using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JankiAvalonia.Services;
using JankiBusiness.ViewModels.Study;

namespace JankiAvalonia.Pages
{
    [ViewForPage(typeof(DashboardPageViewModel))]
    public partial class DashboardPage : UserControl
    {
        public DashboardPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}