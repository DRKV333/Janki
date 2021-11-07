using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JankiCards.Janki
{
    public class CardType : BundleItem
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