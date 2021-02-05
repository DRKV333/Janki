using Newtonsoft.Json;
using System.Linq;

namespace LibAnkiCards
{
    public class Deck
    {
        [JsonProperty("id", Required = Required.Always)]
        public long Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        public IQueryable<Card> GetCards(AnkiContext context) => context.Cards.Where(x => x.DeckId == Id);
    }
}