using JankiBusiness.Abstraction;
using JankiBusiness.Services;
using JankiBusiness.ViewModels.DeckEditor;
using LibAnkiCards;
using LibAnkiCards.Context;
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

        public IAnkiContextProvider Provider { get; set; }

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

        private string selectedField;

        public string SelectedField
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

            AddCardType = new GenericDelegateCommand(async p =>
            {
                string name = await DialogService.ShowTextPromptDialog("Card Type Name", "", true);

                if (name == null)
                    return;

                using (IAnkiContext context = Provider.CreateContext())
                {
                    Collection collection = context.Collection;

                    long id = collection.CardTypes.Any() ? collection.CardTypes.Max(x => x.Key) + 1 : 0;

                    CardType type = new CardType()
                    {
                        Css = @"
.card {
    font-family: arial;
    font-size:150%;
    text-align: center;
    color: Black;
    background-color:black;
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
                        LatexPre = @"
\documentclass[12pt]{article}
\special{papersize=3in,5in}
\usepackage[utf8]{inputenc}
\usepackage{amssymb,amsmath}
\pagestyle{empty}
\setlength{\parindent}{0in}
\begin{document}
",
                        LatexPost = @"
\end{document}
",
                        Id = id,
                        Name = name,
                        Tags = new List<string>(),
                        Fields = new List<CardField>() { new CardField() { Id = 0, Name = "Front" }, new CardField() { Id = 1, Name = "Back" } },
                        Variants = new List<CardVariant>() { MakeVariant("Card 1") }
                    };

                    collection.CardTypes.Add(id, type);
                    context.Collection = collection;

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
                    $"Are you sure you want to delete the \"{SelectedType.Name}\" card type and all {await SelectedType.CountNotes()} cards that use it?",
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

                CardVariant variant = MakeVariant(name);

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

        private CardVariant MakeVariant(string name) => new CardVariant()
        {
            FrontFormat = "{{Front}}",
            BackFormat = "{{FrontSide}}\n\n<hr id=answer>\n\n{{Back}}",
            Name = name
        };

        public override async Task OnNavigatedTo(object param)
        {
            using (IAnkiContext context = Provider.CreateContext())
            {
                List<CardTypeViewModel> cardTypes = await Task.Run(() => context.Collection.CardTypes.Select(x => new CardTypeViewModel(Provider, x.Value)).ToList());

                CardTypes.Clear();
                foreach (var item in cardTypes)
                {
                    CardTypes.Add(item);
                }
            }
        }

        public override Task OnNavigatedFrom() => SelectedType?.SaveChanges() ?? Task.CompletedTask;
    }
}