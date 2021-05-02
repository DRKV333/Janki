using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace Janki
{
    internal class HtmlWebView : MediaWebView
    {
        private string htmlString = "";

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.Register(nameof(Html), typeof(string), typeof(MediaWebView), new PropertyMetadata("",
                (d, e) =>
                {
                    HtmlWebView sender = (HtmlWebView)d;
                    sender.htmlString = (string)e.NewValue;
                    sender.NavigateToIndex();
                }));

        protected override async Task<IInputStream> MediaOverride(string path)
        {
            // This is the only way to put a string into a WebView using IUriToStreamResolver.
            // A managed MemoryStream wrapped into an IInputStream will not work!

            if (path == IndexUri.AbsolutePath)
            {
                InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();

                using (var dataWriter = new DataWriter(memoryStream))
                {
                    dataWriter.UnicodeEncoding = UnicodeEncoding.Utf8;
                    dataWriter.ByteOrder = ByteOrder.LittleEndian;

                    dataWriter.WriteString(htmlString);
                    await dataWriter.StoreAsync();
                    await dataWriter.FlushAsync();
                    dataWriter.DetachStream();
                }

                return memoryStream.GetInputStreamAt(0);
            }

            return null;
        }
    }
}