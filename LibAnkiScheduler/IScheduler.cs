using LibAnkiCards;
using System.Collections.Generic;

namespace LibAnkiScheduler
{
    public interface IScheduler
    {
        string Name { get; }

        void SetActiveDecks(IEnumerable<Deck> decks);

        void SetSelectedDeck(Deck deck);

        void Reset();

        Card GetCard();
    }
}