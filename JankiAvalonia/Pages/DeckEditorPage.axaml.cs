using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JankiAvalonia.Services;
using JankiBusiness.ViewModels.DeckEditor;

namespace JankiAvalonia.Pages
{
    [ViewForPage(typeof(DeckEditorPageViewModel))]
    public partial class DeckEditorPage : UserControl
    {
        public DeckEditorPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}