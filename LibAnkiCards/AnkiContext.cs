using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace LibAnkiCards
{
    public class AnkiContext : DbContext
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
                            CardTypes = new Dictionary<long, CardType>(),
                            Decks = new Dictionary<long, Deck>()
                        };
                        Collections.Add(collection);
                    }
                }

                return collection;
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