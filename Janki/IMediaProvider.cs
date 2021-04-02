using System.IO;
using System.Threading.Tasks;

namespace Janki
{
    public interface IMediaProvider
    {
        Task<Stream> GetMediaStream(string name);
    }
}