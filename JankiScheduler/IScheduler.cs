using JankiCards.Janki;
using System.Collections.Generic;

namespace JankiScheduler
{
    public interface IScheduler
    {
        string Name { get; }

        void SetActiveDecks(IEnumerable<Deck> decks);

        void SetSelectedDeck(Deck deck);

        void Reset();

        CardStudyData GetCard();

        void AnswerCard(CardStudyData card, int ease);

        int NewCount { get; }

        int DueCount { get; }

        int ReviewCount { get; }
    }
}