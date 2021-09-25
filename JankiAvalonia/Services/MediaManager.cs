using LibAnkiCards.Context;
using System.IO;

namespace JankiAvalonia.Services
{
    public class MediaManager : IAnkiContextProvider
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
    }
}