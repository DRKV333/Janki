using JankiBusiness.Abstraction;
using JankiBusiness.Services;
using JankiBusiness.ViewModels.DeckEditor;
using JankiBusiness.Web;
using JankiScheduler;
using LibAnkiCards.Janki;
using LibAnkiCards.Janki.Context;
using System;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.Study
{
    public class StudyPageViewModel : PageViewModel
    {
        public IJankiContextProvider ContextProvider { get; set; }
        public INavigationService NavigationService { get; set; }
        public IDialogService DialogService { get; set; }

        private Lazy<WebEditBoxToolbarCoordinator> coordinator;
        public WebEditBoxToolbarCoordinator Coordinator => coordinator.Value;

        private Deck deck;
        private IScheduler scheduler;

        private bool answersVisible = false;

        public bool AnswersVisible
        {
            get => answersVisible;
            set => Set(ref answersVisible, value);
        }

        private bool editorVisible = false;

        public bool EditorVisible
        {
            get => editorVisible;
            set => Set(ref editorVisible, value);
        }

        private CardStudyData currentCard;
        private CardViewModel currentCardVM;

        private bool flipped = false;

        public string Html => flipped ? currentCardVM?.BackHtml : currentCardVM?.FrontHtml;

        public NoteViewModel CurrentNote => currentCardVM?.Card;

        public StudyCountsViewModel Counts { get; } = new StudyCountsViewModel();

        public GenericCommand Flip { get; }

        public GenericCommand Answer { get; }

        public GenericCommand Back { get; }

        public StudyPageViewModel()
        {
            Flip = new GenericDelegateCommand(p =>
            {
                flipped = !flipped;
                AnswersVisible = true;
                RaisePropertyChanged(nameof(Html));
                return Task.CompletedTask;
            });

            Answer = new GenericDelegateCommand(async p =>
            {
                int ease = int.Parse((string)p);
                scheduler.AnswerCard(currentCard, ease);
                await FetchNextCard();
            });

            Back = new GenericDelegateCommand(p =>
            {
                NavigationService.NavigateToVM(typeof(DashboardPageViewModel), null);
                return Task.CompletedTask;
            });

            coordinator = new Lazy<WebEditBoxToolbarCoordinator>(() => new WebEditBoxToolbarCoordinator() { DialogService = DialogService });
        }

        public override async Task OnNavigatedTo(object param)
        {
            long deckId = (long)param;

            using (JankiContext context = ContextProvider.CreateContext())
            {
                deck = await context.Decks.FindAsync(deckId);
            }

            // TODO: Create scheduler if necessary, set active deck and reset, update counts.

            await FetchNextCard();
        }

        public override async Task OnNavigatedFrom()
        {
            if (currentCardVM != null)
            {
                using (JankiContext context = ContextProvider.CreateContext())
                {
                    currentCardVM.Card.SaveChanges(context);
                    await context.SaveChangesAsync();
                }
            }

            currentCard = null;
            currentCardVM = null;
        }

        private async ValueTask FetchNextCard()
        {
            AnswersVisible = false;
            EditorVisible = false;

            currentCard = scheduler.GetCard();

            if (currentCard == null)
            {
                NavigationService.NavigateToVM(typeof(DashboardPageViewModel), null);
                return;
            }

            using (JankiContext context = ContextProvider.CreateContext())
            {
                context.CardStudyDatas.Attach(currentCard);
                await context.Entry(currentCard).Reference(x => x.Card).LoadAsync();

                if (currentCardVM != null)
                {
                    currentCardVM.Card.SaveChanges(context);
                    await context.SaveChangesAsync();
                }

                currentCardVM = new CardViewModel(currentCard.Variant, currentCard.Card);

                currentCardVM.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CardViewModel.FrontHtml) || e.PropertyName == nameof(CardViewModel.BackHtml))
                        RaisePropertyChanged(nameof(Html));
                };
            }

            flipped = false;

            RaisePropertyChanged(nameof(Html));
            RaisePropertyChanged(nameof(CurrentNote));

            Counts.FillCounts(scheduler);
        }
    }
}