using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace JankiAvalonia.Pages
{
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