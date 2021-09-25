using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using JankiBusiness.Services;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JankiAvalonia.Services
{
    public class DialogService : AvaloniaObject, IDialogService
    {
        public static readonly DirectProperty<DialogService, Window?> ParentWindowProperty =
            AvaloniaProperty.RegisterDirect<DialogService, Window?>(nameof(ParentWindow),
                x => x.ParentWindow,
                (x, v) => x.ParentWindow = v);

        private Window? parentWindow;

        public Window? ParentWindow
        {
            get => parentWindow;
            set => SetAndRaise(ParentWindowProperty, ref parentWindow, value);
        }

        public async Task<Stream> OpenFile(params string[] filters) => (await OpenFileWithName(filters)).file;

        public async Task<(Stream file, string name)> OpenFileWithName(params string[] filters)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                AllowMultiple = false,
                Filters = { new FileDialogFilter() { Extensions = filters.ToList() } },
            };

            string[] files = await dialog.ShowAsync(ParentWindow);
            string filePath = files[0];

            return (new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true), Path.GetFileNameWithoutExtension(filePath));
        }

        public async Task<bool> ShowConfirmationDialog(string title, string content, string positiveOption, string negativeOption)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = title,
                Content = content,
                PrimaryButtonText = positiveOption,
                CloseButtonText = negativeOption
            };

            return (await dialog.ShowAsync()) == ContentDialogResult.Primary;
        }

        public async Task<string?> ShowTextPromptDialog(string title, string initialContent, bool canCancel)
        {
            TextBox textBox = new TextBox() { Text = initialContent };
            textBox.AttachedToVisualTree += (_, _) => textBox.Focus();

            ContentDialog dialog = new ContentDialog()
            {
                Title = title,
                Content = textBox,
                PrimaryButtonText = "Ok",
                CloseButtonText = "Cancel"
            };

            return (await dialog.ShowAsync()) == ContentDialogResult.Primary ? textBox.Text : null;
        }
    }
}