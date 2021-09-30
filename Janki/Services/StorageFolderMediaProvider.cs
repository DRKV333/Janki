using JankiBusiness.Web;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Janki.Services
{
    internal class StorageFolderMediaProvider : IMediaProvider
    {
        private readonly StorageFolder root;
        private readonly string path;
        private readonly bool create;

        private StorageFolder mediaFolder;

        public StorageFolderMediaProvider(StorageFolder root, string path, bool create)
        {
            this.root = root;
            this.path = path;
            this.create = create;
        }

        public async Task<Stream> GetMediaStream(string name)
        {
            name = name.Replace('/', '\\');
            StorageFolder folder = await GetMediaFolder();
            if (!((await folder.TryGetItemAsync(name)) is IStorageFile item))
                return null;
            return await item.OpenStreamForReadAsync();
        }

        public async ValueTask<StorageFolder> GetMediaFolder()
        {
            if (mediaFolder == null)
            {
                if (create)
                    mediaFolder = await root.CreateFolderAsync(path, CreationCollisionOption.OpenIfExists);
                else
                    mediaFolder = await root.GetFolderAsync(path);
            }

            return mediaFolder;
        }
    }
}