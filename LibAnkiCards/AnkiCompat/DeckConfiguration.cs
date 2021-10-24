using Newtonsoft.Json;

namespace LibAnkiCards.AnkiCompat
{
    internal class DeckConfiguration
    {
        [JsonProperty("id", Required = Required.Always)]
        public long Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("rev")]
        public DeckReviewConfiguration ReviewConfiguration { get; set; } = new DeckReviewConfiguration();

        [JsonProperty("new")]
        public DeckNewConfiguration NewConfiguration { get; set; } = new DeckNewConfiguration();

        [JsonProperty("delays")]
        public float[] Delays { get; set; } = new[] { 1.0f, 10.0f };

        [JsonProperty("maxTaken")]
        public int MaxTimeTaken { get; set; } = 60;

        [JsonProperty("initialFactor")]
        public int InintialFactor { get; set; } = 2500;

        [JsonProperty("ints")]
        public Ints Ints { get; set; } = new Ints();

        [JsonProperty("minInt")]
        public int MinInt { get; set; } = 1;

        [JsonProperty("mult")]
        public float Mult { get; set; } = 0;

        [JsonProperty("hardFactor")]
        public float HardFactor { get; set; } = 1.2f;

        [JsonProperty("ease4")]
        public float Ease4 { get; set; } = 1.3f;
    }
}