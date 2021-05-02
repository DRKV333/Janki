using LibAnkiCards.Context;
using LibAnkiCards.Importing;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Janki
{
    internal class LocalStorageMediaManager : IMediaImporter, IAnkiContextProvider
    {
        private readonly StorageFolderMediaProvider media = new StorageFolderMediaProvider(ApplicationData.Current.LocalFolder, "media", true);
        private readonly StorageFolderMediaProvider mathjax = new StorageFolderMediaProvider(Package.Current.InstalledLocation, @"Assets\web\mathjax", false);

        public IMediaProvider CardMediaProvider { get; }

        public LocalStorageMediaManager()
        {
            CardMediaProvider = new CompositeMediaProvider(mathjax, media);
        }

        public async Task ImportMedia(string name, Stream content)
        {
            StorageFile file = await (await media.GetMediaFolder()).CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

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
    }
}