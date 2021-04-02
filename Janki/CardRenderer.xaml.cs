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
    public sealed partial class CardRenderer : UserControl
    {
        private const string IndexPath = "/index";

        private readonly Resolver resolver = new Resolver();
        private readonly Uri index;

        public CardRenderer()
        {
            InitializeComponent();
            index = web.BuildLocalStreamUri("local", IndexPath);
        }

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.Register(nameof(Html), typeof(string), typeof(CardRenderer), new PropertyMetadata("",
                (d, e) =>
                {
                    CardRenderer sender = (CardRenderer)d;
                    sender.resolver.Html = (string)e.NewValue;
                    sender.web.NavigateToLocalStreamUri(sender.index, sender.resolver);
                }));

        public IMediaProvider MediaProvider
        {
            get { return (IMediaProvider)GetValue(MediaProviderProperty); }
            set { SetValue(MediaProviderProperty, value); }
        }

        public static readonly DependencyProperty MediaProviderProperty =
            DependencyProperty.Register(nameof(MediaProvider), typeof(IMediaProvider), typeof(CardRenderer), new PropertyMetadata(null,
                (d, e) =>
                {
                    CardRenderer sender = (CardRenderer)d;
                    sender.resolver.Provider = (IMediaProvider)e.NewValue;
                }));

        private class Resolver : IUriToStreamResolver
        {
            public string Html { get; set; }
            public IMediaProvider Provider { get; set; }

            public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
            {
                if (uri == null)
                    throw new ArgumentNullException(nameof(uri));
                return GetContent(uri.AbsolutePath).AsAsyncOperation();
            }

            private async Task<IInputStream> GetContent(string path)
            {
                if (path == IndexPath)
                {
                    InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();

                    using (var dataWriter = new DataWriter(memoryStream))
                    {
                        dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        dataWriter.ByteOrder = ByteOrder.LittleEndian;

                        dataWriter.WriteString(Html);
                        await dataWriter.StoreAsync();
                        await dataWriter.FlushAsync();
                        dataWriter.DetachStream();
                    }

                    return memoryStream.GetInputStreamAt(0);
                }

                return (await Provider.GetMediaStream(path.TrimStart('/'))).AsInputStream();
            }
        }
    }
}