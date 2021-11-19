using System;
using System.Collections.Generic;

namespace JankiTransfer.DTO
{
    public class CardTypeData
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Css { get; set; }

        public string Tags { get; set; }

        public IList<CardFieldTypeData> FieldsAdded { get; set; }

        public IList<Guid> FieldsRemoved { get; set; }

        public IList<VariantTypeData> VariantsAdded { get; set; }

        public IList<VariantTypeData> VariantsChanged { get; set; }

        public IList<Guid> VariantsRemoved { get; set; }
    }
}