using JankiCards.Importing;
using JankiCards.Janki.Context;
using System.IO;
using System.Threading.Tasks;

namespace JankiAvalonia.Services
{
    public class MediaManager : IJankiContextProvider, IMediaImporter
    {
        public JankiContext CreateContext()
        {
            string path = "collection.janki2";

            bool creating = !File.Exists(path);

            JankiContext context = JankiContext.OpenSQLite(path);

            if (creating)
                context.Database.EnsureCreated();

            return context;
        }

        public async Task ImportMedia(string name, Stream content)
        {
            Directory.CreateDirectory("media");

            using FileStream fs = new FileStream(Path.Combine("media", name), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 4096, true);
            
            await content.CopyToAsync(fs);
        }
    }
}