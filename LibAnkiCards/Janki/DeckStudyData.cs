using System;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
{
    public class DeckStudyData : EntityBase
    {
        public Guid DeckId { get; set; }

        [Required]
        public Deck Deck { get; set; }

        public DateTime DayStart { get; set; }

        public int NewCardsLeftToday { get; set; }
    }
}