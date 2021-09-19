using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace JankiAvalonia.Pages
{
    public partial class StudyPage : UserControl
    {
        public StudyPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
