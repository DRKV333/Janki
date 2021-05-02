using JankiBusiness;
using System.Threading.Tasks;

namespace Janki
{
    public class WebEditBoxToolbarCoordinator : ViewModel
    {
        // Fun Fact: One way data binding to a toggle button's IsChecked does not work...

        public WebEditBox ActiveBox { get; set; }

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
    }
}