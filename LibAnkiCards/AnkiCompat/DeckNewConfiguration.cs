using Newtonsoft.Json;

namespace LibAnkiCards.AnkiCompat
{
    public class DeckNewConfiguration
    {
        [JsonProperty("perDay")]
        public int PerDayLimit { get; set; } = 20;
    }
}