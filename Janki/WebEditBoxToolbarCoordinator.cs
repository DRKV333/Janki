﻿using JankiBusiness;
using LibAnkiCards.Importing;
using System.IO;

namespace Janki
{
    public class WebEditBoxToolbarCoordinator : ViewModel
    {
        // Fun Fact: One way data binding to a toggle button's IsChecked does not work...

        public WebEditBox ActiveBox { get; set; }

        public IMediaImporter Importer { get; set; }
        public IDialogService DialogService { get; set; }

        public GenericCommand InsertImage { get; }

        private bool bold;

        public bool Bold
        {
            get => bold;
            set { Set(ref bold, value); ActiveBox.Bold(value); }
        }

        public void SetActualBold(bool value)
        {
            bold = value;
            RaisePropertyChanged(nameof(Bold));
        }

        private bool italic;

        public bool Italic
        {
            get => italic;
            set { Set(ref italic, value); ActiveBox.Italic(value); }
        }

        public void SetActualItalic(bool value)
        {
            italic = value;
            RaisePropertyChanged(nameof(Italic));
        }

        private bool underline;

        public bool Underline
        {
            get => underline;
            set { Set(ref underline, value); ActiveBox.Underline(value); }
        }

        public void SetActualUnderline(bool value)
        {
            underline = value;
            RaisePropertyChanged(nameof(Underline));
        }

        public WebEditBoxToolbarCoordinator()
        {
            InsertImage = new GenericDelegateCommand(async p =>
            {
                if (ActiveBox == null)
                    return;

                (Stream file, string name) = await DialogService.OpenFileWithName(".gif", ".jpg", ".jpeg", ".png", ".svg");
                if (file == null)
                    return;

                await Importer.ImportMedia(name, file);

                ActiveBox.InsertImage(name);
            });
        }
    }
}