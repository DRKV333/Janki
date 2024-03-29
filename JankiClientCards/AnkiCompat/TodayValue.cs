﻿using JankiCards.Converters;
using Newtonsoft.Json;

namespace JankiCards.AnkiCompat
{
    [JsonConverter(typeof(ObjectToArrayJsonConverter<TodayValue>))]
    internal class TodayValue
    {
        [JsonProperty(Order = 1)]
        public int Today { get; set; }

        [JsonProperty(Order = 2)]
        public int Value { get; set; }

        public void Reset(int today)
        {
            if (Today != today)
                Value = 0;
        }
    }
}