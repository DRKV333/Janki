using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace LibAnkiCards
{
    [Table("notes")]
    public class Note
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("mid")]
        public long CardTypeId { get; set; }

        public CardType GetCardType(AnkiContext context) => context.Collections.First().CardTypes[CardTypeId];

        [Required]
        [Column("tags")]
        public string Tags { get; set; }

        [Required]
        [Column("flds")]
        public string Fields { get; set; }

        [Required]
        [Column("sfld")]
        public string ShortField { get; set; }
    }
}