using Windows.Foundation;
using Windows.UI.Xaml;

namespace Janki
{
    public class WebEditBox : MediaWebView
    {
        private bool htmlReady = false;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(WebEditBox), new PropertyMetadata("",
                (d, e) =>
                {
                    WebEditBox sender = (WebEditBox)d;
                    if (sender.htmlReady)
                        sender.SetEditorHtml((string)e.NewValue);
                }));

        public WebEditBoxToolbarCoordinator Coordinator
        {
            get { return (WebEditBoxToolbarCoordinator)GetValue(CoordinatorProperty); }
            set { SetValue(CoordinatorProperty, value); }
        }

        public static readonly DependencyProperty CoordinatorProperty =
            DependencyProperty.Register(nameof(Coordinator), typeof(WebEditBoxToolbarCoordinator), typeof(WebEditBox), new PropertyMetadata(null));

        public WebEditBox()
        {
            Web.ScriptNotify += Web_ScriptNotify;
            SetIndexUri("FieldEditor.html");
        }

        public string BoolString(bool b) => b ? "true" : "false";

        public void Bold(bool value) => SetUnsetFormat("bold", value);

        public void Italic(bool value) => SetUnsetFormat("italic", value);

        public void Underline(bool value) => SetUnsetFormat("underline", value);

        private void SetUnsetFormat(string format, bool value)
        {
            Web.InvokeScriptAsync(format, new[] { BoolString(value) });
            FetchText();
        }

        private void Web_ScriptNotify(object sender, Windows.UI.Xaml.Controls.NotifyEventArgs e)
        {
            if (e.Value == "hello")
            {
                SetEditorHtml(Text);
                htmlReady = true;
            }
            else if (e.Value.StartsWith("text "))
            {
                Text = e.Value.Substring("text ".Length);
            }
            else
            {
                string[] split = e.Value.Split(' ');

                if (split[0] == "height")
                {
                    MinHeight = int.Parse(split[1]);
                }
                else if (split[0] == "format")
                {
                    bool value = bool.Parse(split[2]);
                    switch (split[1])
                    {
                        case "b": Coordinator.SetActualBold(value); break;
                        case "i": Coordinator.SetActualItalic(value); break;
                        case "u": Coordinator.SetActualUnderline(value); break;
                    }
                }
            }
        }

        private void FetchText() => Web.InvokeScriptAsync("notifyText", null);

        private void SetEditorHtml(string text)
        {
            Web.InvokeScriptAsync("setEditorHtml", new[] { text });
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            FetchText();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Coordinator.ActiveBox = this;
            Web.InvokeScriptAsync("notifyAllFormats", null);
        }
    }
}