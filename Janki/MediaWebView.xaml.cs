using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web;

namespace Janki
{
    public partial class MediaWebView : UserControl
    {
        protected WebView Web => web;

        protected Uri IndexUri { get; private set; }
        private readonly Resolver resolver;

        public IMediaProvider MediaProvider
        {
            get { return (IMediaProvider)GetValue(MediaProviderProperty); }
            set { SetValue(MediaProviderProperty, value); }
        }

        public static readonly DependencyProperty MediaProviderProperty =
            DependencyProperty.Register(nameof(MediaProvider), typeof(IMediaProvider), typeof(MediaWebView), new PropertyMetadata(null,
                (d, e) =>
                {
                    MediaWebView sender = (MediaWebView)d;
                    sender.resolver.Provider = (IMediaProvider)e.NewValue;
                    sender.NavigateToIndex();
                }));

        public MediaWebView()
        {
            resolver = new Resolver(this);
            InitializeComponent();
            SetIndexUri("index");
        }

        protected void SetIndexUri(string uri)
        {
            IndexUri = web.BuildLocalStreamUri(Guid.NewGuid().ToString(), uri);
            NavigateToIndex();
        }

        protected void NavigateToIndex()
        {
            web.NavigateToLocalStreamUri(IndexUri, resolver);
        }

        protected virtual Task<IInputStream> MediaOverride(string path) => Task.FromResult<IInputStream>(null);

        private class Resolver : IUriToStreamResolver
        {
            private readonly MediaWebView mediaWebView;
            public IMediaProvider Provider { get; set; }

            public Resolver(MediaWebView mediaWebView)
            {
                this.mediaWebView = mediaWebView;
            }

            public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
            {
                if (uri == null)
                    throw new ArgumentNullException(nameof(uri));
                return GetContent(uri.AbsolutePath).AsAsyncOperation();
            }

            private async Task<IInputStream> GetContent(string path)
            {
                IInputStream localStream = await mediaWebView.MediaOverride(path);
                if (localStream != null)
                    return localStream;

                return (await Provider.GetMediaStream(path.TrimStart('/'))).AsInputStream();
            }
        }
    }
}