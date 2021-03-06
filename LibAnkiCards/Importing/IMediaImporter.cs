using System.IO;
using System.Threading.Tasks;

namespace LibAnkiCards.Importing
{
    public interface IMediaImporter
    {
        Task ImportMedia(string name, Stream content);
    }
}