using LibAnkiCards;
using LibAnkiCards.Context;
using LibAnkiScheduler;
using NUnit.Framework;
using System.Linq;

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

            for (int i = 0; i < 3; i++)
            {
                Card card = scheduler.GetCard();
                Assert.NotNull(card);
            }
        }
    }
}