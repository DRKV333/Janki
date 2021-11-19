using System;
using System.Collections.Generic;

namespace JankiTransfer.DTO
{
    public class ChangeData
    {
        public IList<CardTypeData> CardTypesAdded { get; set; }
        public IList<CardData> CardsAdded { get; set; }
        public IList<DeckData> DecksAdded { get; set; }

        public IList<CardTypeData> CardTypesChanged { get; set; }
        public IList<CardData> CardsChanged { get; set; }

        public IList<Guid> CardTypesRemoved { get; set; }
        public IList<Guid> CardsRemoved { get; set; }
        public IList<Guid> DecksRemoved { get; set; }
    }
}