using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JankiAvalonia.Services;
using JankiBusiness.ViewModels.Study;

namespace JankiAvalonia.Pages
{
    [ViewForPage(typeof(StudyPageViewModel))]
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