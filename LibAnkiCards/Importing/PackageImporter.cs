using LibAnkiCards.AnkiCompat.Context;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace LibAnkiCards.Importing
{
    public class PackageImporter
    {
        private readonly DatabaseImporter databaseImporter;
        private readonly IMediaImporter mediaImporter;

        public PackageImporter(IAnkiContext toContext, IMediaImporter mediaImporter)
        {
            databaseImporter = new DatabaseImporter(toContext);
            this.mediaImporter = mediaImporter;
        }

        public async Task Import(Stream packageStream)
        {
            using (ZipArchive packageArchive = new ZipArchive(packageStream, ZipArchiveMode.Read, true))
            {
                ZipArchiveEntry dbEntry = packageArchive.Entries.SingleOrDefault(x => x.FullName == "collection.anki2");
                if (dbEntry == null)
                    throw new IOException("Package does not contain database.");

                ZipArchiveEntry mediaListEntry = packageArchive.Entries.SingleOrDefault(x => x.FullName == "media");
                if (mediaListEntry == null)
                    throw new IOException("Package does not contain media list.");

                await ImportMedia(packageArchive, mediaListEntry).ConfigureAwait(false);

                await ImportDatabase(dbEntry).ConfigureAwait(false);
            }
        }

        private async Task ImportDatabase(ZipArchiveEntry dbEntry)
        {
            FileInfo tempFile = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

            try
            {
                using (FileStream fs = tempFile.OpenWrite())
                {
                    using (Stream es = dbEntry.Open())
                    {
                        await es.CopyToAsync(fs).ConfigureAwait(false);
                    }
                }

                using (AnkiContext fromContext = AnkiContext.OpenSQLite(tempFile.FullName, true))
                {
                    await databaseImporter.Import(fromContext).ConfigureAwait(false);
                }
            }
            finally
            {
                tempFile.Delete();
            }
        }

        private async Task ImportMedia(ZipArchive packageArchive, ZipArchiveEntry mediaListEntry)
        {
            Dictionary<string, string> mediaList = await ReadMediaList(mediaListEntry).ConfigureAwait(false);

            foreach (var item in mediaList)
            {
                ZipArchiveEntry mediaEntry = packageArchive.Entries.SingleOrDefault(x => x.FullName == item.Key);
                if (mediaEntry == null)
                    throw new IOException($"Media entry {item.Key} not found.");

                using (Stream mediaStream = mediaEntry.Open())
                {
                    await mediaImporter.ImportMedia(item.Value, mediaStream).ConfigureAwait(false);
                }
            }
        }

        private async Task<Dictionary<string, string>> ReadMediaList(ZipArchiveEntry mediaListEntry)
        {
            JsonSerializer serializer = new JsonSerializer();

            using (Stream stream = mediaListEntry.Open())
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                    {
                        return await Task.Run(() => serializer.Deserialize<Dictionary<string, string>>(jsonReader)).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}