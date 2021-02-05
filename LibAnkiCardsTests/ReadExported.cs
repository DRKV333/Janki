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
            Assert.That(cards.Any(x => x.Note.ShortField == "Front"));
        }
    }
}