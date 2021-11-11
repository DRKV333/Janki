using System;
using System.Collections.Generic;

namespace JankiWeb.Models
{
    public class DeckTreeModel
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public IList<DeckTreeModel> Children { get; set; }
    }
}