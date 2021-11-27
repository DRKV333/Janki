using JankiBusiness.ViewModels.DeckEditor;
using JankiCards.Janki;
using JankiCards.Janki.Context;
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
        private readonly IJankiContextProvider provider;

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

        public ObservableCollection<CardFieldType> Fields { get; }

        public ObservableCollection<CardVariantViewModel> Variants { get; }

        private CardVariantViewModel singleVariant;

        public CardTypeViewModel(IJankiContextProvider provider, CardType type)
        {
            this.provider = provider;
            this.type = type;

            name = type.Name;
            Fields = new ObservableCollection<CardFieldType>(type.Fields.OrderBy(x => x.Order));

            List<CardVariantViewModel> variants = type.Variants.Select(x => new CardVariantViewModel(this, type, x)).ToList();
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

        public async Task<int> CountCards()
        {
            using (JankiContext context = provider.CreateContext())
            {
                return await context.TheCards.CountAsync(x => x.CardTypeId == type.Id);
            }
        }

        public async Task Delete()
        {
            using (JankiContext context = provider.CreateContext())
            {
                context.CardTypes.Remove(type);
                await context.SaveChangesAsync();
            }
        }

        public async Task<CardVariantViewModel> AddVariant(VariantType variant)
        {
            type.Variants.Add(variant);

            using (JankiContext context = provider.CreateContext())
            {
                context.CardTypes.Attach(type);
                context.VariantTypes.Add(variant);

                foreach (var item in await context.TheCards.Where(x => x.CardType == type).ToListAsync())
                {
                    context.CardStudyDatas.Add(new CardStudyData()
                    {
                        Card = item,
                        Variant = variant
                    });
                }

                await context.SaveChangesAsync();
            }

            if (singleVariant != null)
            {
                Variants.Add(singleVariant);
                singleVariant = null;
            }

            CardVariantViewModel variantVM = new CardVariantViewModel(this, type, variant);
            Variants.Add(variantVM);
            return variantVM;
        }

        public async Task DeleteVariant(CardVariantViewModel variant)
        {
            if (!type.Variants.Remove(variant.Variant))
                return;

            using (JankiContext context = provider.CreateContext())
            {
                context.VariantTypes.Remove(variant.Variant);
                await SaveChanges(context);
            }

            Variants.Remove(variant);
            if (Variants.Count == 1)
            {
                singleVariant = Variants.First();
                Variants.Clear();
            }
        }

        public async Task SaveChanges()
        {
            using (JankiContext context = provider.CreateContext())
            {
                await SaveChanges(context);
            }
        }

        private async Task SaveChanges(JankiContext context)
        {
            foreach (var item in Fields)
            {
                item.CardType = type;

                CardFieldType dbField = await context.CardFieldTypes.FindAsync();
                dbField.CardType = item.CardType;
                dbField.CardTypeId = item.CardTypeId;
                dbField.Name = item.Name;
                dbField.Order = item.Order;
            }

            await context.SaveChangesAsync();
        }
    }
}