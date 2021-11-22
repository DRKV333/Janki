using System;
using System.Collections.Generic;

namespace JankiTransfer.DTO
{
    public class PublishData
    {
        public IList<Guid> DeckIds { get; set; }

        public string Name { get; set; }
    }
}