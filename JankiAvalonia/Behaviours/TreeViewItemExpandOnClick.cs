using Avalonia;
using Avalonia.Controls;
using System;

namespace JankiAvalonia.Behaviours
{
    public class TreeViewItemExpandOnClick : AvaloniaObject
    {
        public static readonly AttachedProperty<bool> ActiveProperty =
            AvaloniaProperty.RegisterAttached<TreeViewItemExpandOnClick, TreeViewItem, bool>("Active");

        public static bool GetActive(AvaloniaObject obj) => obj.GetValue(ActiveProperty);

        public static void SetActive(AvaloniaObject obj, bool value) => obj.SetValue(ActiveProperty, value);

        static TreeViewItemExpandOnClick()
        {
            ActiveProperty.Changed.Subscribe(x =>
            {
                if (x.NewValue.Value)
                    ((TreeViewItem)x.Sender).PointerPressed += TreeViewItemExpandOnClick_PointerPressed;
                else
                    ((TreeViewItem)x.Sender).PointerPressed -= TreeViewItemExpandOnClick_PointerPressed;
            });
        }

        private static void TreeViewItemExpandOnClick_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (sender != null)
                ((TreeViewItem)sender).IsExpanded = true;
        }
    }
}