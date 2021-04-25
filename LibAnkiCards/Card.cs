using LibAnkiCards.Context;
using LibAnkiCards.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace LibAnkiCards
{
    [Table("cards")]
    public class Card
    {
        public class Configuration : IEntityTypeConfiguration<Card>
        {
            public void Configure(EntityTypeBuilder<Card> builder)
            {
                builder.Property(x => x.LastModified).HasConversion(UnixDateTimeValueConverter.Instance);
                builder.Property(x => x.Queue).HasConversion<int>();
                builder.Property(x => x.Type).HasConversion<int>();
            }
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("nid")]
        [ForeignKey(nameof(Note))]
        public long NoteId { get; set; }

        public Note Note { get; set; }

        [Required]
        [Column("did")]
        public long DeckId { get; set; }

        public Deck GetDeck(IAnkiContext context) => GetDeck(context.Collection);

        public Deck GetDeck(Collection collection) => collection.Decks[DeckId];

        [Required]
        [Column("ord")]
        public int VariantId { get; set; }

        public CardVariant GetVariant(IAnkiContext context) => Note.GetCardType(context).Variants.FirstOrDefault(x => x.Id == VariantId);
        public CardVariant GetVariant(Collection collection) => Note.GetCardType(collection).Variants.FirstOrDefault(x => x.Id == VariantId);

        [Required]
        [Column("mod")]
        public DateTime LastModified { get; set; }

        [Required]
        [Column("usn")]
        public int UserId { get; set; }

        [Required]
        [Column("type")]
        public CardLearnType Type { get; set; }

        [Required]
        [Column("queue")]
        public CardQueueType Queue { get; set; }

        [Required]
        [Column("due")]
        public long Due { get; set; }

        [Required]
        [Column("ivl")]
        public int Ivl { get; set; }

        [NotMapped]
        public int LastIvl { get; set; }

        [Required]
        [Column("factor")]
        public int Factor { get; set; }

        [Required]
        [Column("reps")]
        public int Reps { get; set; }

        [Required]
        [Column("lapses")]
        public int Lapses { get; set; }

        [Required]
        [Column("left")]
        public int Left { get; set; }

        [Required]
        [Column("odue")]
        public int OriginalDue { get; set; }

        [Required]
        [Column("odid")]
        public int OriginalDeckId { get; set; }

        [Required]
        [Column("flags")]
        public int Flags { get; set; }

        [Required]
        [Column("data")]
        public string Data { get; set; }

        public List<Review> Reviews { get; set; }
    }
}