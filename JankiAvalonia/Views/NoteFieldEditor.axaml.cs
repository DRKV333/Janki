using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace JankiAvalonia.Views
{
    public partial class NoteFieldEditor : UserControl
    {
        public static readonly DirectProperty<NoteFieldEditor, bool> IsDynamicOverflowEnabledProperty = 
            AvaloniaProperty.RegisterDirect<NoteFieldEditor, bool>(nameof(IsDynamicOverflowEnabled),
                x => x.IsDynamicOverflowEnabled,
                (x, v) => x.IsDynamicOverflowEnabled = v,
                true);

        private bool isDynamicOverflowEnabled = true;

        public bool IsDynamicOverflowEnabled
        {
            get => isDynamicOverflowEnabled;
            set => SetAndRaise(IsDynamicOverflowEnabledProperty, ref isDynamicOverflowEnabled, value);
        }

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