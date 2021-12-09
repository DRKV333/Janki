using JankiBusiness.Services;
using JankiBusiness.Web;
using JankiCards.Importing;
using JankiCards.Janki.Context;
using System;
using System.IO;
using System.Threading.Tasks;

namespace JankiAvalonia.Services
{
    public class MediaManager : IJankiContextProvider, IMediaImporter, IMediaUnimporter, IMediaProvider, ILastSyncTimeAccessor
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

        private string GetMediaPath(string name) => Path.Combine("media", name);

        public Task<Stream?> GetMediaStream(string name)
        {
            string path = GetMediaPath(name);

            if (File.Exists(path))
                return Task.FromResult<Stream?>(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true));
            else
                return Task.FromResult<Stream?>(null);
        }

        public async Task ImportMedia(string name, Stream content)
        {
            Directory.CreateDirectory("media");

            using FileStream fs = new FileStream(GetMediaPath(name), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 4096, true);

            await content.CopyToAsync(fs);
        }

        public Task UnimportMedia(string name)
        {
            string path = GetMediaPath(name);
            if (File.Exists(path))
                File.Delete(path);
            return Task.CompletedTask;
        }

        private const string SyncTimeFile = "LastSyncTime.txt";

        public async Task<DateTime> GetLastSyncTime()
        {
            if (!File.Exists(SyncTimeFile))
                return DateTime.MinValue;
            return DateTime.Parse(await File.ReadAllTextAsync(SyncTimeFile));
        }

        public Task SetLastSyncTime(DateTime time)
        {
            return File.WriteAllTextAsync(SyncTimeFile, time.ToString());
        }
    }
}