using JankiBusiness;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Janki
{
    internal class DialogService : IDialogService
    {
        public async Task<bool> ShowConfirmationDialog(string title, string content, string positiveOption, string negativeOption)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = title,
                Content = content,
                PrimaryButtonText = positiveOption,
                CloseButtonText = negativeOption
            };

            ContentDialogResult result = await dialog.ShowAsync();

            return result == ContentDialogResult.Primary;
        }

        public async Task<string> ShowTextPromptDialog(string title, string initialContent, bool canCancel)
        {
            TextBox textBox = new TextBox()
            {
                AcceptsReturn = false,
                Height = 32,
                Text = initialContent
            };

            ContentDialog dialog = new ContentDialog()
            {
                Title = title,
                Content = textBox,
                PrimaryButtonText = "Ok"
            };

            if (canCancel)
                dialog.CloseButtonText = "Cancel";

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
                return textBox.Text;
            else
                return null;
        }
    }
}