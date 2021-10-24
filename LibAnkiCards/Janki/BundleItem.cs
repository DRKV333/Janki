using System;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
{
    public abstract class BundleItem : EntityBase
    {
        public Guid BundleId { get; set; }

        [Required]
        public Bundle Bundle { get; set; }
    }
}