using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
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

        public IList<Media> UsedMedia { get; set; }
    }
}