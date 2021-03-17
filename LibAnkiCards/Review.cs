using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibAnkiCards
{
    [Table("revlog")]
    public class Review
    {
        public class Configuration : IEntityTypeConfiguration<Review>
        {
            public void Configure(EntityTypeBuilder<Review> builder)
            {
                builder.Property(x => x.Type).HasConversion<int>();
            }
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("cid")]
        [ForeignKey(nameof(Card))]
        public long CardId { get; set; }

        public Card Card { get; set; }

        [Required]
        [Column("usn")]
        public int UserId { get; set; }

        [Required]
        [Column("ease")]
        public int Ease { get; set; }

        [Required]
        [Column("ivl")]
        public int Ivl { get; set; }

        [Required]
        [Column("lastIvl")]
        public int LastIvl { get; set; }

        [Required]
        [Column("factor")]
        public int Factor { get; set; }

        [Required]
        [Column("time")]
        public int Time { get; set; }

        [Required]
        [Column("type")]
        public CardLearnType Type { get; set; }
    }
}