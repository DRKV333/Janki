using JankiBusiness.Abstraction;
using JankiBusiness.Services;
using JankiTransfer.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.Web
{
    public class BundlePageViewModel : PageViewModel
    {
        public class DeckWithSelection : ViewModel
        {
            private bool selected;

            public bool Selected
            {
                get => selected;
                set => Set(ref selected, value);
            }

            private DeckTreeModel deck;

            public DeckTreeModel Deck
            {
                get => deck;
                set => Set(ref deck, value);
            }
        }

        private readonly JankiWebClient jankiWebClient;

        private IList<BundleModel> bundles;

        public IList<BundleModel> Bundles
        {
            get => bundles;
            set => Set(ref bundles, value);
        }

        private IList<DeckWithSelection> decks;

        public IList<DeckWithSelection> Decks
        {
            get => decks;
            set => Set(ref decks, value);
        }

        private string bundleName;

        public string BundleName
        {
            get => bundleName;
            set => Set(ref bundleName, value);
        }

        private BundleModel selectedBundle;

        public BundleModel SelectedBundle
        {
            get => selectedBundle;
            set => Set(ref selectedBundle, value);
        }

        public BundlePageViewModel(JankiWebClient jankiWebClient, INavigationService navigationService)
        {
            this.jankiWebClient = jankiWebClient;

            Publish = new GenericDelegateCommand(async p =>
            {
                await jankiWebClient.PublishBundle(Decks.Where(x => x.Selected).Select(x => x.Deck.Id).ToList(), BundleName);
                await OnNavigatedTo(null);
            });

            Import = new GenericDelegateCommand(async p =>
            {
                if (SelectedBundle != null)
                {
                    await jankiWebClient.ImportBundle(SelectedBundle.Id);
                    await OnNavigatedTo(null);
                }
            });

            Other = new GenericDelegateCommand(p =>
            {
                navigationService.NavigateToVM(typeof(SyncPageViewModel), null);
                return Task.CompletedTask;
            });
        }

        public GenericCommand Publish { get; }

        public GenericCommand Import { get; }

        public GenericCommand Other { get; }

        public override async Task OnNavigatedTo(object param)
        {
            Bundles = await jankiWebClient.GetPublicBundles();
            Decks = (await jankiWebClient.GetAllDecks()).Select(x => new DeckWithSelection() { Deck = x }).ToList();
        }
    }
}