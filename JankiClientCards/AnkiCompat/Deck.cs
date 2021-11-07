using JankiCards.AnkiCompat.Context;
using Newtonsoft.Json;
using System.Linq;

namespace JankiCards.AnkiCompat
{
    internal class Deck
    {
        [JsonProperty("id", Required = Required.Always)]
        public long Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        public IQueryable<Card> GetCards(IAnkiContext context) => context.Cards.Where(x => x.DeckId == Id);

        [JsonProperty("lrnToday")]
        public TodayValue LrnToday { get; set; } = new TodayValue();

        [JsonProperty("revToday")]
        public TodayValue RevToday { get; set; } = new TodayValue();

        [JsonProperty("newToday")]
        public TodayValue NewToday { get; set; } = new TodayValue();

        [JsonProperty("timeToday")]
        public TodayValue TimeToday { get; set; } = new TodayValue();

        public void ResetToday(int today)
        {
            LrnToday.Reset(today);
            RevToday.Reset(today);
            NewToday.Reset(today);
            TimeToday.Reset(today);
        }

        [JsonProperty("conf")]
        public long ConfigurationId { get; set; } = 0;

        public DeckConfiguration GetConfiguration(Collection collection) => collection.DeckConfigurations[ConfigurationId];
    }
}