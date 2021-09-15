using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using JankiAvalonia.Pages;
using System;

namespace JankiAvalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FindControl<Frame>("ContentFrame").Navigate(typeof(DashboardPage), null);
        }

        private void NavigationView_ItemInvoked(object sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer.Tag is Type page)
            {
                this.FindControl<Frame>("ContentFrame").Navigate(page, null, args.RecommendedNavigationTransitionInfo);
            }
        }
    }
}