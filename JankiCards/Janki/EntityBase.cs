using System;
using System.ComponentModel.DataAnnotations;

namespace JankiCards.Janki
{
    public abstract class EntityBase
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public DateTime LastModified { get; set; }
    }
}