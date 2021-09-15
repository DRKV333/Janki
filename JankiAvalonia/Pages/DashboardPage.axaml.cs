using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace JankiAvalonia.Pages
{
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