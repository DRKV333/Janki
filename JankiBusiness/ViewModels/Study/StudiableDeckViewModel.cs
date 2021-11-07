using JankiBusiness.Services;
using JankiScheduler;
using JankiCards.Janki;

namespace JankiBusiness.ViewModels.Study
{
    public class StudiableDeckViewModel : ViewModel
    {
        private readonly Deck deck;

        public string Name => deck.Name;

        public StudyCountsViewModel Counts { get; } = new StudyCountsViewModel();

        public StudiableDeckViewModel(Scheduler scheduler, Deck deck)
        {
            this.deck = deck;

            scheduler.SelectDeck(deck).Wait();
            Counts.FillCounts(scheduler);
        }

        public void NavigateTo(INavigationService navigation)
        {
            navigation.NavigateToVM(typeof(StudyPageViewModel), deck.Id);
        }
    }
}