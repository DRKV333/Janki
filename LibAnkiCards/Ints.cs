﻿using LibAnkiCards.Converters;
using Newtonsoft.Json;

namespace LibAnkiCards
{
    [JsonConverter(typeof(ObjectToArrayJsonConverter<Ints>))]
    public class Ints
    {
        [JsonProperty(Order = 1)]
        public int Graduate { get; set; } = 1;

        [JsonProperty(Order = 2)]
        public int EarlyRemove { get; set; } = 4;

        [JsonProperty(Order = 3)]
        public int IDontKnow { get; set; } = 0;
    }
}