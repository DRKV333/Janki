using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibAnkiCards
{
    [Table("notes")]
    public class Note
    {
        public class Configuration : IEntityTypeConfiguration<Note>
        {
            public void Configure(EntityTypeBuilder<Note> builder)
            {
                builder.Property(x => x.LastModified).HasConversion(UnixDateTimeValueConverter.Instance);
            }
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("guid")]
        public string Guid { get; set; }

        [Required]
        [Column("mid")]
        public long CardTypeId { get; set; }

        public CardType GetCardType(AnkiContext context) => context.Collection.CardTypes[CardTypeId];

        [Required]
        [Column("mod")]
        public DateTime LastModified { get; set; }

        [Required]
        [Column("usn")]
        public int UserId { get; set; }

        [Required]
        [Column("tags")]
        public string Tags { get; set; }

        [Required]
        [Column("flds")]
        public string Fields { get; set; }

        [Required]
        [Column("sfld")]
        public string ShortField { get; set; }

        [Required]
        [Column("csum")]
        public long Checksum { get; set; }

        [Required]
        [Column("flags")]
        public int Flags { get; set; }

        [Required]
        [Column("data")]
        public string Data { get; set; }

        public List<Card> Cards { get; set; }
    }
}