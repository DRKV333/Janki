using LibAnkiCards;
using LibAnkiCards.Context;
using LibAnkiCards.Importing;
using LibAnkiCardsTests;
using LibAnkiScheduler;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace LibAnkiSchedulerTests
{
    public class Simple
    {
        [Test]
        public void Name()
        {
            PythonScheduler scheduler = new PythonScheduler(new SQLiteAnkiContextProvider("samples/collectionSmall.anki2", true));

            Assert.AreEqual("std2", scheduler.Name);
        }

        [Test]
        public void Reset()
        {
            IAnkiContextProvider contextProvider = new SQLiteAnkiContextProvider("samples/collectionSmall.anki2", true);
            PythonScheduler scheduler = new PythonScheduler(contextProvider);

            using (IAnkiContext context = contextProvider.CreateContext())
            {
                scheduler.SetSelectedDeck(context.Collection.Decks.First().Value);
            }
            scheduler.Reset();

            Assert.Pass();
        }

        [Test]
        public void GetCard()
        {
            IAnkiContextProvider contextProvider = new SQLiteAnkiContextProvider("samples/collectionBig.anki2", true);
            PythonScheduler scheduler = new PythonScheduler(contextProvider);

            using (IAnkiContext context = contextProvider.CreateContext())
            {
                scheduler.SetSelectedDeck(context.Collection.Decks[1611747606148]);
            }

            Card card = null;

            for (int i = 0; i < 3; i++)
            {
                card = scheduler.GetCard();
                Assert.NotNull(card);
            }
        }

        [Test]
        public async Task AnswerCard()
        {
            using MemoryAnkiContextProvider provider = new MemoryAnkiContextProvider();

            PythonScheduler scheduler;

            using (IAnkiContext context = provider.CreateContext())
            {
                using (IAnkiContext fromContext = AnkiContext.OpenSQLite("samples/collectionBig.anki2", true))
                {
                    DatabaseImporter importer = new DatabaseImporter(context);
                    await importer.Import(fromContext);
                }

                context.SaveChanges();

                scheduler = new PythonScheduler(provider);
                scheduler.SetSelectedDeck(context.Collection.Decks.Single(x => x.Value.Name == "4000 Essential English Words::1.Book").Value);
            }

            Card card = scheduler.GetCard();
            int count = 0;

            while (card != null)
            {
                scheduler.AnswerCard(card, 3);
                count++;
                card = scheduler.GetCard();
            }

            Assert.Pass($"Count was: {count}");
        }
    }
}