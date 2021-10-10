using Newtonsoft.Json;

namespace LibAnkiCards.AnkiCompat
{
    public class DeckReviewConfiguration
    {
        [JsonProperty("perDay")]
        public int PerDayLimit { get; set; } = 200;
    }
}