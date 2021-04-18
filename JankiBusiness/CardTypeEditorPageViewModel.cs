using LibAnkiCards.Context;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace JankiBusiness
{
    public class CardTypeEditorPageViewModel : ViewModel
    {
        public interface ISelectionRedirector
        {
            ISelectionRedirector Redirect();

            CardVariantViewModel GetCardVariant();

            CardTypeViewModel GetCardType();
        }

        public IAnkiContextProvider Provider { get; set; }

        public IDialogService DialogService { get; set; }

        private ObservableCollection<CardTypeViewModel> cardTypes;

        public ObservableCollection<CardTypeViewModel> CardTypes
        {
            get
            {
                if (cardTypes == null)
                    Init();
                return cardTypes;
            }
        }

        private ISelectionRedirector selectedItem;

        public ISelectionRedirector SelectedItem
        {
            get => selectedItem;
            set
            {
                Set(ref selectedItem, value);

                CardTypeViewModel type = selectedItem.GetCardType();
                if (type != null)
                    SelectedType = type;

                ISelectionRedirector redirected = selectedItem.Redirect();
                if (redirected != null)
                    SelectedItem = redirected;
                else
                    SelectedVariant = selectedItem.GetCardVariant();
            }
        }

        private CardVariantViewModel selectedVariant;

        public CardVariantViewModel SelectedVariant
        {
            get => selectedVariant;
            set => Set(ref selectedVariant, value);
        }

        private CardTypeViewModel selectedType;

        public CardTypeViewModel SelectedType
        {
            get => selectedType;
            set => Set(ref selectedType, value);
        }

        private string selectedField;

        public string SelectedField
        {
            get => selectedField;
            set => Set(ref selectedField, value);
        }

        public GenericCommand AddField { get; }

        public GenericCommand RemoveField { get; }

        public CardTypeEditorPageViewModel()
        {
            AddField = new GenericDelegateCommand(async p =>
            {
                if (SelectedType != null)
                {
                    string name = await DialogService.ShowTextPromptDialog("Add Field", "", true);
                    if (name != null)
                    {
                        SelectedType.Fields.Add(name);
                        SelectedField = name;
                    }
                }
            });

            RemoveField = new GenericDelegateCommand(p =>
            {
                if (SelectedType != null && SelectedField != null)
                {
                    SelectedType.Fields.Remove(SelectedField);
                }
                return Task.CompletedTask;
            });
        }

        private void Init()
        {
            using (IAnkiContext context = Provider.CreateContext())
            {
                cardTypes = new ObservableCollection<CardTypeViewModel>(context.Collection.CardTypes.Select(x => new CardTypeViewModel(Provider, x.Value)));
            }
        }
    }
}