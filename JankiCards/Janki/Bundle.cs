using System.Collections.Generic;

namespace JankiCards.Janki
{
    public class Bundle : EntityBase
    {
        public string Name { get; set; }

        public bool IsPublic { get; set; }

        public IList<Deck> Decks { get; set; }

        public IList<CardType> CardTypes { get; set; }
    }
}