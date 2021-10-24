using JankiBusiness.ViewModels.DeckEditor;
using LibAnkiCards.Janki;
using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Core.Settings;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.Study
{
    public class CardViewModel : ViewModel
    {
        private static readonly StubbleVisitorRenderer frontRenderer = new StubbleBuilder()
            .Configure(
                x => x.AddValueGetter(typeof(CardViewModel), NoteFieldValueGetter)
            )
            .Build();
        
        private static readonly StubbleVisitorRenderer frontRendererPreview = new StubbleBuilder()
            .Configure(
                x => x.AddValueGetter(typeof(CardViewModel), PreviewValueGetter)
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
        
        private static readonly StubbleVisitorRenderer backRendererPreview = new StubbleBuilder()
            .Configure(
                x => x.AddValueGetter(typeof(CardViewModel), ComposeValueGetter(
                        FrontSideValueGetter,
                        PreviewValueGetter
                     ))
            )
            .Build();

        private static object PreviewValueGetter(object value, string key, bool ignoreCase) => key;

        private static object NoteFieldValueGetter(object value, string key, bool ignoreCase) =>
            ((CardViewModel)value).Card.Fields.FirstOrDefault(
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

        public NoteViewModel Card { get; }
        public CardType Type { get; }
        public VariantType Variant { get; }

        private string frontContent;

        private string frontHtml;

        public string FrontHtml
        {
            get => frontHtml;
            private set => Set(ref frontHtml, value);
        }

        private string backHtml;

        public string BackHtml
        {
            get => backHtml;
            private set => Set(ref backHtml, value);
        }

        private CardViewModel(CardType Type, VariantType Variant)
        {
            this.Type = Type;
            this.Variant = Variant;
            Render();
        }

        public CardViewModel(VariantType Variant, NoteViewModel Card)
        {
            Type = Variant.CardType;
            this.Variant = Variant;
            this.Card = Card;

            Card.PropertyChanged += (s, e) => Render();
            Render();
        }

        public CardViewModel(VariantType Variant, Card Card)
        {
            Type = Variant.CardType;
            this.Variant = Variant;
            this.Card = new NoteViewModel(Card);
            Render();
        }

        public static CardViewModel CreatePreview(CardType type, VariantType variant) => new CardViewModel(type, variant);

        private StubbleVisitorRenderer FrontRenderer => Card == null ? frontRendererPreview : frontRenderer;

        private StubbleVisitorRenderer BackRenderer => Card == null ? backRendererPreview : backRenderer;

        public async Task Render()
        {
            frontContent = await RenderContent(Variant.FrontFormat, FrontRenderer).ConfigureAwait(false);
            FrontHtml = RenderSide(frontContent);
            BackHtml = RenderSide(await RenderContent(Variant.BackFormat, BackRenderer).ConfigureAwait(false));
        }

        private string RenderSide(string content)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<html>");

            builder.Append("<head>");

            if (!Debugger.IsAttached)
                builder.Append("<script id=\"MathJax-script\" async src=\"tex-mml-chtml.js\"></script>");

            builder.Append("<style>");
            builder.Append(Type.Css);
            builder.Append("</style>");

            builder.Append("</head>");

            builder.Append("<body class=\"card\">");
            builder.Append(content);
            builder.Append("</body>");

            builder.Append("</html>");

            return builder.ToString();
        }

        private async ValueTask<string> RenderContent(string template, StubbleVisitorRenderer renderer)
        {
            try
            {
                return await renderer.RenderAsync(template, this, SkipHtmlEncodingSettings);
            }
            catch (Exception e)
            {
                return $"Failed to render card: {e}";
            }
        }
    }
}