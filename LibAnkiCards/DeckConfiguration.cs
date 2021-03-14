using Newtonsoft.Json;

namespace LibAnkiCards
{
    public class DeckConfiguration
    {
        [JsonProperty("id", Required = Required.Always)]
        public long Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("rev")]
        public DeckReviewConfiguration ReviewConfiguration { get; set; } = new DeckReviewConfiguration();

        [JsonProperty("new")]
        public DeckNewConfiguration NewConfiguration { get; set; } = new DeckNewConfiguration();
    }
}