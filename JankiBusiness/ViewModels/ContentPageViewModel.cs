using System.Threading.Tasks;

namespace JankiBusiness.ViewModels
{
    public class ContentPageViewModel : PageViewModel
    {
        private bool inited = false;

        private PageViewModel content;
        public PageViewModel Content
        {
            get
            {
                if (!inited)
                {
                    inited = true;
                    SetContent(DefaultPage, null);
                }
                return content;
            }
            private set => Set(ref content, value);
        }

        protected async Task SetContent(PageViewModel page, object param)
        {
            if (Content != null)
                await Content.OnNavigatedFrom();

            Content = page;

            if (Content != null)
                await Content?.OnNavigatedTo(param);
        }

        public override async Task OnNavigatedFrom()
        {
            if (Content != null)
                await Content.OnNavigatedFrom();
        }

        public override async Task OnNavigatedTo(object param)
        {
            if (Content != null)
                await Content.OnNavigatedTo(param);
        }

        protected virtual PageViewModel DefaultPage => null;
    }
}