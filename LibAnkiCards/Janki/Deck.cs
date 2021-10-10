using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
{
    public class Deck : EntityBase
    {
        [Required]
        public string Name { get; set; }

        public Guid? ParentDeckId { get; set; }

        public Deck ParentDeck { get; set; }

        public IList<Card> Cards { get; set; }
    }
}