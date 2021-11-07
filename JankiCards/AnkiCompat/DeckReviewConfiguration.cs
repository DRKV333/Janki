using Newtonsoft.Json;

namespace JankiCards.AnkiCompat
{
    internal class DeckReviewConfiguration
    {
        [JsonProperty("perDay")]
        public int PerDayLimit { get; set; } = 200;
    }
}