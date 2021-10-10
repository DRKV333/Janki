using LibAnkiCards.AnkiCompat;
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

        void AnswerCard(Card card, int ease);

        int NewCount { get; }

        int DueCount { get; }

        int ReviewCount { get; }
    }
}