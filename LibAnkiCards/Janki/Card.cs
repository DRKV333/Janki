using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
{
    public class Card : BundleItem
    {
        public Guid CardTypeId { get; set; }

        [Required]
        public CardType CardType { get; set; }

        public string Tags { get; set; }

        public IList<CardField> Fields { get; set; }

        public Guid DeckId { get; set; }

        [Required]
        public Deck Deck { get; set; }
    }
}