using JankiBusiness;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
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

        public async Task<Stream> OpenFile(params string[] filters) => (await OpenFileWithName(filters)).file;

        public async Task<(Stream file, string name)> OpenFileWithName(params string[] filters)
        {
            FileOpenPicker picker = new FileOpenPicker();

            foreach (var item in filters)
            {
                picker.FileTypeFilter.Add(item);
            }

            StorageFile file = await picker.PickSingleFileAsync();

            if (file == null)
                return (null, null);

            return ((await file.OpenReadAsync()).AsStreamForRead(), file.Name);
        }
    }
}