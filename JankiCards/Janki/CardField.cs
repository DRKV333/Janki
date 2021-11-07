using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JankiCards.Janki
{
    public class CardField : EntityBase
    {
        public Guid CardId { get; set; }

        [Required]
        public Card Card { get; set; }

        public Guid? CardFieldTypeId { get; set; }

        public CardFieldType CardFieldType { get; set; }

        [Required]
        public string Content { get; set; }

        public IList<Media> Media { get; set; }
    }
}