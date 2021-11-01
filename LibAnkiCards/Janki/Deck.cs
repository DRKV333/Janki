using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
{
    public class Deck : BundleItem
    {
        [Required]
        public string Name { get; set; }

        public Guid? ParentDeckId { get; set; }

        public Deck ParentDeck { get; set; }

        public IList<Card> Cards { get; set; }

        public DeckStudyData StudyData { get; set; }
    }
}