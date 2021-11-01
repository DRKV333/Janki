using LibAnkiCards.Janki;
using System;
using System.Linq;

namespace JankiScheduler
{
    public class DueQueue : CardQueue
    {
        public override IQueryable<CardStudyData> FilterCards(DateTime now, IQueryable<CardStudyData> cards) =>
            cards.Where(x => x.DueNext <= now).OrderBy(x => x.DueNext);
    }
}