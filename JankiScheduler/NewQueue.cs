using JankiCards.Janki;
using JankiCards.Janki.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JankiScheduler
{
    public class NewQueue : CardQueue
    {
        private const int NewCardLimit = 10;

        public override async Task StartSession(DateTime now, JankiContext context, IList<Guid> actualDecks)
        {
            DateTime oneDayAgo = now - TimeSpan.FromDays(1);
            foreach (var item in await context.DeckStudyDatas.Where(x => actualDecks.Contains(x.DeckId) && x.DayStart < oneDayAgo).ToListAsync())
            {
                item.DayStart = now;
                item.NewCardsLeftToday = NewCardLimit;
            }
        }

        public override IQueryable<CardStudyData> FilterCards(DateTime now, IQueryable<CardStudyData> cards) =>
            cards.Where(x => x.Reps == 0 && x.Card.Deck.StudyData.NewCardsLeftToday > 0)
            .OrderBy(x => x.Card.Created);

        public override async Task<int> Count(JankiContext context, IList<Guid> actualDecks, DateTime now, IQueryable<CardStudyData> cards) =>
            Math.Min(
                await context.Decks.Where(x => actualDecks.Contains(x.Id)).SumAsync(x => x.StudyData.NewCardsLeftToday),
                await FilterCards(now, cards).CountAsync());

        public override async Task OnCardAnswered(JankiContext context, CardStudyData card, Ease ease)
        {
            DeckStudyData data = await context.DeckStudyDatas.Where(x => x.DeckId == card.Card.DeckId).SingleAsync();
            data.NewCardsLeftToday--;
        }
    }
}