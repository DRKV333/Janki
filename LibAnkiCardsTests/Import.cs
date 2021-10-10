using LibAnkiCards;
using LibAnkiCards.AnkiCompat.Context;
using LibAnkiCards.Importing;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LibAnkiCardsTests
{
    public class Import
    {
        [Test]
        public async Task DatabaseMergeCardTypes()
        {
            using AnkiContext fromContext = AnkiContext.OpenSQLite("samples/collectionMerge.anki2", true);
            using AnkiContext toContext = AnkiContext.OpenSQLite("samples/collectionBig.anki2", true);

            int oldTypeCount = toContext.Collection.CardTypes.Count;

            DatabaseImporter importer = new DatabaseImporter(toContext);
            await importer.Import(fromContext);

            Assert.AreEqual(oldTypeCount + 2, toContext.Collection.CardTypes.Count);
        }

        [Test]
        public async Task DatabaseMigrateEntities()
        {
            using AnkiContext fromContext = AnkiContext.OpenSQLite("samples/collectionMerge.anki2", true);
            using MemoryAnkiContextProvider provider = new MemoryAnkiContextProvider();
            using IAnkiContext toContext = provider.CreateContext();

            int oldNoteCount = toContext.Notes.Count();

            DatabaseImporter importer = new DatabaseImporter(toContext);
            await importer.Import(fromContext);

            await toContext.SaveChangesAsync();

            Assert.AreEqual(oldNoteCount + 3, toContext.Notes.Count());
        }

        [Test]
        public async Task Package()
        {
            using FileStream packageStream = new FileStream("samples/TestDeckSmall.apkg", FileMode.Open, FileAccess.Read);
            using MemoryAnkiContextProvider provider = new MemoryAnkiContextProvider();
            using IAnkiContext toContext = provider.CreateContext();

            MockMediaImporter mediaImporter = new MockMediaImporter();

            PackageImporter packageImporter = new PackageImporter(toContext, mediaImporter);
            await packageImporter.Import(packageStream);
            await toContext.SaveChangesAsync();

            Assert.That(mediaImporter.Imported, Is.EquivalentTo(new[] { "icon.png" }));
            Assert.AreEqual(5, toContext.Notes.Count());
            Assert.AreEqual(6, toContext.Cards.Count());
        }
    }
}