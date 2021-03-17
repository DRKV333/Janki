using LibAnkiCards;
using LibAnkiCards.Context;
using LibAnkiCardsTests;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibAnkiSchedulerTests
{
    public class MemeoryAnkiContextProvider : IAnkiContextProvider, IDisposable
    {
#pragma warning disable S3881 // "IDisposable" should be implemented correctly

        private class NoDisposeAnkiContext : IAnkiContext
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
        {
            private readonly IAnkiContext context;

            public NoDisposeAnkiContext(IAnkiContext context) => this.context = context;

            public DbSet<Card> Cards { get => context.Cards; set => context.Cards = value; }
            public DbSet<Note> Notes { get => context.Notes; set => context.Notes = value; }
            public DbSet<Review> Reviews { get => context.Reviews; set => context.Reviews = value; }
            public Collection Collection { get => context.Collection; set => context.Collection = value; }

            public void Dispose()
            {
                // Will be disposed by provider.
            }

            public int SaveChanges() => context.SaveChanges();

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
        }

        private readonly IAnkiContext context;
        private readonly IAnkiContext noDisposeContext;
        private bool disposedValue;

        public MemeoryAnkiContextProvider()
        {
            context = new MemoryAnkiContext();
            noDisposeContext = new NoDisposeAnkiContext(context);
        }

        public IAnkiContext CreateContext() => disposedValue ? throw new ObjectDisposedException(nameof(MemeoryAnkiContextProvider)) : noDisposeContext;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    context.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}