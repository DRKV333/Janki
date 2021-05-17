using JankiBusiness.ViewModels.DeckEditor;
using LibAnkiCards;
using LibAnkiCards.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.CardTypeEditor
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

        public async Task<int> CountNotes()
        {
            using (IAnkiContext context = provider.CreateContext())
            {
                return await context.Notes.CountAsync(x => x.CardTypeId == type.Id);
            }
        }

        public async Task Delete()
        {
            using (IAnkiContext context = provider.CreateContext())
            {
                context.Notes.RemoveRange(context.Notes.Where(x => x.CardTypeId == type.Id));

                Collection collection = context.Collection;
                collection.CardTypes.Remove(type.Id);
                context.Collection = collection;

                await context.SaveChangesAsync();
            }
        }

        public async Task<CardVariantViewModel> AddVariant(CardVariant variant)
        {
            long id = type.Variants.Max(x => x.Id) + 1;
            variant.Id = id;

            type.Variants.Add(variant);

            await SaveChanges();

            if (singleVariant != null)
            {
                Variants.Add(singleVariant);
                singleVariant = null;
            }

            CardVariantViewModel variantVM = new CardVariantViewModel(type, variant);
            Variants.Add(variantVM);
            return variantVM;
        }

        public async Task DeleteVariant(CardVariantViewModel variant)
        {
            if (!type.Variants.Remove(variant.Variant))
                return;

            await SaveChanges();

            Variants.Remove(variant);
            if (Variants.Count == 1)
            {
                singleVariant = Variants.First();
                Variants.Clear();
            }
        }

        public async Task SaveChanges()
        {
            int fieldNum = 0;
            type.Fields.Clear();
            foreach (var item in Fields)
            {
                type.Fields.Add(new CardField() { Id = fieldNum++, Name = item });
            }

            using (IAnkiContext context = provider.CreateContext())
            {
                Collection collection = context.Collection;

                collection.CardTypes[type.Id] = type;

                context.Collection = collection;

                await context.SaveChangesAsync();
            }
        }
    }
}