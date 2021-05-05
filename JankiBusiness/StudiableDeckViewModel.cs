using LibAnkiCards;
using LibAnkiScheduler;

namespace JankiBusiness
{
    public class StudiableDeckViewModel : ViewModel
    {
        private readonly Deck deck;

        public string Name => deck.Name;

        public StudyCountsViewModel Counts { get; } = new StudyCountsViewModel();

        public StudiableDeckViewModel(IScheduler scheduler, Deck deck)
        {
            this.deck = deck;

            scheduler.SetSelectedDeck(deck);
            scheduler.Reset();

            Counts.FillCounts(scheduler);
        }

        public void NavigateTo(INavigationService navigation)
        {
            navigation.NavigateToVM(typeof(StudyPageViewModel), deck.Id);
        }
    }
}