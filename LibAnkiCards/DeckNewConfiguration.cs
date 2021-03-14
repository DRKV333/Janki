using Newtonsoft.Json;

namespace LibAnkiCards
{
    public class DeckNewConfiguration
    {
        [JsonProperty("perDay")]
        public int PerDayLimit { get; set; } = 20;
    }
}