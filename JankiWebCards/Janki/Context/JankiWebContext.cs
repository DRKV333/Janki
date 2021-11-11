using JankiCards.Janki;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

        public JankiWebContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Card>().HasOne(x => x.CardType).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Card>().HasOne(x => x.Deck).WithMany(x => x.Cards).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CardStudyData>().HasOne(x => x.Card).WithOne().OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Media>().HasOne(x => x.CardField).WithMany(x => x.Media).OnDelete(DeleteBehavior.Restrict);
        }
    }
}