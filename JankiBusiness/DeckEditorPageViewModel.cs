using LibAnkiCards;
using LibAnkiCards.Context;
using System.Collections.ObjectModel;
using System.Linq;

namespace JankiBusiness
{
    public class DeckEditorPageViewModel : ViewModel
    {
        public IAnkiContextProvider ContextProvider { get; set; }
        public IDialogService DialogService { get; set; }

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

        public GenericCommand AddDeck { get; }

        public GenericCommand DeleteSelectedDeck { get; }

        public DeckEditorPageViewModel()
        {
            DeleteSelectedCard = new GenericDelegateCommand(async p =>
            {
                if (SelectedCard == null)
                    return;

                if (await DialogService.ShowConfirmationDialog(
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

            AddDeck = new GenericDelegateCommand(async p =>
            {
                string name = await DialogService.ShowTextPromptDialog("Deck Name", "", true);

                if (name == null)
                    return;

                using (IAnkiContext context = ContextProvider.CreateContext())
                {
                    Collection collection = context.Collection;

                    long id = collection.Decks.Any() ? collection.Decks.Max(x => x.Key + 1) : 0;

                    Deck deck = new Deck()
                    {
                        ConfigurationId = collection.DeckConfigurations.First().Key,
                        Id = id,
                        Name = name
                    };

                    collection.Decks.Add(id, deck);
                    context.Collection = collection;

                    await context.SaveChangesAsync();

                    DeckViewModel deckVM = new DeckViewModel(ContextProvider, deck);
                    Decks.Add(deckVM);
                    SelectedDeck = deckVM;
                }
            });

            DeleteSelectedDeck = new GenericDelegateCommand(async p =>
            {
                if (SelectedDeck == null)
                    return;

                if (await DialogService.ShowConfirmationDialog(
                    "Delete Deck",
                    $"Are you sure you want to delete \"{SelectedDeck.Name}\" and all {SelectedDeck.Cards.Count} cards in it?",
                    "Delete", "Cancel"))
                {
                    await SelectedDeck.Delete();

                    Decks.Remove(SelectedDeck);
                    SelectedDeck = null;
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