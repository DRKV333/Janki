using Newtonsoft.Json;

namespace LibAnkiCards.AnkiCompat
{
    internal class Configuration
    {
        [JsonProperty("lastUnburied")]
        public int LastUnburied { get; set; } = 0;

        [JsonProperty("rollover")]
        public int RolloverHour { get; set; } = 4;

        [JsonProperty("collapseTime")]
        public int CollapseTime { get; set; } = 1200;

        [JsonProperty("newSpread")]
        public NewCardOrdering NewCardOrdering { get; set; } = NewCardOrdering.Distribute;

        [JsonProperty("dayLearnFirst")]
        public bool DayLearnFirst { get; set; } = false;
    }
}