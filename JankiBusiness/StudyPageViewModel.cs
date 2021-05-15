using LibAnkiCards;
using LibAnkiCards.Context;
using LibAnkiScheduler;
using System.Threading.Tasks;

namespace JankiBusiness
{
    public class StudyPageViewModel : PageViewModel
    {
        public IAnkiContextProvider ContextProvider { get; set; }
        public INavigationService NavigationService { get; set; }

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

        private Card currentCard;
        private CardViewModel currentCardVM;

        private bool flipped = false;

        public string Html => flipped ? currentCardVM?.BackHtml : currentCardVM?.FrontHtml;

        public NoteViewModel CurrentNote => currentCardVM?.Note;

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
        }

        public override async Task OnNavigatedTo(object param)
        {
            long deckId = (long)param;

            using (IAnkiContext context = ContextProvider.CreateContext())
            {
                deck = context.Collection.Decks[deckId];
            }

            if (scheduler == null)
            {
                scheduler = await Task.Run(() => new PythonScheduler(ContextProvider));
            }

            scheduler.SetSelectedDeck(deck);
            scheduler.Reset();

            Counts.FillCounts(scheduler);

            await FetchNextCard();
        }

        public override async Task OnNavigatedFrom()
        {
            if (currentCardVM != null)
            {
                using (IAnkiContext context = ContextProvider.CreateContext())
                {
                    currentCardVM.Note.SaveChanges(context);
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

            using (IAnkiContext context = ContextProvider.CreateContext())
            {
                context.Cards.Attach(currentCard);
                await context.Entry(currentCard).Reference(x => x.Note).LoadAsync();

                if (currentCardVM != null)
                {
                    currentCardVM.Note.SaveChanges(context);
                    await context.SaveChangesAsync();
                }

                currentCardVM = new CardViewModel(context.Collection, currentCard);

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