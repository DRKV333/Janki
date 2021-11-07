using System;
using System.ComponentModel.DataAnnotations;

namespace JankiCards.Janki
{
    public class CardStudyData : EntityBase
    {
        public Guid CardId { get; set; }

        [Required]
        public Card Card { get; set; }

        public Guid VariantId { get; set; }

        public VariantType Variant { get; set; }

        public int Reps { get; set; }

        public int CorrectRepsInARow { get; set; }

        public int IncorrectRepsInARow { get; set; }

        public double EaseFactor { get; set; }

        public int Interval { get; set; }

        public DateTime? DueNext { get; set; }

        public DateTime LastAnswerTime { get; set; }

        public int LastAnswer { get; set; } 
    }
}