using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibAnkiCards
{
    public class CardField
    {
        [JsonProperty("ord", Required = Required.Always)]
        public long Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
    }
}
