using JankiBusiness.Abstraction;
using JankiBusiness.Services;
using JankiCards.Janki.Context;
using JankiTransfer.ChangeDetection;
using JankiTransfer.DTO;
using System;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.Web
{
    public class SyncPageViewModel : PageViewModel
    {
        private readonly ILastSyncTimeAccessor lastSyncTime;
        private readonly IJankiContextProvider contextProvider;
        private readonly JankiWebClient jankiWebClient;

        public SyncPageViewModel(ILastSyncTimeAccessor lastSyncTime, IJankiContextProvider contextProvider, JankiWebClient jankiWebClient, INavigationService navigationService)
        {
            this.lastSyncTime = lastSyncTime;
            this.contextProvider = contextProvider;
            this.jankiWebClient = jankiWebClient;

            KeepLocal = new GenericDelegateCommand(p =>
            {
                if (SelectedCollition != null)
                {
                    SelectedCollition.Remote.Remove();
                    SelectedCollition = null;
                    RaisePropertyChanged(nameof(Changes));
                }
                return Task.CompletedTask;
            });

            TakeRemote = new GenericDelegateCommand(p =>
            {
                if (SelectedCollition != null)
                {
                    SelectedCollition.Local.Remove();
                    SelectedCollition = null;
                    RaisePropertyChanged(nameof(Changes));
                }
                return Task.CompletedTask;
            });

            Sync = new GenericDelegateCommand(async p =>
            {
                if (changes.Collitions.Count == 0)
                {
                    using (JankiContext context = contextProvider.CreateContext())
                    {
                        await detector.ApplyChanges(remoteChanges, Guid.Empty, context);
                        await context.SaveChangesAsync();
                    }

                    await jankiWebClient.PostSync(localChages);

                    await lastSyncTime.SetLastSyncTime(DateTime.UtcNow + TimeSpan.FromSeconds(5));

                    await OnNavigatedTo(null);
                }
            });

            Other = new GenericDelegateCommand(p =>
            {
                navigationService.NavigateToVM(typeof(BundlePageViewModel), null);
                return Task.CompletedTask;
            });
        }

        private readonly ChangeDetector<JankiContext> detector = new ChangeDetector<JankiContext>(new JankiChangeContext());

        private ChangeData localChages;
        private ChangeData remoteChanges;

        private ChangeCollitions changes;

        public ChangeCollitions Changes
        {
            get => changes;
            set => Set(ref changes, value);
        }

        private ChangeCollitions.Collition selectedCollition;

        public ChangeCollitions.Collition SelectedCollition
        {
            get => selectedCollition;
            set => Set(ref selectedCollition, value);
        }

        public GenericCommand KeepLocal { get; }

        public GenericCommand TakeRemote { get; }

        public GenericCommand Sync { get; }

        public GenericCommand Other { get; }

        public override async Task OnNavigatedTo(object param)
        {
            DateTime lastTime = await lastSyncTime.GetLastSyncTime();

            using (JankiContext context = contextProvider.CreateContext())
            {
                localChages = await detector.DetectChanges(lastTime, context);
            }

            remoteChanges = await jankiWebClient.GetSync(lastTime);

            Changes = new ChangeCollitions(localChages, remoteChanges);
        }
    }
}