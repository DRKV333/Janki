using IronPython.Runtime;
using LibAnkiCards.AnkiCompat;
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

        public PythonList MakeCopyOfActives()
        {
            PythonList list = new PythonList();
            list.__init__(ActiveDecks);
            return list;
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

        public int CardTimeTaken(Card card)
        {
            int maxTaken = card.GetDeck(Collection).GetConfiguration(Collection).MaxTimeTaken;

            if (cardTimers.TryGetValue(card.Id, out long startTime))
            {
                return Math.Min((int)(Time - startTime) * 1000, maxTaken);
            }

            return maxTaken;
        }

        public int LeftToday(Card card, int dayCutoff, int? left = null, long? now = null)
        {
            float[] delays = card.GetDeck(Collection).GetConfiguration(Collection).Delays;

            if (now == null)
                now = Time;
            if (left == null)
                left = delays.Length;

            int ok = 0;

            for (int i = Math.Max(0, delays.Length - left.Value); i < delays.Length; i++)
            {
                now += (int)(delays[i] * 60);
                if (now > dayCutoff)
                    break;
                ok = i;
            }

            return ok + 1;
        }

        public int DelayForRepeatingGrade(Card card, int left)
        {
            float[] delays = card.GetDeck(Collection).GetConfiguration(Collection).Delays;

            int delay1 = DelayForGrade(card, left);
            int delay2 = delays.Length > 1 ? DelayForGrade(card, left - 1) : delay1 * 2;

            return (delay1 + Math.Max(delay1, delay2)) / 2;
        }

        public int DelayForGrade(Card card, int left)
        {
            left %= 1000;

            float[] delays = card.GetDeck(Collection).GetConfiguration(Collection).Delays;

            int idx = delays.Length - left;

            if (idx < 0 || idx >= delays.Length)
                idx = 0;

            return (int)(delays[idx] * 60);
        }

        public int StartingLeft(Card card, int dayCutoff)
        {
            // TODO
            //if card.Type == CARD_TYPE_RELEARNING:
            // conf = self._lapseConf(card)
            //else:
            // conf = self._lrnConf(card)

            return card.GetDeck(Collection).GetConfiguration(Collection).Delays.Length * LeftToday(card, dayCutoff) * 1000;
        }

        public void UpdateStats(Card card, string type, int count = 1)
        {
            //TODO: parents should be update as well...

            Deck deck = card.GetDeck(Collection);

            TodayValue target;
            switch (type)
            {
                case "new": target = deck.NewToday; break;
                case "rev": target = deck.RevToday; break;
                case "time": target = deck.TimeToday; break;
                case "lrn": target = deck.LrnToday; break;
                default: throw new ArgumentException(nameof(type));
            }

            target.Value += count;

            SaveCollection();
        }

        public void FlushCard(Card card)
        {
            card.LastModified = DateTime.UtcNow;
            // TODO User

            using (IAnkiContext context = contextProvider.CreateContext())
            {
                context.Cards.Update(card);
                context.SaveChanges();
            }
        }

        #endregion Fetching the next card

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
                                 .ThenBy(x => x.Id)
                                 .Select(x => new { x.Due, x.Id })
                                 .Take(limit)
                                 .AsEnumerable()
                                 .Select(x => new PythonTuple(new object[] { x.Due, x.Id }))
                );
            }

            return list;
        }

        public PythonList QueryLearnDayQueue(int deckId, int today, int limit)
        {
            PythonList list = new PythonList();

            using (IAnkiContext context = contextProvider.CreateContext())
            {
                list.__init__(
                    context.Cards.Where(x => x.DeckId == deckId && x.Queue == CardQueueType.DayLearnRelearn && x.Due <= today)
                                 .Select(x => x.Id)
                                 .Take(limit)
                                 .AsEnumerable()
                );
            }

            Shuffle(today, list);

            return list;
        }

        private void Shuffle<T>(int seed, IList<T> list)
        {
            Random rng = new Random(seed);

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public void LogLearn(Card card, int ease, bool leaving, CardLearnType type, int lastLeft)
        {
            int lastIvl = -DelayForGrade(card, lastLeft);
            int ivl = leaving ? card.Ivl : -DelayForGrade(card, card.Left);

            Review review = new Review()
            {
                Card = card,
                UserId = -1,
                Ease = ease,
                Ivl = ivl,
                LastIvl = lastIvl,
                Factor = card.Factor,
                Time = CardTimeTaken(card),
                Type = type
            };

            using (IAnkiContext context = contextProvider.CreateContext())
            {
                context.Cards.Attach(card);
                context.Reviews.Add(review);
                context.SaveChanges();
            }
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

        public PythonList QueryReviewQueue(int today, int limit)
        {
            PythonList list = new PythonList();

            using (IAnkiContext context = contextProvider.CreateContext())
            {
                list.__init__(
                    context.Cards.Where(x => ActiveDecks.Contains(x.DeckId) && x.Queue == CardQueueType.Review && x.Due < today)
                                 .OrderByDescending(x => x.Due)
                                 .Select(x => x.Id)
                                 .Take(limit)
                                 .AsEnumerable()
                );
            }

            return list;
        }

        public void LogReview(Card card, int ease, int delay, CardLearnType type)
        {
            Review review = new Review()
            {
                Card = card,
                UserId = -1,
                Ease = ease,
                Ivl = delay == 0 ? card.Ivl : -delay,
                LastIvl = card.LastIvl,
                Factor = card.Factor,
                Time = CardTimeTaken(card),
                Type = type
            };

            using (IAnkiContext context = contextProvider.CreateContext())
            {
                context.Cards.Attach(card);
                context.Reviews.Add(review);
                context.SaveChanges();
            }
        }

        #endregion Review

        #region New Cards

        // TODO: New limits for nested decks should accumulate
        public int DeckNewLimit(long deckId) => Math.Max(0, Collection.Decks[deckId].GetConfiguration(Collection).NewConfiguration.PerDayLimit - Collection.Decks[deckId].NewToday.Value);

        public int NewCount()
        {
            using (IAnkiContext context = contextProvider.CreateContext())
            {
                return Math.Min(DeckNewLimit(SelectedDeck.Id),
                                context.Cards.Count(x => ActiveDecks.Contains(x.DeckId) && x.Queue == CardQueueType.New));
            }
        }

        public PythonList QueryNewQueue(long deckId, int limit)
        {
            PythonList list = new PythonList();

            using (IAnkiContext context = contextProvider.CreateContext())
            {
                list.__init__(
                    context.Cards.Where(x => x.DeckId == deckId && x.Queue == CardQueueType.New)
                                 .OrderByDescending(x => x.Due)
                                 .ThenByDescending(x => x.VariantId)
                                 .Select(x => x.Id)
                                 .Take(limit)
                                 .AsEnumerable()
                );
            }

            return list;
        }

        #endregion New Cards
    }
}