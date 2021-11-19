using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        public DbSet<AuditLog> AuditLogs { get; set; }

        public JankiContext(DbContextOptions options) : base(options)
        {
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
            SaveChangesAsync(acceptAllChangesOnSuccess).Result;

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ChangeTracker.DetectChanges();

            foreach (var item in ChangeTracker.Entries())
            {
                if (item.Entity is EntityBase entityBase)
                {
                    if (item.State == EntityState.Added)
                    {
                        entityBase.Created = DateTime.UtcNow;
                        entityBase.LastModified = DateTime.UtcNow;
                    }
                    else if (item.State == EntityState.Modified)
                    {
                        entityBase.LastModified = DateTime.UtcNow;
                    }

                    if (!(item.Entity is AuditLog) && item.State == EntityState.Modified)
                    {
                        foreach (var prop in item.Properties)
                        {
                            if (prop.Metadata.Name == nameof(EntityBase.LastModified) ||
                                prop.Metadata.Name == nameof(EntityBase.IsDeleted))
                                continue;

                            if (prop.IsModified)
                            {
                                AuditLog log = new AuditLog()
                                {
                                    Created = DateTime.UtcNow,
                                    LastModified = DateTime.UtcNow,
                                    ChangedId = entityBase.Id,
                                    Table = item.Entity.GetType().Name,
                                    Column = prop.Metadata.Name,
                                    OldValue = prop.OriginalValue.ToString(),
                                    NewValue = prop.CurrentValue.ToString()
                                };

                                AuditLogs.Add(log);
                            }
                        }
                    }
                }
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CardType>().Ignore(x => x.Bundle);
            modelBuilder.Entity<Deck>().Ignore(x => x.Bundle);
            modelBuilder.Entity<CardField>().Ignore(x => x.Bundle);
            modelBuilder.Entity<Card>().Ignore(x => x.Bundle);
            modelBuilder.Entity<VariantType>().Ignore(x => x.Bundle);

            modelBuilder.Entity<CardField>().HasOne(x => x.CardFieldType).WithMany().OnDelete(DeleteBehavior.SetNull);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(JankiContext).Assembly);
        }
    }
}