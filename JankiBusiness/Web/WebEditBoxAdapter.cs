using System;

namespace JankiBusiness.Web
{
    public class WebEditBoxAdapter
    {
        public class ScriptInvokedEventArgs : EventArgs
        {
            public string Script { get; }
            public string[] Arguments { get; }

            public ScriptInvokedEventArgs(string Script, string[] Arguments)
            {
                this.Script = Script;
                this.Arguments = Arguments;
            }
        }

        public class MinHeightChangedEventArgs : EventArgs
        {
            public int MinHeight { get; }

            public MinHeightChangedEventArgs(int MinHeight)
            {
                this.MinHeight = MinHeight;
            }
        }

        public class TextChangedEventArgs : EventArgs
        {
            public string Text { get; }

            public TextChangedEventArgs(string Text)
            {
                this.Text = Text;
            }
        }

        private bool htmlReady = false;
        private string deferredText = null;

        public WebEditBoxToolbarCoordinator Coordinator { get; set; }

        public event EventHandler<ScriptInvokedEventArgs> ScriptInvoked;

        public event EventHandler<MinHeightChangedEventArgs> MinHeightChanged;

        public event EventHandler<TextChangedEventArgs> TextChanged;

        private static string BoolString(bool b) => b ? "true" : "false";

        public void Bold(bool value) => SetUnsetFormat("bold", value);

        public void Italic(bool value) => SetUnsetFormat("italic", value);

        public void Underline(bool value) => SetUnsetFormat("underline", value);

        public void InsertImage(string src) => ScriptInvoked?.Invoke(this, new ScriptInvokedEventArgs("insertImage", new[] { src }));

        private void SetUnsetFormat(string format, bool value)
        {
            ScriptInvoked?.Invoke(this, new ScriptInvokedEventArgs(format, new[] { BoolString(value) }));
            FetchText();
        }

        public void OnScriptNotify(string value)
        {
            if (value == "hello")
            {
                if (!htmlReady)
                {
                    htmlReady = true;
                    if (deferredText != null)
                    {
                        SetText(deferredText);
                        deferredText = null;
                    }
                }
            }
            else if (value.StartsWith("text "))
            {
                string text = value.Substring("text ".Length);
                TextChanged?.Invoke(this, new TextChangedEventArgs(text));
            }
            else
            {
                string[] split = value.Split(' ');

                if (split[0] == "height")
                {
                    int minHeight = int.Parse(split[1]);
                    MinHeightChanged?.Invoke(this, new MinHeightChangedEventArgs(minHeight));
                }
                else if (split[0] == "format")
                {
                    if (Coordinator == null)
                        return;

                    bool boolValue = bool.Parse(split[2]);
                    switch (split[1])
                    {
                        case "b": Coordinator.SetActualBold(boolValue); break;
                        case "i": Coordinator.SetActualItalic(boolValue); break;
                        case "u": Coordinator.SetActualUnderline(boolValue); break;
                    }
                }
            }
        }

        public void FetchText() => ScriptInvoked?.Invoke(this, new ScriptInvokedEventArgs("notifyText", new string[0]));

        public void SetText(string text)
        {
            if (htmlReady)
                ScriptInvoked?.Invoke(this, new ScriptInvokedEventArgs("setEditorHtml", new[] { text }));
            else
                deferredText = text;
        }

        public void Activate()
        {
            Coordinator.ActiveBox = this;
            ScriptInvoked?.Invoke(this, new ScriptInvokedEventArgs("notifyAllFormats", new string[0]));
            ScriptInvoked?.Invoke(this, new ScriptInvokedEventArgs("notifyHeight", new string[0]));
        }
    }
}