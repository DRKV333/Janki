using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JankiAvalonia.Services;
using JankiBusiness.ViewModels.Web;

namespace JankiAvalonia.Pages
{
    [ViewForPage(typeof(SyncPageViewModel))]
    public partial class SyncPage : UserControl
    {
        public SyncPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
