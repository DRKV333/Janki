using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JankiAvalonia.Services;
using JankiBusiness.ViewModels.Web;

namespace JankiAvalonia.Pages
{
    [ViewForPage(typeof(LoginPageViewModel))]
    public partial class LoginPage : UserControl
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}