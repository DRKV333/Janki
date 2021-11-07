using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JankiCards.Janki.Context
{
    public class JankiContext : DbContext
    {
        public DbSet<Card> TheCards { get; set; }
        public DbSet<CardField> CardFields { get; set; }
        public DbSet<CardFieldType> CardFieldTypes { get; set; }
        public DbSet<CardType> CardTypes { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<VariantType> VariantTypes { get; set; }
        public DbSet<CardStudyData> CardStudyDatas { get; set; }
        public DbSet<DeckStudyData> DeckStudyDatas { get; set; }

        public JankiContext(DbContextOptions options) : base(options)
        {
        }

        public static JankiContext OpenSQLite(string path, bool readOnly = false)
        {
            string connection = new SqliteConnectionStringBuilder()
            {
                DataSource = path,
                Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWriteCreate
            }.ToString();
            DbContextOptions options = new DbContextOptionsBuilder().UseSqlite(connection).Options;
            return new JankiContext(options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>().Ignore(x => x.Bundle);
            modelBuilder.Entity<CardType>().Ignore(x => x.Bundle);
            modelBuilder.Entity<Deck>().Ignore(x => x.Bundle);
            modelBuilder.Entity<Media>().Ignore(x => x.Bundle);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(JankiContext).Assembly);
        }
    }
}