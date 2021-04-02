using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Janki
{
    public sealed partial class CardRenderer : UserControl
    {
        public CardRenderer()
        {
            InitializeComponent();
        }

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.Register(nameof(Html), typeof(string), typeof(CardRenderer), new PropertyMetadata("",
                (d, e) => ((CardRenderer)d).web.NavigateToString((string)e.NewValue)));
    }
}