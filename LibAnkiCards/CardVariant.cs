using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibAnkiCards
{
    public class CardVariant
    {
        [JsonProperty("ord", Required = Required.Always)]
        public long Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("qfmt", Required = Required.Always)]
        public string FrontFormat { get; set; }

        [JsonProperty("afmt", Required = Required.Always)]
        public string BackFormat { get; set; }
    }
}
