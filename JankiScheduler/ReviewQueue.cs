using LibAnkiCards.Janki;
using System;
using System.Linq;

namespace JankiScheduler
{
    public class ReviewQueue : CardQueue
    {
        private const int RereviewCountLimit = 5;

        public override IQueryable<CardStudyData> FilterCards(DateTime now, IQueryable<CardStudyData> cards)
        {
            DateTime oneDayAgo = now - TimeSpan.FromDays(1); 
            return cards.Where(x => x.LastAnswerTime > oneDayAgo && x.LastAnswer < 3 && x.IncorrectRepsInARow < RereviewCountLimit);
        }
    }
}