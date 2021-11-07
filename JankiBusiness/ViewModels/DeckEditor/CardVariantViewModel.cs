using JankiBusiness.ViewModels.CardTypeEditor;
using JankiBusiness.ViewModels.Study;
using JankiCards.Janki;

namespace JankiBusiness.ViewModels.DeckEditor
{
    public class CardVariantViewModel : ViewModel, CardTypeEditorPageViewModel.ISelectionRedirector
    {
        public VariantType Variant { get; }

        public string Name
        {
            get => Variant.Name;
            set
            {
                Variant.Name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public string FrontFormat
        {
            get => Variant.FrontFormat;
            set
            {
                Variant.FrontFormat = value;
                RaisePropertyChanged(nameof(FrontFormat));
                Preview.Render();
            }
        }

        public string BackFormat
        {
            get => Variant.BackFormat;
            set
            {
                Variant.BackFormat = value;
                RaisePropertyChanged(nameof(BackFormat));
                Preview.Render();
            }
        }

        public CardViewModel Preview { get; }

        public CardTypeViewModel TypeVM { get; }

        public CardVariantViewModel(CardTypeViewModel TypeVM, CardType type, VariantType Variant)
        {
            this.TypeVM = TypeVM;
            this.Variant = Variant;
            Preview = CardViewModel.CreatePreview(type, Variant);
        }

        public CardTypeEditorPageViewModel.ISelectionRedirector Redirect() => null;

        public CardVariantViewModel GetCardVariant() => this;

        public CardTypeViewModel GetCardType() => TypeVM;
    }
}