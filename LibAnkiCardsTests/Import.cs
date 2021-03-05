using LibAnkiCards;
using LibAnkiCards.Importing;
using NUnit.Framework;
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
            using AnkiContext toContext = new MemoryAnkiContext();
            toContext.Database.EnsureCreated();

            int oldNoteCount = toContext.Notes.Count();

            DatabaseImporter importer = new DatabaseImporter(toContext);
            await importer.Import(fromContext);

            await toContext.SaveChangesAsync();

            Assert.AreEqual(oldNoteCount + 3, toContext.Notes.Count());
        }
    }
}