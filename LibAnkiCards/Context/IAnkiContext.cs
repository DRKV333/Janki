using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibAnkiCards.Context
{
    public interface IAnkiContext : IDisposable
    {
        DbSet<Card> Cards { get; set; }
        DbSet<Note> Notes { get; set; }
        DbSet<Review> Reviews { get; set; }

        Collection Collection { get; set; }

        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}