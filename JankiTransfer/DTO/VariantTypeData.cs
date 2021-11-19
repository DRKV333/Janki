using System;

namespace JankiTransfer.DTO
{
    public class VariantTypeData
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string FrontFormat { get; set; }

        public string BackFormat { get; set; }
    }
}