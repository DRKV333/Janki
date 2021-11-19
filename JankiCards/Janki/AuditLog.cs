using System;

namespace JankiCards.Janki
{
    public class AuditLog : EntityBase
    {
        public Guid ChangedId { get; set; }

        public string Table { get; set; }

        public string Column { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }
    }
}