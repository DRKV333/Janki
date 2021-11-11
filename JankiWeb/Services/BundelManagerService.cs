using JankiCards.Janki;
using JankiWeb.Models;
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

        public async Task<IEnumerable<DeckTreeModel>> GetAllDecks(Guid bundleId) => await GetChildren(null);

        private async Task<IList<DeckTreeModel>> GetChildren(Guid? parentId)
        {
            List<Deck> topLevel = await context.Decks.Where(x => x.ParentDeckId == parentId).ToListAsync();
            List<DeckTreeModel> topLevelTree = topLevel.Select(x => new DeckTreeModel() { Id = x.Id, Name = x.Name }).ToList();

            foreach (var item in topLevelTree)
            {
                item.Children = await GetChildren(item.Id);
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
                Name = bundleName,
                IsPublic = true
            };

            List<Deck> decks = await IncludeEverything(context.Decks)
                .AsNoTrackingWithIdentityResolution()
                .Where(x => deckIds.Contains(x.Id))
                .ToListAsync();

            DetachAndAssingToBundle(decks, newBundle);

            context.Bundles.Add(newBundle);

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

            context.Decks.AddRange(decks);

            await context.SaveChangesAsync();
        }

        private static IQueryable<Deck> IncludeEverything(IQueryable<Deck> query) => query
            .Include(x => x.ParentDeck)
            .Include(x => x.Cards).ThenInclude(x => x.Fields).ThenInclude(x => x.Media)
            .Include(x => x.Cards).ThenInclude(x => x.CardType).ThenInclude(x => x.Fields)
            .Include(x => x.Cards).ThenInclude(x => x.CardType).ThenInclude(x => x.Variants);

        private static void DetachAndAssingToBundle(IEnumerable<Deck> decks, Bundle bundle)
        {
            foreach (var deck in decks)
            {
                deck.Id = default;
                deck.ParentDeckId = default;
                deck.BundleId = default;
                deck.Bundle = bundle;

                foreach (var card in deck.Cards)
                {
                    card.Id = default;
                    card.DeckId = default;
                    card.CardTypeId = default;

                    card.CardType.Id = default;
                    card.CardType.BundleId = default;
                    card.CardType.Bundle = bundle;

                    foreach (var fieldType in card.CardType.Fields)
                    {
                        fieldType.Id = default;
                        fieldType.CardTypeId = default;
                    }

                    foreach (var variant in card.CardType.Variants)
                    {
                        variant.Id = default;
                        variant.CardTypeId = default;
                    }

                    foreach (var cardField in card.Fields)
                    {
                        cardField.Id = default;
                        cardField.CardFieldTypeId = default;
                        cardField.CardId = default;

                        foreach (var media in cardField.Media)
                        {
                            media.Id = default;
                            media.CardFieldId = default;
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