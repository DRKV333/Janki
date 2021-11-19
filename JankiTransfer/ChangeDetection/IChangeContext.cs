using JankiCards.Janki;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JankiTransfer.ChangeDetection
{
    public interface IChangeContext<TContext>
    {
        IQueryable<CardType> CardTypes(TContext context);

        IQueryable<Deck> Decks(TContext context);

        IQueryable<Card> Cards(TContext context);

        IQueryable<AuditLog> AuditLogs(TContext context);

        IQueryable<CardFieldType> CardFieldTypes(TContext context);

        IQueryable<VariantType> VariantTypes(TContext context);

        IQueryable<CardField> CardFields(TContext context);

        Task<T> SingleOrDefaultAsync<T>(IQueryable<T> query);

        Task<IList<T>> ToListAsync<T>(IQueryable<T> query);

        void Add(TContext context, object entity);

        void Update(TContext context, object entity);

        void Remove(TContext context, object entity);
    }
}