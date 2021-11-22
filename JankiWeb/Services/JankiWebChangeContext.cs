using JankiCards.Janki;
using JankiTransfer.ChangeDetection;
using JankiWebCards.Janki.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JankiWeb.Services
{
    public class JankiWebChangeContext : IChangeContext<JankiWebContext>
    {
        private Guid bundleId;

        public JankiWebChangeContext(IHttpContextAccessor httpContextAccessor)
        {
            bundleId = httpContextAccessor.HttpContext.User.GetBundleId();
        }

        public void Add(JankiWebContext context, object entity) => context.Add(entity);

        public IQueryable<AuditLog> AuditLogs(JankiWebContext context) =>
            context.AuditLogs.OrderBy(x => x.Created).IgnoreQueryFilters();

        public IQueryable<CardField> CardFields(JankiWebContext context) =>
            context.CardFields.IgnoreQueryFilters().Where(x => x.BundleId == bundleId);

        public IQueryable<CardFieldType> CardFieldTypes(JankiWebContext context) => context.CardFieldTypes;

        public IQueryable<Card> Cards(JankiWebContext context) => context.TheCards
            .Include(x => x.Fields)
            .ThenInclude(x => x.Media)
            .IgnoreQueryFilters()
            .Where(x => x.BundleId == bundleId);

        public IQueryable<CardType> CardTypes(JankiWebContext context) =>
            context.CardTypes
            .Include(x => x.Fields)
            .Include(x => x.Variants)
            .IgnoreQueryFilters()
            .Where(x => x.BundleId == bundleId);

        public IQueryable<Deck> Decks(JankiWebContext context) =>
            context.Decks.IgnoreQueryFilters().Where(x => x.BundleId == bundleId);

        public void Remove(JankiWebContext context, object entity) => context.Remove(entity);

        public Task<T> SingleOrDefaultAsync<T>(IQueryable<T> query) => query.SingleOrDefaultAsync();

        public async Task<IList<T>> ToListAsync<T>(IQueryable<T> query) => await query.ToListAsync();

        public void Update(JankiWebContext context, object entity) => context.Update(entity);

        public IQueryable<VariantType> VariantTypes(JankiWebContext context) =>
            context.VariantTypes.IgnoreQueryFilters().Where(x => x.BundleId == bundleId);
    }
}