using Newtonsoft.Json;

namespace LibAnkiCards.AnkiCompat
{
    internal class DeckReviewConfiguration
    {
        [JsonProperty("perDay")]
        public int PerDayLimit { get; set; } = 200;
    }
}