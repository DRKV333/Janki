using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibAnkiCards
{
    [Table("notes")]
    public class Note
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

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