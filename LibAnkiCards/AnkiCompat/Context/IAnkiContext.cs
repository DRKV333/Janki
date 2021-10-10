using LibAnkiCards.AnkiCompat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibAnkiCards.AnkiCompat.Context
{
    public interface IAnkiContext : IDisposable
    {
        DbSet<Card> Cards { get; set; }
        DbSet<Note> Notes { get; set; }
        DbSet<Review> Reviews { get; set; }

        Collection Collection { get; set; }

        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    }
}