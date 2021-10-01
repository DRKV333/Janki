using Avalonia;
using JankiBusiness.Web;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JankiAvalonia.Views
{
    public class HtmlWebView : MediaWebView
    {
        public static readonly DirectProperty<HtmlWebView, string> HtmlProperty =
            AvaloniaProperty.RegisterDirect<HtmlWebView, string>(nameof(Html),
                x => x.Html,
                (x, v) => x.Html = v,
                "");

        private string html = "";

        public string Html
        {
            get => html;
            set { if (SetAndRaise(HtmlProperty, ref html, value)) NavigateToLocal("index"); }
        }

        protected override IMediaProvider? ApplyMediaOverrides(IMediaProvider? provider)
        {
            IMediaProvider mine = new HtmlProvider(this);
            if (provider == null)
                return mine;
            else
                return new CompositeMediaProvider(mine, provider);
        }

        private class HtmlProvider : IMediaProvider
        {
            private readonly HtmlWebView html;

            public HtmlProvider(HtmlWebView html)
            {
                this.html = html;
            }

            public Task<Stream?> GetMediaStream(string name)
            {
                if (name == "index")
                    return Task.FromResult<Stream?>(new MemoryStream(Encoding.UTF8.GetBytes(html.Html)));
                return Task.FromResult<Stream?>(null);
            }
        }
    }
}