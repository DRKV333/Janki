using System.IO;
using System.Threading.Tasks;

namespace JankiBusiness.Web
{
    public class CompositeMediaProvider : IMediaProvider
    {
        private readonly IMediaProvider[] providers;

        public CompositeMediaProvider(params IMediaProvider[] providers)
        {
            this.providers = providers;
        }

        public async Task<Stream> GetMediaStream(string name)
        {
            foreach (var item in providers)
            {
                Stream stream = await item.GetMediaStream(name);
                if (stream != null)
                    return stream;
            }
            return null;
        }
    }
}