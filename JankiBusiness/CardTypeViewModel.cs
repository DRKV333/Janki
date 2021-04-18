using LibAnkiCards;
using LibAnkiCards.Context;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JankiBusiness
{
    public class CardTypeViewModel : ViewModel, CardTypeEditorPageViewModel.ISelectionRedirector
    {
        private readonly CardType type;
        private readonly IAnkiContextProvider provider;

        private string name;

        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

        public string Css
        {
            get => type.Css;
            set
            {
                type.Css = value;
                RaisePropertyChanged(nameof(Css));
                RenderAllVariants();
            }
        }

        public ObservableCollection<string> Fields { get; }

        public ObservableCollection<CardVariantViewModel> Variants { get; }

        private CardVariantViewModel singleVariant;

        public CardTypeViewModel(IAnkiContextProvider provider, CardType type)
        {
            this.provider = provider;
            this.type = type;

            name = type.Name;
            Fields = new ObservableCollection<string>(type.Fields.OrderBy(x => x.Id).Select(x => x.Name));

            List<CardVariantViewModel> variants = type.Variants.Select(x => new CardVariantViewModel(type, x)).ToList();
            if (variants.Count > 1)
            {
                singleVariant = null;
                Variants = new ObservableCollection<CardVariantViewModel>(variants);
            }
            else
            {
                singleVariant = variants.Single();
                Variants = new ObservableCollection<CardVariantViewModel>();
            }
        }

        public CardTypeEditorPageViewModel.ISelectionRedirector Redirect() => singleVariant == null ? Variants.FirstOrDefault() : null;

        public CardVariantViewModel GetCardVariant() => singleVariant;

        public CardTypeViewModel GetCardType() => this;

        private void RenderAllVariants()
        {
            singleVariant?.Preview.Render();
            foreach (var item in Variants)
            {
                item.Preview.Render();
            }
        }
    }
}