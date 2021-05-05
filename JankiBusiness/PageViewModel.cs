using System.Threading.Tasks;

namespace JankiBusiness
{
    public abstract class PageViewModel : ViewModel
    {
        public virtual Task OnNavigatedTo(object param) => Task.CompletedTask;

        public virtual Task OnNavigatedFrom() => Task.CompletedTask;
    }
}