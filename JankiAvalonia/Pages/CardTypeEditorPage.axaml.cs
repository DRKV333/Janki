using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace JankiAvalonia.Pages
{
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