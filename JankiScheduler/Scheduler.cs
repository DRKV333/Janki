using JankiCards.Janki;
using JankiCards.Janki.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JankiScheduler
{
    public class Scheduler
    {
        public int NewCount { get; private set; }
        public int DueCount { get; private set; }
        public int ReviewCount { get; private set; }

        private readonly NewQueue newQueue = new NewQueue();
        private readonly DueQueue dueQueue = new DueQueue();
        private readonly ReviewQueue reviewQueue = new ReviewQueue();

        private readonly CardQueue[] queues;
        private int nextQueue = 0;
        private int? previousQueue = null;
        private Guid prevCard = Guid.Empty;

        private readonly IJankiContextProvider contextProvider;
        private List<Guid> actualDecks = new List<Guid>();

        public Scheduler(IJankiContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;

            queues = new CardQueue[]
            {
                newQueue,
                dueQueue,
                reviewQueue
            };
        }

        public Task SelectDeck(Deck deck) => SelectDeck(deck, DateTime.UtcNow);

        public async Task SelectDeck(Deck deck, DateTime now)
        {
            nextQueue = 0;
            previousQueue = null;
            prevCard = Guid.Empty;

            using (JankiContext context = contextProvider.CreateContext())
            {
                List<Guid> childDecks = await context.Decks.Where(x => x.ParentDeckId == deck.Id).Select(x => x.Id).ToListAsync();

                if (childDecks.Count > 0)
                    actualDecks = childDecks;
                else
                    actualDecks = new List<Guid>() { deck.Id };

                foreach (var item in queues)
                {
                    await item.StartSession(now, context, actualDecks);
                }

                await context.SaveChangesAsync();

                await UpdateCounts(context, now);
            }
        }

        private async Task UpdateCounts(JankiContext context, DateTime now)
        {
            IQueryable<CardStudyData> cards = QueryCards(context);
            NewCount = await newQueue.Count(context, actualDecks, now, cards);
            DueCount = await dueQueue.Count(context, actualDecks, now, cards);
            ReviewCount = await reviewQueue.Count(context, actualDecks, now, cards);
        }

        private IQueryable<CardStudyData> QueryCards(JankiContext context) =>
            context.CardStudyDatas.Where(x => actualDecks.Contains(x.Card.DeckId))
                .Include(x => x.Card).ThenInclude(x => x.Fields)
                .Include(x => x.Card).ThenInclude(x => x.CardType).ThenInclude(x => x.Fields)
                .Include(x => x.Variant);

        public Task<CardStudyData> GetCard() => GetCard(DateTime.UtcNow);

        public async Task<CardStudyData> GetCard(DateTime now)
        {
            int startQueue = nextQueue;

            using (JankiContext context = contextProvider.CreateContext())
            {
                IQueryable<CardStudyData> cards = QueryCards(context);

                do
                {
                    int queueMaybe = nextQueue;
                    nextQueue = ++nextQueue % queues.Length;

                    CardQueue queue = queues[queueMaybe];

                    CardStudyData card = await queue.FilterCards(now, cards).FirstOrDefaultAsync();
                    if (card != null && card.Id != prevCard)
                    {
                        prevCard = card.Id;
                        previousQueue = queueMaybe;
                        return card;
                    }
                }
                while (nextQueue != startQueue);

                return null;
            }
        }

        public Task AnswerCard(CardStudyData card, Ease ease) => AnswerCard(card, ease, DateTime.UtcNow);

        public async Task AnswerCard(CardStudyData card, Ease ease, DateTime now)
        {
            card.Reps++;

            if ((int)ease >= 3)
            {
                if (card.CorrectRepsInARow == 0)
                    card.Interval = 1;
                else if (card.CorrectRepsInARow == 1)
                    card.Interval = (int)Math.Round(card.Interval * card.EaseFactor);

                card.IncorrectRepsInARow = 0;
                card.CorrectRepsInARow++;
            }
            else
            {
                card.CorrectRepsInARow = 0;
                card.IncorrectRepsInARow++;
                card.Interval = 1;
            }

            card.EaseFactor = card.EaseFactor + (0.1 - (5 - (int)ease) * (0.08 + (5 - (int)ease) * 0.02));
            if (card.EaseFactor < 1.3)
                card.EaseFactor = 1.3;

            card.LastAnswerTime = now;
            card.LastAnswer = (int)ease;
            card.DueNext = now + TimeSpan.FromDays(card.Interval);

            using (JankiContext context = contextProvider.CreateContext())
            {
                context.CardStudyDatas.Update(card);
                if (previousQueue != null)
                    await queues[previousQueue.Value].OnCardAnswered(context, card, ease);
                await context.SaveChangesAsync();
                await UpdateCounts(context, now);
            }
        }
    }
}