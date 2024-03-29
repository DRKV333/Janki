﻿using JankiBusiness.Services;
using JankiBusiness.Web;
using JankiCards.Importing;
using JankiCards.Janki.Context;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Janki.Services
{
    internal class LocalStorageMediaManager : IMediaImporter, IMediaUnimporter, IJankiContextProvider
    {
        private readonly StorageFolderMediaProvider media = new StorageFolderMediaProvider(ApplicationData.Current.LocalFolder, "media", true);

        public IMediaProvider CardMediaProvider { get; }

        public IMediaProvider FieldEditorMediaProvider { get; }

        public LocalStorageMediaManager()
        {
            CardMediaProvider = new CompositeMediaProvider(ManifestResourceMediaProvider.MathJax, media);
            FieldEditorMediaProvider = new CompositeMediaProvider(ManifestResourceMediaProvider.FieldEditor, media);
        }

        public async Task ImportMedia(string name, Stream content)
        {
            StorageFile file = await (await media.GetMediaFolder()).CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

            using (Stream fileStream = await file.OpenStreamForWriteAsync())
            {
                await content.CopyToAsync(fileStream);
            }
        }

        public JankiContext CreateContext()
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "collection.janki2");

            bool creating = !File.Exists(path);

            JankiContext context = JankiContext.OpenSQLite(path);

            if (creating)
                context.Database.EnsureCreated();

            return context;
        }

        public async Task UnimportMedia(string name)
        {
            try
            {
                await (await (await media.GetMediaFolder()).GetFileAsync(name)).DeleteAsync();
            }
            catch (Exception)
            {
                // This is not really a problem.
            }
        }
    }
}