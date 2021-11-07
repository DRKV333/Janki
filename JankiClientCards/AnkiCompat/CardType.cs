using Newtonsoft.Json;
using System.Collections.Generic;

namespace JankiCards.AnkiCompat
{
    internal class CardType
    {
        [JsonProperty("id", Required = Required.Always)]
        public long Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("tmpls", Required = Required.Always)]
        public IList<CardVariant> Variants { get; set; }

        [JsonProperty("flds", Required = Required.Always)]
        public IList<CardField> Fields { get; set; }

        [JsonProperty("css", Required = Required.Always)]
        public string Css { get; set; }

        [JsonProperty("latexPre", Required = Required.Always)]
        public string LatexPre { get; set; }

        [JsonProperty("latexPost", Required = Required.Always)]
        public string LatexPost { get; set; }

        [JsonProperty("tags")]
        public IList<string> Tags { get; set; }
    }
}