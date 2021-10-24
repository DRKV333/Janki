using LibAnkiCards.AnkiCompat;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibAnkiCards.AnkiCompat.Context
{
    internal class AnkiContext : DbContext, IAnkiContext
    {
        private Collection collection;

        public DbSet<Card> Cards { get; set; }
        protected DbSet<Collection> Collections { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Review> Reviews { get; set; }

        //If support for multiple users becomes a thing, this could change.
        //AnkiContext would probably only expose objects for one user.
        public Collection Collection
        {
            get
            {
                if (collection == null)
                {
                    collection = Collections.SingleOrDefault();

                    if (collection == null)
                    {
                        collection = new Collection()
                        {
                            Created = DateTime.UtcNow,
                            Configuration = new Configuration(),
                            CardTypes = new Dictionary<long, CardType>(),
                            Decks = new Dictionary<long, Deck>(),
                            DeckConfigurations = new Dictionary<long, DeckConfiguration>()
                        };
                        Collections.Add(collection);
                    }
                }

                return collection;
            }

            set
            {
                if (Collection.Id != default)
                    Collections.Update(value);
                collection = value;
            }
        }

        public AnkiContext(DbContextOptions options) : base(options)
        {
        }

        public static AnkiContext OpenSQLite(string path, bool readOnly = false)
        {
            string connection = new SqliteConnectionStringBuilder()
            {
                DataSource = path,
                Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWriteCreate
            }.ToString();
            DbContextOptions options = new DbContextOptionsBuilder().UseSqlite(connection).Options;
            return new AnkiContext(options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnkiContext).Assembly);
        }
    }
}