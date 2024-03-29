﻿using JankiBusiness.ViewModels.CardTypeEditor;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Janki.Pages
{
    public class CardTypeDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CardTypeTemplate { get; set; }
        public DataTemplate CardVariantTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is CardTypeViewModel)
                return CardTypeTemplate;
            else
                return CardVariantTemplate;
        }
    }
}