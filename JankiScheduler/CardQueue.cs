using LibAnkiCards.Janki;
using LibAnkiCards.Janki.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JankiScheduler
{
    public abstract class CardQueue
    {
        public virtual Task StartSession(DateTime now, JankiContext context, IList<Guid> actualDecks) => Task.CompletedTask;

        public abstract IQueryable<CardStudyData> FilterCards(DateTime now, IQueryable<CardStudyData> cards);

        public virtual Task OnCardAnswered(JankiContext context, CardStudyData card, Ease ease) => Task.CompletedTask;

        public virtual Task<int> Count(JankiContext context, IList<Guid> actualDecks, DateTime now, IQueryable<CardStudyData> cards) => FilterCards(now, cards).CountAsync();
    }
}