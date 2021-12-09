using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JankiAvalonia.Services;
using JankiBusiness.ViewModels.Web;

namespace JankiAvalonia.Pages
{
    [ViewForPage(typeof(BundlePageViewModel))]
    public partial class BundlePage : UserControl
    {
        public BundlePage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}