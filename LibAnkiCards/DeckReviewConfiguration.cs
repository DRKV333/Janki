using Newtonsoft.Json;

namespace LibAnkiCards
{
    public class DeckReviewConfiguration
    {
        [JsonProperty("perDay")]
        public int PerDayLimit { get; set; } = 200;
    }
}