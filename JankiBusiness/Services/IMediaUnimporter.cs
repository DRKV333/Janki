using System.Threading.Tasks;

namespace JankiBusiness.Services
{
    public interface IMediaUnimporter
    {
        Task UnimportMedia(string name);
    }
}