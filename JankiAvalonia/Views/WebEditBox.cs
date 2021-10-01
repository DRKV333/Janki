using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using JankiBusiness.Web;

namespace JankiAvalonia.Views
{
    internal class WebEditBox : MediaWebView
    {
        public static readonly DirectProperty<WebEditBox, WebEditBoxToolbarCoordinator?> CoordinatorProperty =
            AvaloniaProperty.RegisterDirect<WebEditBox, WebEditBoxToolbarCoordinator?>(nameof(Coordinator),
                x => x.Coordinator,
                (x, v) => x.Coordinator = v);

        private WebEditBoxToolbarCoordinator? coordinator;

        public WebEditBoxToolbarCoordinator? Coordinator
        {
            get => coordinator;
            set { if (SetAndRaise(CoordinatorProperty, ref coordinator, value)) adapter.Coordinator = value; }
        }

        public static readonly DirectProperty<WebEditBox, string?> TextProperty =
            AvaloniaProperty.RegisterDirect<WebEditBox, string?>(nameof(Text),
                x => x.Text,
                (x, v) => x.Text = v,
                null, BindingMode.TwoWay);

        private string? text;

        public string? Text
        {
            get => text;
            set { if (SetAndRaise(TextProperty, ref text, value)) adapter.SetText(value); }
        }

        private readonly WebEditBoxAdapter adapter = new WebEditBoxAdapter();

        public WebEditBox()
        {
            adapter.MinHeightChanged += (s, e) => Height = e.MinHeight;
            adapter.TextChanged += (s, e) => SetAndRaise(TextProperty, ref text, e.Text);
            adapter.ScriptInvoked += (s, e) =>
            {
                // The arguments are strings, but ExecuteScriptFunction is very dumb.
                // It will literally just string.Join the function name and the arguments,
                ExecuteScriptFunctionWithSerializedParams(e.Script, e.Arguments);
            };

            RegisterJavascriptObject("external", new ExternalNotifier(adapter));

            NavigateToLocal("fieldeditor.html");
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            adapter.Activate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            adapter.FetchText();
            base.OnLostFocus(e);
        }

        private class ExternalNotifier
        {
            private readonly WebEditBoxAdapter adapter;

            public ExternalNotifier(WebEditBoxAdapter adapter)
            {
                this.adapter = adapter;
            }

            public void notify(string message)
            {
                Dispatcher.UIThread.Post(() => adapter.OnScriptNotify(message));
            }
        }
    }
}