using LibAnkiCards;
using LibAnkiCards.Context;
using LibAnkiScheduler;

namespace JankiBusiness
{
    public class StudiableDeckViewModel : ViewModel
    {
        private readonly IAnkiContextProvider contextProvider;
        private readonly Deck deck;

        public string Name => deck.Name;

        public int NewCount { get; private set; }
        public int DueCount { get; private set; }
        public int ReviewCount { get; private set; }

        public StudiableDeckViewModel(IAnkiContextProvider contextProvider, Deck deck)
        {
            this.contextProvider = contextProvider;
            this.deck = deck;
        }

        public void ResetCounts(IScheduler scheduler)
        {
            scheduler.SetSelectedDeck(deck);
            scheduler.Reset();

            NewCount = scheduler.NewCount;
            RaisePropertyChanged(nameof(NewCount));
            DueCount = scheduler.DueCount;
            RaisePropertyChanged(nameof(DueCount));
            ReviewCount = scheduler.ReviewCount;
            RaisePropertyChanged(nameof(ReviewCount));
        }
    }
}