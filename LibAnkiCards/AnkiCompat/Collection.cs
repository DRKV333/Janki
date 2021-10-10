using LibAnkiCards.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibAnkiCards.AnkiCompat
{
    [Table("col")]
    public class Collection
    {
        public class EntityConfiguration : IEntityTypeConfiguration<Collection>
        {
            public void Configure(EntityTypeBuilder<Collection> builder)
            {
                builder.Property(x => x.Created).HasConversion(UnixDateTimeValueConverter.Instance);
                builder.Property(x => x.Configuration).HasConversion(JsonValueConverter<Configuration>.Instace);
                builder.Property(x => x.CardTypes).HasConversion(JsonValueConverter<Dictionary<long, CardType>>.Instace);
                builder.Property(x => x.Decks).HasConversion(JsonValueConverter<Dictionary<long, Deck>>.Instace);
                builder.Property(x => x.DeckConfigurations).HasConversion(JsonValueConverter<Dictionary<long, DeckConfiguration>>.Instace);
            }
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("crt")]
        public DateTime Created { get; set; }

        [Required]
        [Column("conf")]
        public Configuration Configuration { get; set; }

        [Required]
        [Column("models")]
        public Dictionary<long, CardType> CardTypes { get; set; }

        [Required]
        [Column("decks")]
        public Dictionary<long, Deck> Decks { get; set; }

        [Required]
        [Column("dconf")]
        public Dictionary<long, DeckConfiguration> DeckConfigurations { get; set; }
    }
}