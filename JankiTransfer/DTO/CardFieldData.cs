using System;
using System.Collections.Generic;

namespace JankiTransfer.DTO
{
    public class CardFieldData
    {
        public Guid Id { get; set; }

        public Guid? CardFieldTypeId { get; set; }

        public string Content { get; set; }

        public IList<Guid> Media { get; set; }
    }
}