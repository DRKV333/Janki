using Newtonsoft.Json;

namespace LibAnkiCards.AnkiCompat
{
    internal class DeckNewConfiguration
    {
        [JsonProperty("perDay")]
        public int PerDayLimit { get; set; } = 20;
    }
}