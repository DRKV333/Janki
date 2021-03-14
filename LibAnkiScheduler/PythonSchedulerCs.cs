using IronPython.Runtime;
using LibAnkiCards;
using LibAnkiCards.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibAnkiScheduler
{
    // This class has to be public, otherwise IronPython won't be able to call into it.
    public class PythonSchedulerCs
    {
        private readonly IAnkiContextProvider contextProvider;

        public readonly List<long> ActiveDecks = new List<long>();
        public Deck SelectedDeck { get; set; }

        private readonly Dictionary<long, long> cardTimers = new Dictionary<long, long>();

        internal PythonSchedulerCs(IAnkiContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;

            using (IAnkiContext context = contextProvider.CreateContext())
            {
                Collection = context.Collection;
            }
        }

        public Collection Collection { get; }

        public void SaveCollection()
        {
            using (IAnkiContext context = contextProvider.CreateContext())
            {
                context.Collection = Collection;
                context.SaveChanges();
            }
        }

        #region Daily cutoff

        public void ResetToday(int today)
        {
            foreach (var item in Collection.Decks)
            {
                item.Value.ResetToday(today);
            }
        }

        public long Time => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        private DateTime DateWithRollover(DateTime date) => new DateTime(date.Year, date.Month, date.Day,
                                                                         Collection.Configuration.RolloverHour, 0, 0, 0);

        public int DaysSinceCreation() => (DateTime.UtcNow - DateWithRollover(Collection.Created)).Days;

        public long DayCutoff()
        {
            DateTime cutoff = DateWithRollover(DateTime.UtcNow);
            if (cutoff < DateTime.UtcNow)
                cutoff = cutoff.AddDays(1);
            return ((DateTimeOffset)cutoff).ToUnixTimeSeconds();
        }

        #endregion Daily cutoff

        #region Fetching the next card

        public Card GetCard(long id)
        {
            using (IAnkiContext context = contextProvider.CreateContext())
            {
                return context.Cards.Single(x => x.Id == id);
            }
        }
        
        public void CardStartTimer(Card card) => cardTimers[card.Id] = Time;

        #endregion

        #region Learning queues

        public int LearnCount(int lrnCutoff, int today)
        {
            using (IAnkiContext context = contextProvider.CreateContext())
            {
                int count = context.Cards.Count(x => ActiveDecks.Contains(x.DeckId) && x.Queue == CardQueueType.LearnRelearn && x.Due < lrnCutoff);
                count += context.Cards.Count(x => ActiveDecks.Contains(x.DeckId) && x.Queue == CardQueueType.DayLearnRelearn && x.Due <= today);
                count += context.Cards.Count(x => ActiveDecks.Contains(x.DeckId) && x.Queue == CardQueueType.Preview);
                return count;
            }
        }

        public PythonList QueryLearnQueue(int cutoff, int limit)
        {
            PythonList list = new PythonList();

            using (IAnkiContext context = contextProvider.CreateContext())
            {
                list.__init__(
                    context.Cards.Where(x => ActiveDecks.Contains(x.DeckId) &&
                                             (x.Queue == CardQueueType.LearnRelearn || x.Queue == CardQueueType.Preview) &&
                                             x.Due < cutoff)
                                 .OrderBy(x => x.Due)
                                 .Select(x => new { x.Due, x.Id })
                                 .Take(limit)
                                 .AsEnumerable()
                                 .Select(x => new PythonTuple(new object[] { x.Due, x.Id }))
                );
            }

            return list;
        }

        #endregion Learning queues

        #region Review

        public int DeckRevLimitSingle(Deck deck, int? parentLimit = null)
        {
            // TODO: dyn
            // TODO: Nested Decks

            return Math.Max(0, deck.GetConfiguration(Collection).ReviewConfiguration.PerDayLimit - deck.RevToday.Value);
        }

        public int RevCount(int today, int limit)
        {
            using (IAnkiContext context = contextProvider.CreateContext())
            {
                return Math.Min(limit, context.Cards.Count(x => ActiveDecks.Contains(x.DeckId) && x.Queue == CardQueueType.Review && x.Due <= today));
            }
        }

        #endregion Review

        #region New Cards

        public int NewCount()
        {
            using (IAnkiContext context = contextProvider.CreateContext())
            {
                // TODO: New limits for nested decks should accumulate
                return Math.Min(Math.Max(0, SelectedDeck.GetConfiguration(Collection).NewConfiguration.PerDayLimit - SelectedDeck.NewToday.Value),
                                context.Cards.Count(x => ActiveDecks.Contains(x.DeckId) && x.Queue == CardQueueType.New));
            }
        }

        #endregion New Cards
    }
}