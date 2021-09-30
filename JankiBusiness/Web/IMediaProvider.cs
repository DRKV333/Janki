using System.IO;
using System.Threading.Tasks;

namespace JankiBusiness.Web
{
    public interface IMediaProvider
    {
        Task<Stream> GetMediaStream(string name);
    }
}