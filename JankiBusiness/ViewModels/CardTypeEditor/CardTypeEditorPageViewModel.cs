using JankiBusiness.Abstraction;
using JankiBusiness.Services;
using JankiBusiness.ViewModels.DeckEditor;
using LibAnkiCards.Janki;
using LibAnkiCards.Janki.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.CardTypeEditor
{
    public class CardTypeEditorPageViewModel : PageViewModel
    {
        public interface ISelectionRedirector
        {
            ISelectionRedirector Redirect();

            CardVariantViewModel GetCardVariant();

            CardTypeViewModel GetCardType();
        }

        public IJankiContextProvider Provider { get; set; }

        public IDialogService DialogService { get; set; }

        public ObservableCollection<CardTypeViewModel> CardTypes { get; } = new ObservableCollection<CardTypeViewModel>();

        private ISelectionRedirector selectedItem;

        public ISelectionRedirector SelectedItem
        {
            get => selectedItem;
            set
            {
                Set(ref selectedItem, value);

                if (selectedItem == null)
                {
                    SelectedType = null;
                    SelectedVariant = null;
                }
                else
                {
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
            set
            {
                if (value != selectedType)
                    selectedType?.SaveChanges();
                Set(ref selectedType, value);
            }
        }

        private CardFieldType selectedField;

        public CardFieldType SelectedField
        {
            get => selectedField;
            set => Set(ref selectedField, value);
        }

        public GenericCommand AddField { get; }

        public GenericCommand RemoveField { get; }

        public GenericCommand AddCardType { get; }

        public GenericCommand RemoveCardType { get; }

        public GenericCommand AddVariant { get; }

        public GenericCommand RemoveVariant { get; }

        public CardTypeEditorPageViewModel()
        {
            AddField = new GenericDelegateCommand(async p =>
            {
                if (SelectedType != null)
                {
                    string name = await DialogService.ShowTextPromptDialog("Add Field", "", true);
                    if (name != null)
                    {
                        CardFieldType newField = new CardFieldType() { Name = name };
                        SelectedType.Fields.Add(newField);
                        SelectedField = newField;
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

            AddCardType = new GenericDelegateCommand(async p =>
            {
                string name = await DialogService.ShowTextPromptDialog("Card Type Name", "", true);

                if (name == null)
                    return;

                using (JankiContext context = Provider.CreateContext())
                {
                    CardType type = new CardType()
                    {
                        Css = @"
.card {
    font-family: arial;
    font-size:150%;
    text-align: center;
    color: Black;
    background-color: White;
}

#rubric {
    text-align: left;
    padding: 4px;
    padding-left: 10px;
    padding-right: 10px;
    margin-bottom: 10px;
    background: #1d6695;
    color: white;
    font-weight: 500;
}

img {
    max-width: 100%;
    height: auto;
    width: 300px;
    border-radius: 20px;
}
",
                        Name = name,
                        Fields = new List<CardFieldType>() { new CardFieldType() { Name = "Front", Order = 1 }, new CardFieldType() { Name = "Back", Order = 2 } },
                        Variants = new List<VariantType>() { MakeVariant("Card 1") }
                    };

                    context.CardTypes.Add(type);

                    await context.SaveChangesAsync();

                    CardTypeViewModel typeVM = new CardTypeViewModel(Provider, type);
                    CardTypes.Add(typeVM);
                    SelectedItem = typeVM;
                }
            });

            RemoveCardType = new GenericDelegateCommand(async p =>
            {
                if (SelectedType == null)
                    return;

                if (await DialogService.ShowConfirmationDialog(
                    "Delete Card Type",
                    $"Are you sure you want to delete the \"{SelectedType.Name}\" card type and all {await SelectedType.CountCards()} cards that use it?",
                    "Delete", "Cancel"))
                {
                    CardTypeViewModel toDelete = SelectedType;

                    selectedType = null;
                    SelectedItem = null;

                    CardTypes.Remove(toDelete);

                    await toDelete.Delete();
                }
            });

            AddVariant = new GenericDelegateCommand(async p =>
            {
                if (SelectedType == null)
                    return;

                string name = await DialogService.ShowTextPromptDialog("Variant Name", "", true);

                if (name == null)
                    return;

                VariantType variant = MakeVariant(name);

                SelectedItem = await SelectedType.AddVariant(variant);
            });

            RemoveVariant = new GenericDelegateCommand(async p =>
            {
                if (selectedType == null || SelectedVariant == null || !SelectedType.Variants.Any())
                    return;

                if (await DialogService.ShowConfirmationDialog(
                    "Delete Card Variant",
                    $"Are you sure you want to delete the \"{SelectedType.Name}\" variant?",
                    "Delete", "Cancel"))
                {
                    CardTypeViewModel toDeleteType = SelectedType;
                    CardVariantViewModel toDeleteVariant = SelectedVariant;

                    selectedVariant = null;
                    SelectedItem = SelectedType;

                    await toDeleteType.DeleteVariant(toDeleteVariant);
                }
            });
        }

        private VariantType MakeVariant(string name) => new VariantType()
        {
            FrontFormat = "{{Front}}",
            BackFormat = "{{FrontSide}}\n\n<hr id=answer>\n\n{{Back}}",
            Name = name
        };

        public override async Task OnNavigatedTo(object param)
        {
            using (JankiContext context = Provider.CreateContext())
            {
                List<CardType> cardTypes = await context.CardTypes
                    .Include(x => x.Fields)
                    .Include(x => x.Variants)
                    .ToListAsync();

                CardTypes.Clear();
                foreach (var item in cardTypes)
                {
                    CardTypes.Add(new CardTypeViewModel(Provider, item));
                }
            }
        }

        public override Task OnNavigatedFrom() => SelectedType?.SaveChanges() ?? Task.CompletedTask;
    }
}