using LibAnkiCards.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibAnkiCards
{
    [Table("col")]
    public class Collection
    {
        public class Configuration : IEntityTypeConfiguration<Collection>
        {
            public void Configure(EntityTypeBuilder<Collection> builder)
            {
                builder.Property(x => x.CardTypes).HasConversion(JsonValueConverter<Dictionary<long, CardType>>.Instace);
                builder.Property(x => x.Decks).HasConversion(JsonValueConverter<Dictionary<long, Deck>>.Instace);
            }
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("models")]
        public Dictionary<long, CardType> CardTypes { get; set; }

        [Required]
        [Column("decks")]
        public Dictionary<long, Deck> Decks { get; set; }
    }
}