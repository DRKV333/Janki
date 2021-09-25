using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using System;
using System.Globalization;

namespace JankiAvalonia.Converters
{
    public class NavigationViewItemInvokedEventArgsConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((NavigationViewItemInvokedEventArgs)value).InvokedItemContainer.DataContext;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}