using LibAnkiCards.Converters;
using Newtonsoft.Json;

namespace LibAnkiCards.AnkiCompat
{
    [JsonConverter(typeof(ObjectToArrayJsonConverter<TodayValue>))]
    public class TodayValue
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