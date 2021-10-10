using LibAnkiCards.AnkiCompat;
using LibAnkiCards.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibAnkiCards.Importing
{
    public class DatabaseImporter
    {
        private readonly IAnkiContext toContext;

        private readonly Dictionary<CardType, CardType> existingTypes;
        private long cardTypeNextId;

        private Dictionary<long, Deck> importedDecks;
        private long deckNextId;

        private readonly DeckConfiguration defaultConfiguration;

        public DatabaseImporter(IAnkiContext toContext)
        {
            this.toContext = toContext;

            existingTypes = new Dictionary<CardType, CardType>(CardTypeComparer.Instance);
            foreach (var item in toContext.Collection.CardTypes)
            {
                existingTypes[item.Value] = item.Value;
            }

            cardTypeNextId = GetNextDictKey(toContext.Collection.CardTypes);
            deckNextId = GetNextDictKey(toContext.Collection.Decks);

            defaultConfiguration = toContext.Collection.DeckConfigurations.FirstOrDefault().Value;
            if (defaultConfiguration == null)
            {
                defaultConfiguration = new DeckConfiguration()
                {
                    Id = 1,
                    Name = "Default"
                };
                toContext.Collection.DeckConfigurations.Add(defaultConfiguration.Id, defaultConfiguration);
            }
        }

        private long GetNextDictKey<T>(Dictionary<long, T> dict) => dict.Any() ? dict.Max(x => x.Key) + 1 : DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public async Task Import(IAnkiContext fromContext)
        {
            importedDecks = new Dictionary<long, Deck>();

            List<Note> notes = await fromContext.Notes.Include(x => x.Cards).ThenInclude(x => x.Reviews)
                                     .AsNoTracking().ToListAsync().ConfigureAwait(false);

            foreach (var note in notes)
            {
                note.Id = default;
                note.CardTypeId = RemapCardType(note.GetCardType(fromContext));

                foreach (var card in note.Cards)
                {
                    card.Id = default;
                    card.NoteId = default;

                    card.DeckId = RemapDeck(card.GetDeck(fromContext));

                    foreach (var review in card.Reviews)
                    {
                        review.Id = default;
                        review.CardId = default;
                    }
                }

                toContext.Notes.Add(note);
            }
        }

        private long RemapCardType(CardType oldType)
        {
            if (existingTypes.TryGetValue(oldType, out CardType existingType))
            {
                return existingType.Id;
            }
            else
            {
                oldType.Id = cardTypeNextId++;
                toContext.Collection.CardTypes.Add(oldType.Id, oldType);
                existingTypes.Add(oldType, oldType);
                return oldType.Id;
            }
        }

        private long RemapDeck(Deck oldDeck)
        {
            if (importedDecks.TryGetValue(oldDeck.Id, out Deck existingDeck))
            {
                return existingDeck.Id;
            }
            else
            {
                oldDeck.Id = deckNextId++;
                oldDeck.ConfigurationId = defaultConfiguration.Id;
                toContext.Collection.Decks.Add(oldDeck.Id, oldDeck);
                importedDecks.Add(oldDeck.Id, oldDeck);
                return oldDeck.Id;
            }
        }
    }
}