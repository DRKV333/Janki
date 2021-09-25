using Avalonia;
using Avalonia.Data;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using System;

namespace JankiAvalonia.Behaviours
{
    public class TextEditorBindingBehaviour : Behavior<TextEditor>
    {
        public static readonly DirectProperty<TextEditorBindingBehaviour, string> TextProperty =
            AvaloniaProperty.RegisterDirect<TextEditorBindingBehaviour, string>(nameof(Text),
                x => x.Text,
                (x, v) => x.Text = v,
                "",
                BindingMode.TwoWay);

#pragma warning disable S3963 // "static" fields should be initialized inline
        static TextEditorBindingBehaviour()
        {
            TextProperty.Changed.Subscribe(x =>
            {
                TextEditorBindingBehaviour behaviour = (TextEditorBindingBehaviour)x.Sender;
                if (!behaviour.handling && behaviour.AssociatedObject != null)
                {
                    behaviour.handling = true;
                    behaviour.AssociatedObject.Text = x.NewValue.Value;
                    behaviour.handling = false;
                }
            });
        }
#pragma warning restore S3963 // "static" fields should be initialized inline

        private bool handling = false;

        private string text = "";
        public string Text
        {
            get => text;
            set => SetAndRaise(TextProperty, ref text, value);
        }

        protected override void OnAttached()
        {
            if (AssociatedObject != null)
                AssociatedObject.TextChanged += AssociatedObject_TextChanged;
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
                AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
        }

        private void AssociatedObject_TextChanged(object? sender, EventArgs e)
        {
            if (!handling && AssociatedObject != null)
            {
                handling = true;
                Text = AssociatedObject.Text;
                handling = false;
            }
        }
    }
}