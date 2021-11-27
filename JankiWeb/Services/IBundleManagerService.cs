using JankiCards.Janki;
using JankiTransfer.DTO;
using JankiWebCards.Janki.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JankiWeb.Services
{
    public interface IBundleManagerService
    {
        Task<IEnumerable<DeckTreeModel>> GetAllDecks(Guid bundleId);
        Task<IEnumerable<BundleModel>> GetPublicBundles();
        Task PublishBundle(IList<Guid> deckIds, string bundleName);
        Task ImportBundle(Guid source, Guid dest);
    }

    public class BundleManagerService : IBundleManagerService
    {
        private readonly JankiWebContext context;

        public BundleManagerService(JankiWebContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<DeckTreeModel>> GetAllDecks(Guid bundleId) => await GetChildren(bundleId, null);

        private async Task<IList<DeckTreeModel>> GetChildren(Guid bundleId, Guid? parentId)
        {
            List<Deck> topLevel = await context.Decks.Where(x => x.BundleId == bundleId && x.ParentDeckId == parentId).ToListAsync();
            List<DeckTreeModel> topLevelTree = topLevel.Select(x => new DeckTreeModel() { Id = x.Id, Name = x.Name }).ToList();

            foreach (var item in topLevelTree)
            {
                item.Children = await GetChildren(bundleId, item.Id);
            }

            return topLevelTree;
        }

        public async Task<IEnumerable<BundleModel>> GetPublicBundles() =>
            (await context.Bundles.Where(x => x.IsPublic).ToListAsync())
            .Select(BundleToModel);

        public async Task PublishBundle(IList<Guid> deckIds, string bundleName)
        {
            Bundle newBundle = new Bundle()
            {
                Id = Guid.NewGuid(),
                Name = bundleName,
                IsPublic = true
            };

            List<Deck> decks = await IncludeEverything(context.Decks)
                .AsNoTrackingWithIdentityResolution()
                .Where(x => deckIds.Contains(x.Id))
                .ToListAsync();

            context.Bundles.Add(newBundle);

            DetachAndAssingToBundle(decks, newBundle);

            await context.SaveChangesAsync();
        }

        public async Task ImportBundle(Guid source, Guid dest)
        {
            Bundle destBundle = await context.Bundles.FindAsync(dest);

            List<Deck> decks = await IncludeEverything(context.Decks)
                .AsNoTrackingWithIdentityResolution()
                .Where(x => x.BundleId == source)
                .ToListAsync();

            DetachAndAssingToBundle(decks, destBundle);

            await context.SaveChangesAsync();
        }

        private static IQueryable<Deck> IncludeEverything(IQueryable<Deck> query) => query
            .Include(x => x.ParentDeck)
            .Include(x => x.Cards).ThenInclude(x => x.Fields).ThenInclude(x => x.Media)
            .Include(x => x.Cards).ThenInclude(x => x.CardType).ThenInclude(x => x.Fields)
            .Include(x => x.Cards).ThenInclude(x => x.CardType).ThenInclude(x => x.Variants);

        private void DetachAndAssingToBundle(IEnumerable<Deck> decks, Bundle bundle)
        {
            Dictionary<object, Guid> guids = new Dictionary<object, Guid>();

            Guid MakeGuid(object obj)
            {
                if (!guids.TryGetValue(obj, out Guid id))
                {
                    id = Guid.NewGuid();
                    guids.Add(obj, id);
                }
                return id;
            }

            foreach (var deck in decks)
            {
                deck.Id = MakeGuid(deck);
                deck.ParentDeckId = deck.ParentDeck == null ? null : MakeGuid(deck.ParentDeck);
                deck.BundleId = bundle.Id;
                deck.Bundle = bundle;

                context.Decks.Add(deck);

                foreach (var card in deck.Cards)
                {
                    card.Id = MakeGuid(card);
                    card.DeckId = deck.Id;
                    card.BundleId = bundle.Id;
                    card.Bundle = bundle;

                    card.CardType.Id = MakeGuid(card.CardType);
                    card.CardType.BundleId = bundle.Id;
                    card.CardType.Bundle = bundle;

                    card.CardTypeId = card.CardType.Id;

                    context.TheCards.Add(card);

                    foreach (var fieldType in card.CardType.Fields)
                    {
                        fieldType.Id = MakeGuid(fieldType);
                        fieldType.CardTypeId = card.CardType.Id;

                        context.CardFieldTypes.Add(fieldType);
                    }

                    foreach (var variant in card.CardType.Variants)
                    {
                        variant.Id = MakeGuid(variant);
                        variant.CardTypeId = card.CardType.Id;
                        variant.BundleId = bundle.Id;
                        variant.Bundle = bundle;

                        context.VariantTypes.Add(variant);
                    }

                    foreach (var cardField in card.Fields)
                    {
                        cardField.Id = MakeGuid(cardField);
                        cardField.CardFieldTypeId = cardField.CardFieldType == null ? null : MakeGuid(cardField.CardFieldType);
                        cardField.CardId = card.Id;
                        cardField.BundleId = bundle.Id;
                        cardField.Bundle = bundle;

                        context.CardFields.Add(cardField);

                        foreach (var media in cardField.Media)
                        {
                            media.Id = MakeGuid(media);
                            media.CardFieldId = cardField.Id;

                            context.Medias.Add(media);
                        }
                    }
                }
            }
        }

        private static BundleModel BundleToModel(Bundle bundle) => new BundleModel()
        {
            Id = bundle.Id,
            Name = bundle.Name
        };
    }
}