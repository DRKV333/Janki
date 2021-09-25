using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JankiAvalonia.Services;
using JankiBusiness.ViewModels.CardTypeEditor;

namespace JankiAvalonia.Pages
{
    [ViewForPage(typeof(CardTypeEditorPageViewModel))]
    public partial class CardTypeEditorPage : UserControl
    {
        public CardTypeEditorPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}