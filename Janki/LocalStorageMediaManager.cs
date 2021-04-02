using LibAnkiCards.Context;
using LibAnkiCards.Importing;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Janki
{
    internal class LocalStorageMediaManager : IMediaImporter, IAnkiContextProvider, IMediaProvider
    {
        private StorageFolder mediaFolder;

        public async Task ImportMedia(string name, Stream content)
        {
            StorageFile file = await (await GetMediaFolder()).CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

            using (Stream fileStream = await file.OpenStreamForWriteAsync())
            {
                await content.CopyToAsync(fileStream);
            }
        }

        public IAnkiContext CreateContext()
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "collection.anki2");

            bool creating = !File.Exists(path);

            AnkiContext context = AnkiContext.OpenSQLite(path);

            if (creating)
                context.Database.EnsureCreated();

            return context;
        }

        public async Task<Stream> GetMediaStream(string name) => await (await (await GetMediaFolder()).GetFileAsync(name)).OpenStreamForReadAsync();

        private async ValueTask<StorageFolder> GetMediaFolder()
        {
            if (mediaFolder == null)
                mediaFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("media", CreationCollisionOption.OpenIfExists);

            return mediaFolder;
        }
    }
}