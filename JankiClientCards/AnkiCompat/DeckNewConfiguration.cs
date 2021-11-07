using Newtonsoft.Json;

namespace JankiCards.AnkiCompat
{
    internal class DeckNewConfiguration
    {
        [JsonProperty("perDay")]
        public int PerDayLimit { get; set; } = 20;
    }
}