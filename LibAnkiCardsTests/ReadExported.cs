using LibAnkiCards;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace LibAnkiCardsTests
{
    public class ReadExported
    {
        [Test]
        public void FindNoteSimple()
        {
            using AnkiContext context = AnkiContext.OpenSQLite("samples/collectionSmall.anki2", true);

            Assert.AreEqual(1, context.Collections.Count());
            Collection collection = context.Collections.First();

            Deck deck = collection.Decks.FirstOrDefault(x => x.Value.Name == "TestDeckSmall").Value;
            Assert.IsNotNull(deck);

            List<Card> cards = deck.GetCards(context).Include(x => x.Note).ToList();
            Assert.AreEqual(6, cards.Count);

            Card card = cards.FirstOrDefault(x => x.Note.ShortField == "Front");
            Assert.IsNotNull(card);

            CardType type = card.Note.GetCardType(context);
            Assert.That(type.Name == "CustomCardTypeTest");
        }

        [Test]
        public void FindReviewsSimple()
        {
            using AnkiContext context = AnkiContext.OpenSQLite("samples/collectionBig.anki2", true);

            Card card = context.Cards.Single(x => x.Id == 1556384731399);
            context.Entry(card).Collection(x => x.Reviews).Load();

            Assert.AreEqual(5, card.Reviews.Count);
            Assert.AreEqual(2, card.Reviews.Count(x => x.Ease == 1));
            Assert.AreEqual(2, card.Reviews.Count(x => x.Ease == 2));
            Assert.AreEqual(1, card.Reviews.Count(x => x.Ease == 3));
        }

        [Test]
        public void RepsIsReviewCount()
        {
            using AnkiContext context = AnkiContext.OpenSQLite("samples/collectionBig.anki2", true);

            Assert.That(context.Cards.All(x => x.Reps == x.Reviews.Count));
        }
    }
}