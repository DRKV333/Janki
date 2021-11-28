using JankiBusiness.Abstraction;
using JankiBusiness.Services;
using JankiBusiness.Web;
using JankiCards.Importing;
using JankiCards.Janki;
using JankiCards.Janki.Context;
using JankiClientCards.Importing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.DeckEditor
{
    public class DeckEditorPageViewModel : PageViewModel
    {
        public IJankiContextProvider ContextProvider { get; set; }
        public IDialogService DialogService { get; set; }
        public IMediaImporter MediaImporter { get; set; }
        public IMediaUnimporter MediaUnimporter { get; set; }

        private readonly Lazy<WebEditBoxToolbarCoordinator> coordinator;
        public WebEditBoxToolbarCoordinator Coordinator => coordinator.Value;

        private readonly Lazy<PackageImporter> packageImporter;

        public ObservableCollection<DeckViewModel> Decks { get; } = new ObservableCollection<DeckViewModel>();

        public CardAdderViewModel CardAdderViewModel { get; }

        private DeckViewModel selectedDeck;

        public DeckViewModel SelectedDeck
        {
            get => selectedDeck;
            set { selectedDeck?.SetSearchTerm(""); Set(ref selectedDeck, value); }
        }

        private CardViewModel selectedCard;

        public CardViewModel SelectedCard
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
            packageImporter = new Lazy<PackageImporter>(() => new PackageImporter(ContextProvider, MediaImporter));

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
                    using (JankiContext context = ContextProvider.CreateContext())
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

                using (JankiContext context = ContextProvider.CreateContext())
                {
                    Deck deck = new Deck()
                    {
                        Name = name,
                        StudyData = new DeckStudyData()
                    };

                    context.Decks.Add(deck);
                    await context.SaveChangesAsync();

                    DeckViewModel deckVM = new DeckViewModel(ContextProvider, MediaUnimporter, deck);
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

                    await packageImporter.Value.Import(sourceFile);
                }

                await OnNavigatedTo(null);
            });

            Search = new GenericDelegateCommand(p => SelectedDeck.SetSearchTerm(SearchTerm));

            coordinator = new Lazy<WebEditBoxToolbarCoordinator>(() => new WebEditBoxToolbarCoordinator()
            {
                DialogService = DialogService,
                Importer = MediaImporter
            });
        }

        public override async Task OnNavigatedTo(object param)
        {
            searchTerm = "";

            using (JankiContext context = ContextProvider.CreateContext())
            {
                List<Deck> decks = await context.Decks.ToListAsync();
                
                Decks.Clear();
                foreach (var item in decks)
                {
                    Decks.Add(new DeckViewModel(ContextProvider, MediaUnimporter, item));
                }

                CardAdderViewModel.LoadTypes(
                    await context.CardTypes
                        .Include(x => x.Fields)
                        .Include(x => x.Variants)
                        .ToListAsync());
            }
        }

        public override Task OnNavigatedFrom() => SelectedDeck?.SaveCard.ExecuteAsync(SelectedCard) ?? Task.CompletedTask;
    }
}