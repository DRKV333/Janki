using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibAnkiCards
{
    [Table("cards")]
    public class Card
    {
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

        [Required]
        [Column("ord")]
        public int VariantId { get; set; }

        public CardVariant GetVariant(AnkiContext context) => Note.GetCardType(context).Variants[VariantId];
    }
}