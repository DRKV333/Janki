using System.IO;
using System.Threading.Tasks;

namespace JankiCards.Importing
{
    public interface IMediaImporter
    {
        Task ImportMedia(string name, Stream content);
    }
}