﻿using Newtonsoft.Json;

namespace LibAnkiCards.AnkiCompat
{
    internal class CardField
    {
        [JsonProperty("ord", Required = Required.Always)]
        public long Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
    }
}