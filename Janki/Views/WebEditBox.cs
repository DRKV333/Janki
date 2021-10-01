using JankiBusiness.Web;
using Windows.UI.Xaml;

namespace Janki.Views
{
    public class WebEditBox : MediaWebView
    {
        private readonly WebEditBoxAdapter adapter = new WebEditBoxAdapter();

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
                    sender.adapter.SetText((string)e.NewValue);
                }));

        public WebEditBoxToolbarCoordinator Coordinator
        {
            get { return (WebEditBoxToolbarCoordinator)GetValue(CoordinatorProperty); }
            set { SetValue(CoordinatorProperty, value); }
        }

        public static readonly DependencyProperty CoordinatorProperty =
            DependencyProperty.Register(nameof(Coordinator), typeof(WebEditBoxToolbarCoordinator), typeof(WebEditBox), new PropertyMetadata(null,
                (d, e) =>
                {
                    WebEditBox sender = (WebEditBox)d;
                    sender.adapter.Coordinator = (WebEditBoxToolbarCoordinator)e.NewValue;
                }));

        public WebEditBox()
        {
            Web.ScriptNotify += (s, e) => adapter.OnScriptNotify(e.Value);

            adapter.ScriptInvoked += (s, e) =>
            Web.InvokeScriptAsync(e.Script, e.Arguments.Length == 0 ? null : e.Arguments);
            adapter.MinHeightChanged += (s, e) => MinHeight = e.MinHeight;
            adapter.TextChanged += (s, e) => Text = e.Text;

            SetIndexUri("FieldEditor.html");
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            adapter.Activate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            adapter.FetchText();
            base.OnLostFocus(e);
        }
    }
}