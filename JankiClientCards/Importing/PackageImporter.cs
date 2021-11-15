using JankiCards.AnkiCompat.Context;
using JankiCards.Importing;
using JankiCards.Janki;
using JankiCards.Janki.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace JankiClientCards.Importing
{
    public class PackageImporter
    {
        private readonly IJankiContextProvider provider;
        private readonly IMediaImporter mediaImporter;

        public PackageImporter(IJankiContextProvider provider, IMediaImporter mediaImporter)
        {
            this.provider = provider;
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
                    await ImportDatabase(fromContext).ConfigureAwait(false);
                }
            }
            finally
            {
                tempFile.Delete();
            }
        }

        private async Task ImportDatabase(AnkiContext from)
        {
            JankiCards.AnkiCompat.Deck theNullDeck = new JankiCards.AnkiCompat.Deck() { Name = "Undefined" };

            using (JankiContext to = provider.CreateContext())
            {
                Dictionary<JankiCards.AnkiCompat.CardType, CardType> importedTypes = new Dictionary<JankiCards.AnkiCompat.CardType, CardType>();
                Dictionary<JankiCards.AnkiCompat.Deck, Deck> importedDecks = new Dictionary<JankiCards.AnkiCompat.Deck, Deck>();

                List<JankiCards.AnkiCompat.Note> notes = await from.Notes.Include(x => x.Cards).ToListAsync();

                foreach (var note in notes)
                {
                    JankiCards.AnkiCompat.CardType cardType = note.GetCardType(from);
                    if (!importedTypes.TryGetValue(cardType, out CardType newCardType))
                    {
                        newCardType = new CardType()
                        {
                            Fields = cardType.Fields.Select((x, i) => new CardFieldType()
                            {
                                Name = x.Name,
                                Order = i
                            }).ToList(),
                            Css = cardType.Css,
                            Variants = cardType.Variants.Select(x => new VariantType()
                            {
                                FrontFormat = x.FrontFormat,
                                BackFormat = x.BackFormat,
                                Name = x.Name
                            }).ToList(),
                            Name = cardType.Name,
                            Tags = string.Join(" ", cardType.Tags)
                        };
                        importedTypes.Add(cardType, newCardType);
                        to.CardTypes.Add(newCardType);
                    }

                    JankiCards.AnkiCompat.Deck oldDeck = note.Cards.FirstOrDefault()?.GetDeck(from);
                    if (oldDeck == null)
                        oldDeck = theNullDeck;

                    if (!importedDecks.TryGetValue(oldDeck, out Deck newDeck))
                    {
                        newDeck = new Deck()
                        {
                            Name = oldDeck.Name,
                            StudyData = new DeckStudyData()
                        };
                        importedDecks.Add(oldDeck, newDeck);
                        to.Decks.Add(newDeck);
                    }

                    Card card = new Card()
                    {
                        CardType = newCardType,
                        Deck = newDeck,
                        Fields = note.Fields.Select((x, i) => new CardField()
                        {
                            CardFieldType = newCardType.Fields.ElementAtOrDefault(i),
                            Content = x
                        }).ToList(),
                        Tags = note.Tags
                    };
                    to.TheCards.Add(card);

                    foreach (var item in newCardType.Variants)
                    {
                        to.CardStudyDatas.Add(new CardStudyData()
                        {
                            Card = card,
                            Variant = item
                        });
                    }
                }

                await to.SaveChangesAsync();
            }
        }
    }
}