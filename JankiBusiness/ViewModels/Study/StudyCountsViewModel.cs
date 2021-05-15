using LibAnkiScheduler;

namespace JankiBusiness.ViewModels.Study
{
    public class StudyCountsViewModel : ViewModel
    {
        public int NewCount { get; private set; }
        public int DueCount { get; private set; }
        public int ReviewCount { get; private set; }
        public int Total { get; private set; }

        public void FillCounts(IScheduler scheduler)
        {
            NewCount = scheduler.NewCount;
            RaisePropertyChanged(nameof(NewCount));
            DueCount = scheduler.DueCount;
            RaisePropertyChanged(nameof(DueCount));
            ReviewCount = scheduler.ReviewCount;
            RaisePropertyChanged(nameof(ReviewCount));
            Total = NewCount + DueCount + ReviewCount;
            RaisePropertyChanged(nameof(Total));
        }
    }
}