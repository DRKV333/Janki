using System;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
{
    public class CardStudyData : EntityBase
    {
        public Guid CardId { get; set; }

        [Required]
        public Card Card { get; set; }

        public Guid VariantId { get; set; }

        public VariantType Variant { get; set; }
    }
}