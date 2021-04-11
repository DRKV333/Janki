using JankiBusiness;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Janki
{
    internal class ConfirmationDialogService : IConfirmationDialogService
    {
        public async Task<bool> ShowDialog(string title, string content, string positiveOption, string negativeOption)
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
    }
}