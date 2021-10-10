using System;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
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