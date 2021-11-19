using System;
using System.Collections.Generic;

namespace JankiTransfer.DTO
{
    public class CardData
    {
        public Guid Id { get; set; }

        public Guid CardTypeId { get; set; }

        public Guid DeckId { get; set; }

        public IList<CardFieldData> FieldsAdded { get; set; }

        public IList<CardFieldData> FieldsChanged { get; set; }

        public IList<Guid> FieldsRemoved { get; set; }
    }
}