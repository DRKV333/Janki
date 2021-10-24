using System.Collections.Generic;

namespace LibAnkiCards.Janki
{
    public class Bundle : EntityBase
    {
        public IList<Card> Cards { get; set; }

        public IList<Deck> Decks { get; set; }

        public IList<CardType> CardTypes { get; set; }

        public IList<Media> Medias { get; set; }
    }
}