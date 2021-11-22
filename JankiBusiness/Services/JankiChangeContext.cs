using JankiCards.Janki;
using JankiCards.Janki.Context;
using JankiTransfer.ChangeDetection;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JankiBusiness.Services
{
    public class JankiChangeContext : IChangeContext<JankiContext>
    {
        public void Add(JankiContext context, object entity) => context.Add(entity);

        public IQueryable<AuditLog> AuditLogs(JankiContext context) =>
            context.AuditLogs.OrderBy(x => x.Created).IgnoreQueryFilters();

        public IQueryable<CardField> CardFields(JankiContext context) => context.CardFields.IgnoreQueryFilters();

        public IQueryable<CardFieldType> CardFieldTypes(JankiContext context) => context.CardFieldTypes;

        public IQueryable<Card> Cards(JankiContext context) =>
            context.TheCards
            .Include(x => x.Fields)
            .ThenInclude(x => x.Media)
            .IgnoreQueryFilters();

        public IQueryable<CardType> CardTypes(JankiContext context) =>
            context.CardTypes
            .Include(x => x.Fields)
            .Include(x => x.Variants)
            .IgnoreQueryFilters();

        public IQueryable<Deck> Decks(JankiContext context) => context.Decks.IgnoreQueryFilters();

        public void Remove(JankiContext context, object entity) => context.Remove(entity);

        public Task<T> SingleOrDefaultAsync<T>(IQueryable<T> query) => query.SingleOrDefaultAsync();

        public async Task<IList<T>> ToListAsync<T>(IQueryable<T> query) => await query.ToListAsync();

        public void Update(JankiContext context, object entity) => context.Update(entity);

        public IQueryable<VariantType> VariantTypes(JankiContext context) => context.VariantTypes.IgnoreQueryFilters();
    }
}