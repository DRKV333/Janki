using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
{
    public class Card : EntityBase
    {
        public Guid CardTypeId { get; set; }

        [Required]
        public CardType CardType { get; set; }

        public string Tags { get; set; }

        public IList<CardField> Fields { get; set; }

        public IList<Deck> Decks { get; set; }
    }
}