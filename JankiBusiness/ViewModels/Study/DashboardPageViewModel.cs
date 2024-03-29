﻿using JankiBusiness.Abstraction;
using JankiBusiness.Services;
using JankiCards.Janki.Context;
using JankiScheduler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.Study
{
    public class DashboardPageViewModel : PageViewModel
    {
        public IJankiContextProvider ContextProvider { get; set; }
        public INavigationService NavigationService { get; set; }

        public ObservableCollection<StudiableDeckViewModel> Decks { get; } = new ObservableCollection<StudiableDeckViewModel>();

        public GenericCommand Study { get; }

        private bool loading = true;

        private Scheduler scheduler;

        public bool Loading
        {
            get => loading;
            set => Set(ref loading, value);
        }

        public DashboardPageViewModel()
        {
            Study = new GenericDelegateCommand(p =>
            {
                ((StudiableDeckViewModel)p).NavigateTo(NavigationService);
                return Task.CompletedTask;
            });
        }

        public override async Task OnNavigatedTo(object param)
        {
            if (scheduler == null)
                scheduler = new Scheduler(ContextProvider);

            Loading = true;

            using (JankiContext context = ContextProvider.CreateContext())
            {
                List<StudiableDeckViewModel> decks = await Task.Run(() => context.Decks.Select(x => new StudiableDeckViewModel(scheduler, x)).ToList());

                Decks.Clear();
                foreach (var item in decks)
                {
                    if (item.Counts.DueCount > 0 || item.Counts.NewCount > 0 || item.Counts.ReviewCount > 0)
                        Decks.Add(item);
                }
            }

            Loading = false;
        }
    }
}