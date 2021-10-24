using JankiBusiness.Services;
using LibAnkiCards.Janki;

namespace JankiBusiness.ViewModels.Study
{
    public class StudiableDeckViewModel : ViewModel
    {
        private readonly Deck deck;

        public string Name => deck.Name;

        public StudyCountsViewModel Counts { get; } = new StudyCountsViewModel();

        public StudiableDeckViewModel(Deck deck)
        {
            this.deck = deck;

            // TODO: Update counts from scheduler.
        }

        public void NavigateTo(INavigationService navigation)
        {
            navigation.NavigateToVM(typeof(StudyPageViewModel), deck.Id);
        }
    }
}