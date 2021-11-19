using System;

namespace JankiTransfer.DTO
{
    public class DeckData
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid? ParentDeckId { get; set; }
    }
}