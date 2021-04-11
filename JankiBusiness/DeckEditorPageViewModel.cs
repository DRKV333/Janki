using LibAnkiCards.Context;
using System.Collections.ObjectModel;
using System.Linq;

namespace JankiBusiness
{
    public class DeckEditorPageViewModel : ViewModel
    {
        public IAnkiContextProvider ContextProvider { get; set; }
        public IConfirmationDialogService ConfirmationDialogService { get; set; }

        private ObservableCollection<DeckViewModel> decks;

        public ObservableCollection<DeckViewModel> Decks
        {
            get
            {
                if (decks == null)
                    Init();
                return decks;
            }
        }

        private CardAdderViewModel cardAdderViewModel;

        public CardAdderViewModel CardAdderViewModel
        {
            get
            {
                if (cardAdderViewModel == null)
                    Init();
                return cardAdderViewModel;
            }
        }

        private DeckViewModel selectedDeck;

        public DeckViewModel SelectedDeck
        {
            get => selectedDeck;
            set => Set(ref selectedDeck, value);
        }

        private NoteViewModel selectedCard;

        public NoteViewModel SelectedCard
        {
            get => selectedCard;
            set => Set(ref selectedCard, value);
        }

        public GenericCommand DeleteSelectedCard { get; }

        public DeckEditorPageViewModel()
        {
            DeleteSelectedCard = new GenericDelegateCommand(async p =>
            {
                if (SelectedCard == null)
                    return;

                if (await ConfirmationDialogService.ShowDialog(
                    "Delete Card",
                    $"Are you sure you want to delete \"{SelectedCard.ShortField}\"?",
                    "Delete", "Cancel"))
                {
                    using (IAnkiContext context = ContextProvider.CreateContext())
                    {
                        SelectedCard.Delete(context);
                        await context.SaveChangesAsync();
                    }

                    SelectedDeck.Cards.Remove(SelectedCard);
                    SelectedCard = null;
                }
            });
        }

        private void Init()
        {
            using (IAnkiContext context = ContextProvider.CreateContext())
            {
                decks = new ObservableCollection<DeckViewModel>(context.Collection.Decks.Select(x => new DeckViewModel(ContextProvider, x.Value)));
                cardAdderViewModel = new CardAdderViewModel(context.Collection, this);
            }
        }
    }
}