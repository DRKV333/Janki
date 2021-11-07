using System;
using System.ComponentModel.DataAnnotations;

namespace JankiCards.Janki
{
    public class VariantType : EntityBase
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string FrontFormat { get; set; }

        [Required]
        public string BackFormat { get; set; }

        public Guid CardTypeId { get; set; }

        [Required]
        public CardType CardType { get; set; }
    }
}