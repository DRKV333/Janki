using JankiBusiness.Abstraction;
using JankiBusiness.ViewModels.Study;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.DeckEditor
{
    public class CardCarouselViewModel : ViewModel
    {
        private int selectedIndex = 0;

        private CardViewModel selectedCard;

        public CardViewModel SelectedCard
        {
            get => selectedCard;
            private set => Set(ref selectedCard, value);
        }

        public GenericCommand Previous { get; }

        public GenericCommand Next { get; }

        public CardCarouselViewModel(IList<CardViewModel> cards)
        {
            SelectedCard = cards[0];

            Previous = new GenericDelegateCommand(o =>
            {
                if (selectedIndex > 0)
                {
                    selectedIndex--;
                    SelectedCard = cards[selectedIndex];
                }

                return Task.CompletedTask;
            });

            Next = new GenericDelegateCommand(o =>
            {
                if (selectedIndex < cards.Count - 1)
                {
                    selectedIndex++;
                    SelectedCard = cards[selectedIndex];
                }

                return Task.CompletedTask;
            });
        }
    }
}