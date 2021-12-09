using JankiCards.Janki;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JankiWebCards.Janki.Context
{
    public class JankiWebContext : IdentityDbContext<JankiUser>
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
        public DbSet<Bundle> Bundles { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public JankiWebContext(DbContextOptions options) : base(options)
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
                    if (item.State == EntityState.Deleted)
                    {
                        item.State = EntityState.Modified;
                        entityBase.IsDeleted = true;
                    }

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

                            if (prop.IsModified && prop.OriginalValue != prop.CurrentValue && !(prop?.OriginalValue.Equals(prop.CurrentValue) ?? false))
                            {
                                AuditLog log = new AuditLog()
                                {
                                    Created = DateTime.UtcNow,
                                    LastModified = DateTime.UtcNow,
                                    ChangedId = entityBase.Id,
                                    Table = item.Entity.GetType().Name,
                                    Column = prop.Metadata.Name,
                                    OldValue = prop.OriginalValue?.ToString(),
                                    NewValue = prop.CurrentValue?.ToString()
                                };

                                AuditLogs.Add(log);
                            }
                        }
                    }
                }
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Card>().HasOne(x => x.CardType).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Card>().HasOne(x => x.Deck).WithMany(x => x.Cards).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CardStudyData>().HasOne(x => x.Card).WithOne().OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Media>().HasOne(x => x.CardField).WithMany(x => x.Media).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<VariantType>().HasOne(x => x.CardType).WithMany(x => x.Variants).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CardField>().HasOne(x => x.Card).WithMany(x => x.Fields).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Card>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<CardField>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<CardFieldType>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<CardType>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Deck>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Media>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<VariantType>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<CardStudyData>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<DeckStudyData>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Bundle>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<AuditLog>().HasQueryFilter(x => !x.IsDeleted);
        }
    }
}