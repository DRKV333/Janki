using LibAnkiCards.Context;
using LibAnkiScheduler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace JankiBusiness
{
    public class DashboardPageViewModel : PageViewModel
    {
        public IAnkiContextProvider ContextProvider { get; set; }

        public ObservableCollection<StudiableDeckViewModel> Decks { get; } = new ObservableCollection<StudiableDeckViewModel>();

        public override async Task OnNavigatedTo(object param)
        {
            using (IAnkiContext context = ContextProvider.CreateContext())
            {
                List<StudiableDeckViewModel> decks = await Task.Run(() => context.Collection.Decks.Select(x => new StudiableDeckViewModel(ContextProvider, x.Value)).ToList());

                IScheduler scheduler = await Task.Run(() => new PythonScheduler(ContextProvider));

                Decks.Clear();
                foreach (var item in decks)
                {
                    item.ResetCounts(scheduler);

                    if (item.DueCount > 0 || item.NewCount > 0 || item.ReviewCount > 0)
                        Decks.Add(item);
                }
            }
        }
    }
}