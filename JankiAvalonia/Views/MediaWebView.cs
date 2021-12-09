using Avalonia;
using JankiBusiness.Web;
using System.IO;
using WebViewControl;

namespace JankiAvalonia.Views
{
    public class MediaWebView : WebView
    {
        public static readonly DirectProperty<MediaWebView, IMediaProvider?> ProviderProperty =
            AvaloniaProperty.RegisterDirect<MediaWebView, IMediaProvider?>(nameof(Provider),
                x => x.Provider,
                (x, v) => x.Provider = v);

        private const string LocalPrefix = "http://local.local/";

        private IMediaProvider? provider;

        public IMediaProvider? Provider
        {
            get => provider;
            set { if (SetAndRaise(ProviderProperty, ref provider, value)) actualProvider = ApplyMediaOverrides(value); }
        }

        private IMediaProvider? actualProvider;

        protected virtual IMediaProvider? ApplyMediaOverrides(IMediaProvider? provider) => provider;

        public MediaWebView()
        {
            BeforeResourceLoad += MediaWebView_BeforeResourceLoad;
            actualProvider = ApplyMediaOverrides(null);
            NavigateToLocal("index");
        }

        public void NavigateToLocal(string uri)
        {
            LoadUrl(LocalPrefix + uri);
        }

        private void MediaWebView_BeforeResourceLoad(ResourceHandler resourceHandler)
        {
            Stream? stream = actualProvider?.GetMediaStream(resourceHandler.Url.Replace(LocalPrefix, "").TrimStart('/').TrimEnd('/')).Result;

            if (stream != null)
            {
                resourceHandler.RespondWith(stream);
            }
        }
    }
}