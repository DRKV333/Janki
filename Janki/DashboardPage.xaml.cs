﻿using JankiBusiness;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Janki
{
    public sealed partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            (DataContext as PageViewModel)?.OnNavigatedTo(e.Parameter);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            (DataContext as PageViewModel)?.OnNavigatedFrom();
        }
    }
}