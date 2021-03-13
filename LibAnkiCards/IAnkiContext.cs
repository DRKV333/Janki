using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibAnkiCards
{
    public interface IAnkiContext : IDisposable
    {
        DbSet<Card> Cards { get; set; }
        DbSet<Note> Notes { get; set; }
        DbSet<Review> Reviews { get; set; }

        Collection Collection { get; }

        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}