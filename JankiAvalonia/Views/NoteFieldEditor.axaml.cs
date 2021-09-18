using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace JankiAvalonia.Views
{
    public partial class NoteFieldEditor : UserControl
    {
        public NoteFieldEditor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}