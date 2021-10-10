using System;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
{
    public class CardFieldType : EntityBase
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Order { get; set; }

        public Guid CardTypeId { get; set; }

        [Required]
        public CardType CardType { get; set; }
    }
}