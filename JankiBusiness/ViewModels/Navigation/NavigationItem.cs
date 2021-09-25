namespace JankiBusiness.ViewModels.Navigation
{
    public class NavigationItem
    {
        public string Name { get; }
        public string Icon { get; }
        public PageViewModel Content { get; }

        public NavigationItem(string Name, string Icon, PageViewModel Content)
        {
            this.Name = Name;
            this.Icon = Icon;
            this.Content = Content;
        }
    }
}