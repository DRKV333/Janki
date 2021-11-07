using System;
using System.ComponentModel.DataAnnotations;

namespace JankiCards.Janki
{
    public class Media : BundleItem
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string FilePath { get; set; }

        public Guid CardFieldId { get; set; }

        [Required]
        public CardField CardField { get; set; }
    }
}