using JankiCards.Janki;
using JankiTransfer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JankiTransfer.ChangeDetection
{
    public class ChangeDetector<TContext>
    {
        private readonly IChangeContext<TContext> changeContext;

        public ChangeDetector(IChangeContext<TContext> changeContext)
        {
            this.changeContext = changeContext;
        }

        public async Task<ChangeData> DetectChanges(DateTime since, TContext context)
        {
            ChangeData data = new ChangeData();

            data.CardTypesAdded = (await changeContext.ToListAsync(changeContext.CardTypes(context).Where(x => !x.IsDeleted && x.Created > since)))
                .Select(x => new CardTypeData()
                {
                    Id = x.Id,
                    Css = x.Css,
                    Name = x.Name,
                    Tags = x.Tags,
                    FieldsAdded = x.Fields.Select(y => new CardFieldTypeData()
                    {
                        Id = y.Id,
                        Name = y.Name,
                        Order = y.Order
                    }).ToList(),
                    VariantsAdded = x.Variants.Select(y => new VariantTypeData()
                    {
                        Id = y.Id,
                        BackFormat = y.BackFormat,
                        FrontFormat = y.FrontFormat,
                        Name = y.Name
                    }).ToList(),
                }).ToList();

            data.DecksAdded = (await changeContext.ToListAsync(changeContext.Decks(context).Where(x => !x.IsDeleted && x.Created > since)))
                .Select(x => new DeckData()
                {
                    Id = x.Id,
                    Name = x.Name,
                    ParentDeckId = x.ParentDeckId
                }).ToList();

            data.CardsAdded = (await changeContext.ToListAsync(changeContext.Cards(context).Where(x => !x.IsDeleted && x.Created > since)))
                .Select(x => new CardData()
                {
                    Id = x.Id,
                    CardTypeId = x.CardTypeId,
                    DeckId = x.DeckId,
                    FieldsAdded = x.Fields.Select(y => new CardFieldData()
                    {
                        Id = y.Id,
                        Content = y.Content,
                        CardFieldTypeId = y.CardFieldTypeId,
                        Media = y.Media.Select(z => z.Id).ToList()
                    }).ToList()
                }).ToList();

            Dictionary<Guid, CardTypeData> cardTypeChanges = new Dictionary<Guid, CardTypeData>();
            Dictionary<Guid, VariantTypeData> variantTypeChanges = new Dictionary<Guid, VariantTypeData>();

            IList<CardFieldType> cardFieldTypesAdded = await changeContext.ToListAsync(changeContext.CardFieldTypes(context).Where(x =>
                !x.IsDeleted && x.CardType.Created < since && x.Created > since));
            foreach (var item in cardFieldTypesAdded)
            {
                CardTypeData cardTypeData = GetOrCreateCardTypeData(cardTypeChanges, item.CardTypeId);

                cardTypeData.FieldsAdded.Add(new CardFieldTypeData()
                {
                    Id = item.Id,
                    Name = item.Name,
                    Order = item.Order
                });
            }

            IList<CardFieldType> cardFieldsRemoved = await changeContext.ToListAsync(changeContext.CardFieldTypes(context).Where(x =>
                x.IsDeleted && x.CardType.Created < since && x.Created < since && x.LastModified > since));
            foreach (var item in cardFieldsRemoved)
            {
                CardTypeData cardTypeData = GetOrCreateCardTypeData(cardTypeChanges, item.CardTypeId);
                cardTypeData.FieldsRemoved.Add(item.Id);
            }

            IList<VariantType> variantTypesAdded = await changeContext.ToListAsync(changeContext.VariantTypes(context).Where(x =>
                !x.IsDeleted && x.CardType.Created < since && x.Created > since));
            foreach (var item in variantTypesAdded)
            {
                CardTypeData cardTypeData = GetOrCreateCardTypeData(cardTypeChanges, item.CardTypeId);

                cardTypeData.VariantsAdded.Add(new VariantTypeData()
                {
                    Id = item.Id,
                    BackFormat = item.BackFormat,
                    FrontFormat = item.FrontFormat,
                    Name = item.Name
                });
            }

            IList<VariantType> variantsRemoved = await changeContext.ToListAsync(changeContext.VariantTypes(context).Where(x =>
                x.IsDeleted && x.CardType.Created < since && x.Created < since && x.LastModified > since));
            foreach (var item in variantsRemoved)
            {
                CardTypeData cardTypeData = GetOrCreateCardTypeData(cardTypeChanges, item.CardTypeId);
                cardTypeData.VariantsRemoved.Add(item.Id);
            }

            IList<AuditLog> variantTypeLogs = await changeContext.ToListAsync(changeContext.AuditLogs(context).Where(x =>
                !x.IsDeleted && x.Created > since && x.Table == nameof(VariantType)));
            foreach (var item in variantTypeLogs)
            {
                VariantType variantType = await changeContext.SingleOrDefaultAsync(changeContext.VariantTypes(context).Where(x => x.Id == item.ChangedId));
                if (variantType == null)
                    continue;

                if (!variantTypeChanges.TryGetValue(variantType.Id, out VariantTypeData itemVariant))
                {
                    CardTypeData cardTypeData = GetOrCreateCardTypeData(cardTypeChanges, variantType.CardTypeId);
                    itemVariant = new VariantTypeData()
                    {
                        Id = variantType.Id
                    };
                    variantTypeChanges.Add(variantType.Id, itemVariant);
                    cardTypeData.VariantsChanged.Add(itemVariant);
                }

                if (item.Column == nameof(VariantType.FrontFormat))
                    itemVariant.FrontFormat = item.NewValue;
                else if (item.Column == nameof(VariantType.BackFormat))
                    itemVariant.BackFormat = item.NewValue;
            }

            IList<AuditLog> cardTypeLogs = await changeContext.ToListAsync(changeContext.AuditLogs(context).Where(x =>
                !x.IsDeleted && x.Created > since && x.Table == nameof(CardType)));
            foreach (var item in cardTypeLogs)
            {
                CardTypeData cardTypeData = GetOrCreateCardTypeData(cardTypeChanges, item.ChangedId);

                if (item.Column == nameof(CardTypeData.Css))
                    cardTypeData.Css = item.NewValue;
            }

            data.CardTypesChanged = cardTypeChanges.Values.ToList();

            data.CardTypesRemoved = await changeContext.ToListAsync(changeContext.CardTypes(context).Where(x =>
                x.IsDeleted && x.Created < since && x.LastModified > since).Select(x => x.Id));

            data.DecksRemoved = await changeContext.ToListAsync(changeContext.Decks(context).Where(x =>
                x.IsDeleted && x.Created < since && x.LastModified > since).Select(x => x.Id));

            data.CardsRemoved = await changeContext.ToListAsync(changeContext.Cards(context).Where(x =>
                x.IsDeleted && x.Created < since && x.LastModified > since).Select(x => x.Id));

            Dictionary<Guid, CardData> cardChanges = new Dictionary<Guid, CardData>();
            Dictionary<Guid, CardFieldData> fieldChanges = new Dictionary<Guid, CardFieldData>();

            IList<CardField> fieldsAdded = await changeContext.ToListAsync(changeContext.CardFields(context).Where(x =>
                !x.IsDeleted && x.Card.Created < since && x.Created > since));
            foreach (var item in fieldsAdded)
            {
                CardData cardData = GetOrCreateCardData(cardChanges, item.CardId);

                cardData.FieldsAdded.Add(new CardFieldData()
                {
                    Id = item.Id,
                    CardFieldTypeId = item.CardFieldTypeId,
                    Content = item.Content,
                    Media = item.Media.Select(x => x.Id).ToList()
                });
            }

            IList<CardField> fieldsRemoved = await changeContext.ToListAsync(changeContext.CardFields(context).Where(x =>
                x.IsDeleted && x.Card.Created < since && x.Created < since && x.LastModified > since));
            foreach (var item in fieldsRemoved)
            {
                CardData cardData = GetOrCreateCardData(cardChanges, item.CardId);
                cardData.FieldsRemoved.Add(item.Id);
            }

            IList<AuditLog> fieldLogs = await changeContext.ToListAsync(changeContext.AuditLogs(context).Where(x =>
                !x.IsDeleted && x.Created > since && x.Table == nameof(CardField)));
            foreach (var item in fieldLogs)
            {
                CardField cardField = await changeContext.SingleOrDefaultAsync(changeContext.CardFields(context).Where(x => x.Id == item.ChangedId));
                if (cardField == null)
                    continue;

                if (!fieldChanges.TryGetValue(cardField.Id, out CardFieldData itemField))
                {
                    CardData cardData = GetOrCreateCardData(cardChanges, cardField.Id);
                    itemField = new CardFieldData()
                    {
                        Id = cardField.Id,
                        Media = cardField.Media.Select(x => x.Id).ToList()
                    };
                    fieldChanges.Add(cardField.Id, itemField);
                    cardData.FieldsChanged.Add(itemField);
                }

                if (item.Column == nameof(CardField.Content))
                    itemField.Content = item.NewValue;
            }

            data.CardsChanged = cardChanges.Values.ToList();

            return data;
        }

        public async Task ApplyChanges(ChangeData data, Guid bundleId, TContext context)
        {
            foreach (var item in data.CardTypesAdded)
            {
                changeContext.Add(context, new CardType()
                {
                    Id = item.Id,
                    BundleId = bundleId,
                    Css = item.Css,
                    Fields = item.FieldsAdded.Select(x => new CardFieldType() { Name = x.Name, Order = x.Order }).ToList(),
                    Name = item.Name,
                    Tags = item.Tags,
                    Variants = item.VariantsAdded.Select(x => new VariantType()
                    {
                        BundleId = bundleId,
                        BackFormat = x.BackFormat,
                        FrontFormat = x.FrontFormat,
                        Name = x.Name
                    }).ToList(),
                });
            }

            foreach (var item in data.DecksAdded)
            {
                changeContext.Add(context, new Deck()
                {
                    Id = item.Id,
                    BundleId = bundleId,
                    Name = item.Name,
                    ParentDeckId = item.ParentDeckId,
                });
            }

            foreach (var item in data.CardsAdded)
            {
                changeContext.Add(context, new Card()
                {
                    Id = item.Id,
                    DeckId = item.DeckId,
                    BundleId = bundleId,
                    CardTypeId = item.CardTypeId,
                    Fields = item.FieldsAdded.Select(x => new CardField()
                    {
                        BundleId = bundleId,
                        Content = x.Content,
                        CardFieldTypeId = x.CardFieldTypeId
                    }).ToList(),
                });
            }

            foreach (var item in data.CardTypesRemoved)
            {
                changeContext.Remove(context, new CardType() { Id = item });
            }

            foreach (var item in data.DecksRemoved)
            {
                changeContext.Remove(context, new Deck() { Id = item });
            }

            foreach (var item in data.CardsRemoved)
            {
                changeContext.Remove(context, new Card() { Id = item });
            }

            foreach (var item in data.CardTypesChanged)
            {
                CardType existing = await changeContext.SingleOrDefaultAsync(changeContext.CardTypes(context).Where(x => x.Id == item.Id));
                if (existing == null)
                    continue;

                if (item.Name != null)
                    existing.Name = item.Name;

                if (item.Tags != null)
                    existing.Tags = item.Tags;

                foreach (var item2 in item.VariantsAdded)
                {
                    existing.Variants.Add(new VariantType()
                    {
                        BundleId = bundleId,
                        Id = item2.Id,
                        FrontFormat = item2.FrontFormat,
                        BackFormat = item2.BackFormat,
                        Name = item2.Name
                    });
                }

                foreach (var item2 in item.VariantsChanged)
                {
                    VariantType variant = existing.Variants.SingleOrDefault(x => x.Id == item2.Id);
                    if (variant == null)
                        continue;

                    if (item2.FrontFormat != null)
                        variant.FrontFormat = item2.FrontFormat;

                    if (item2.BackFormat != null)
                        variant.BackFormat = item2.BackFormat;

                    if (item2.Name != null)
                        variant.Name = item2.Name;
                }

                foreach (var item2 in item.VariantsRemoved)
                {
                    changeContext.Remove(context, new VariantType() { Id = item2 });
                }

                changeContext.Update(context, existing);
            }

            foreach (var item in data.CardsChanged)
            {
                Card existing = await changeContext.SingleOrDefaultAsync(changeContext.Cards(context).Where(x => x.Id == item.Id));
                if (existing == null)
                    continue;

                foreach (var item2 in item.FieldsAdded)
                {
                    existing.Fields.Add(new CardField()
                    {
                        BundleId = bundleId,
                        Id = item2.Id,
                        Content = item2.Content,
                        CardFieldTypeId = item2.CardFieldTypeId
                    });
                }

                foreach (var item2 in item.FieldsChanged)
                {
                    CardField field = existing.Fields.SingleOrDefault(x => x.Id == item2.Id);
                    if (field == null)
                        continue;

                    if (item2.Content != null)
                        field.Content = item2.Content;
                }

                foreach (var item2 in item.FieldsRemoved)
                {
                    changeContext.Remove(context, new CardField() { Id = item2 });
                }

                changeContext.Update(context, existing);
            }
        }

        public static CardTypeData GetOrCreateCardTypeData(Dictionary<Guid, CardTypeData> cardTypeChanges, Guid id)
        {
            if (!cardTypeChanges.TryGetValue(id, out CardTypeData itemCard))
            {
                itemCard = new CardTypeData()
                {
                    Id = id,
                    FieldsAdded = new List<CardFieldTypeData>(),
                    FieldsRemoved = new List<Guid>(),
                    VariantsAdded = new List<VariantTypeData>(),
                    VariantsChanged = new List<VariantTypeData>(),
                    VariantsRemoved = new List<Guid>()
                };
                cardTypeChanges.Add(id, itemCard);
            }

            return itemCard;
        }

        public static CardData GetOrCreateCardData(Dictionary<Guid, CardData> cardChanges, Guid id)
        {
            if (!cardChanges.TryGetValue(id, out CardData itemCard))
            {
                itemCard = new CardData()
                {
                    Id = id,
                    FieldsAdded = new List<CardFieldData>(),
                    FieldsChanged = new List<CardFieldData>(),
                    FieldsRemoved = new List<Guid>()
                };
                cardChanges.Add(id, itemCard);
            }
            return itemCard;
        }
    }
}