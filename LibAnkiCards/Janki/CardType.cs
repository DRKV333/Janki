using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
{
    public class CardType : EntityBase
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Css { get; set; }

        public string Tags { get; set; }

        public IList<CardFieldType> Fields { get; set; }

        public IList<VariantType> Variants { get; set; }
    }
}