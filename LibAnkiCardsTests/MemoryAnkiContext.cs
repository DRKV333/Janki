using LibAnkiCards;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LibAnkiCardsTests
{
    internal class MemoryAnkiContext : AnkiContext
    {
        public MemoryAnkiContext() : base(new DbContextOptionsBuilder().UseSqlite(CreateMemoryConnection()).Options)
        {
            Database.EnsureCreated();
        }

        private static SqliteConnection CreateMemoryConnection()
        {
            string connString = new SqliteConnectionStringBuilder()
            {
                DataSource = ":memory:"
            }.ToString();
            SqliteConnection connection = new SqliteConnection(connString);
            connection.Open();
            return connection;
        }
    }
}