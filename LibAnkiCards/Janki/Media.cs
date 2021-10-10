using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibAnkiCards.Janki
{
    public class Media : EntityBase
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string FilePath { get; set; }

        public IList<CardField> Uses { get; set; }
    }
}