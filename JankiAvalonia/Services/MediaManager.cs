using LibAnkiCards.Context;
using LibAnkiCards.Importing;
using System.IO;
using System.Threading.Tasks;

namespace JankiAvalonia.Services
{
    public class MediaManager : IAnkiContextProvider, IMediaImporter
    {
        public IAnkiContext CreateContext()
        {
            string path = "collection.anki2";

            bool creating = !File.Exists(path);

            AnkiContext context = AnkiContext.OpenSQLite(path);

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