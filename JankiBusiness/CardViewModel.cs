using LibAnkiCards;
using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Core.Settings;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankiBusiness
{
    public class CardViewModel : INotifyPropertyChanged
    {
        private static readonly StubbleVisitorRenderer frontRenderer = new StubbleBuilder()
            .Configure(
                x => x.AddValueGetter(typeof(CardViewModel), NoteFieldValueGetter)
            )
            .Build();

        private static readonly StubbleVisitorRenderer backRenderer = new StubbleBuilder()
            .Configure(
                x => x.AddValueGetter(typeof(CardViewModel), ComposeValueGetter(
                        FrontSideValueGetter,
                        NoteFieldValueGetter
                     ))
            )
            .Build();

        private static object NoteFieldValueGetter(object value, string key, bool ignoreCase) =>
            ((CardViewModel)value).Note.Fields.FirstOrDefault(
                y => StringEquals(key, y.Definition.Name, ignoreCase)
            )?.Value;

        private static object FrontSideValueGetter(object value, string key, bool ignoreCase) =>
            StringEquals(key, "FrontSide", ignoreCase) ? ((CardViewModel)value).frontContent : null;

        private static bool StringEquals(string a, string b, bool ignoreCase) =>
            string.Equals(a, b, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        private static RendererSettingsDefaults.ValueGetterDelegate ComposeValueGetter(params RendererSettingsDefaults.ValueGetterDelegate[] getters) =>
            (v, k, i) =>
            {
                foreach (var item in getters)
                {
                    object result = item(v, k, i);
                    if (result != null)
                        return result;
                }
                return null;
            };

        private static readonly RenderSettings SkipHtmlEncodingSettings = new RenderSettings()
        {
            SkipHtmlEncoding = true
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public NoteViewModel Note { get; }
        public CardVariant Variant { get; }

        private string frontContent;

        private string frontHtml;

        public string FrontHtml
        {
            get => frontHtml;
            private set
            {
                frontHtml = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FrontHtml)));
            }
        }

        private string backHtml;

        public string BackHtml
        {
            get => backHtml;
            private set
            {
                backHtml = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackHtml)));
            }
        }

        public CardViewModel(Collection collection, Card card, NoteViewModel note = null)
        {
            if (note == null)
                Note = new NoteViewModel(collection, card.Note);
            else
                Note = note;

            Variant = card.GetVariant(collection);

            Note.PropertyChanged += (x, y) => Render();

            Render();
        }

        private async void Render()
        {
            frontContent = await RenderContent(Variant.FrontFormat, frontRenderer);
            FrontHtml = RenderSide(frontContent);
            BackHtml = RenderSide(await RenderContent(Variant.BackFormat, backRenderer));
        }

        private string RenderSide(string content)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<html>");

            builder.Append("<head>");

            //TODO: Include MathJax in app package and serve that instead.
            builder.Append("<script src=\"https://polyfill.io/v3/polyfill.min.js?features=es6\"></script>");
            builder.Append("<script id=\"MathJax-script\" async src=\"https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js\"></script>");

            builder.Append("<style>");
            builder.Append(Note.Type.Css);
            builder.Append("</style>");

            builder.Append("</head>");

            builder.Append("<body class=\"card\">");
            builder.Append(content);
            builder.Append("</body>");

            builder.Append("</html>");

            return builder.ToString();
        }

        private ValueTask<string> RenderContent(string template, StubbleVisitorRenderer renderer)
        {
            try
            {
                return renderer.RenderAsync(template, this, SkipHtmlEncodingSettings);
            }
            catch (Exception e)
            {
                return new ValueTask<string>($"Failed to render card: {e}");
            }
        }
    }
}