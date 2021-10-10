using JankiBusiness.Web;
using LibAnkiCards.AnkiCompat.Context;
using LibAnkiCards.Importing;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Janki.Services
{
    internal class LocalStorageMediaManager : IMediaImporter, IAnkiContextProvider
    {
        private readonly StorageFolderMediaProvider media = new StorageFolderMediaProvider(ApplicationData.Current.LocalFolder, "media", true);

        public IMediaProvider CardMediaProvider { get; }

        public IMediaProvider FieldEditorMediaProvider { get; }

        public LocalStorageMediaManager()
        {
            CardMediaProvider = new CompositeMediaProvider(ManifestResourceMedaiProvider.MathJax, media);
            FieldEditorMediaProvider = new CompositeMediaProvider(ManifestResourceMedaiProvider.FieldEditor, media);
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