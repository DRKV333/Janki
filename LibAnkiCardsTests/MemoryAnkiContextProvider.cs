using LibAnkiCards.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace LibAnkiCardsTests
{
    public class MemoryAnkiContextProvider : IAnkiContextProvider, IDisposable
    {
        private readonly SqliteConnection connection;
        private bool disposedValue;

        public MemoryAnkiContextProvider()
        {
            string connString = new SqliteConnectionStringBuilder()
            {
                DataSource = ":memory:"
            }.ToString();
            connection = new SqliteConnection(connString);
            connection.Open();
        }

        public IAnkiContext CreateContext()
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(MemoryAnkiContextProvider));

            AnkiContext context = new AnkiContext(new DbContextOptionsBuilder().UseSqlite(connection).Options);

            try
            {
                context.Database.EnsureCreated();
            }
            catch (Exception)
            {
                context.Dispose();
                throw;
            }

            return context;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    connection.Dispose();
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