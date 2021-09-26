using JankiBusiness.Abstraction;
using JankiBusiness.Services;
using LibAnkiCards;
using LibAnkiCards.Context;
using LibAnkiCards.Importing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.DeckEditor
{
    public class DeckEditorPageViewModel : PageViewModel
    {
        public IAnkiContextProvider ContextProvider { get; set; }
        public IDialogService DialogService { get; set; }
        public IMediaImporter MediaImporter { get; set; }

        public ObservableCollection<DeckViewModel> Decks { get; } = new ObservableCollection<DeckViewModel>();

        public CardAdderViewModel CardAdderViewModel { get; }

        private DeckViewModel selectedDeck;

        public DeckViewModel SelectedDeck
        {
            get => selectedDeck;
            set { selectedDeck?.SetSearchTerm(""); Set(ref selectedDeck, value); }
        }

        private NoteViewModel selectedCard;

        public NoteViewModel SelectedCard
        {
            get => selectedCard;
            set
            {
                SelectedDeck?.SaveCard.ExecuteAsync(selectedCard);
                Set(ref selectedCard, value);
            }
        }

        private string searchTerm = "";

        public string SearchTerm
        {
            get => searchTerm;
            set => Set(ref searchTerm, value);
        }

        public GenericCommand DeleteSelectedCard { get; }

        public GenericCommand AddDeck { get; }

        public GenericCommand DeleteSelectedDeck { get; }

        public GenericCommand Import { get; }

        public GenericCommand Search { get; }

        public DeckEditorPageViewModel()
        {
            CardAdderViewModel = new CardAdderViewModel(this);

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

                    long id = collection.Decks.Any() ? collection.Decks.Max(x => x.Key) + 1 : 0;

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

            Import = new GenericDelegateCommand(async p =>
            {
                using (Stream sourceFile = await DialogService.OpenFile(".apkg", ".colpkg"))
                {
                    if (sourceFile == null)
                        return;

                    using (IAnkiContext context = ContextProvider.CreateContext())
                    {
                        PackageImporter importer = new PackageImporter(context, MediaImporter);
                        await importer.Import(sourceFile);
                        context.Collection = context.Collection; //TODO: Fix this, this is stupid!
                        await context.SaveChangesAsync();
                    }
                }
            });

            Search = new GenericDelegateCommand(p => SelectedDeck.SetSearchTerm(SearchTerm));
        }

        public override async Task OnNavigatedTo(object param)
        {
            searchTerm = "";

            using (IAnkiContext context = ContextProvider.CreateContext())
            {
                List<DeckViewModel> decks = await Task.Run(() => context.Collection.Decks.Select(x => new DeckViewModel(ContextProvider, x.Value)).ToList());
                
                Decks.Clear();
                foreach (var item in decks)
                {
                    Decks.Add(item);
                }

                CardAdderViewModel.LoadTypes(context.Collection);
            }
        }

        public override Task OnNavigatedFrom() => SelectedDeck?.SaveCard.ExecuteAsync(SelectedCard) ?? Task.CompletedTask;
    }
}