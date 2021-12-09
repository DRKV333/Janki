using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JankiAvalonia.Services;
using JankiBusiness.ViewModels.Web;

namespace JankiAvalonia.Pages
{
    [ViewForPage(typeof(JankiWebPageViewModel))]
    public partial class JankiWebPage : UserControl
    {
        public JankiWebPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}