namespace JankiCards.AnkiCompat
{
    internal enum CardQueueType
    {
        New = 0,
        LearnRelearn = 1,
        Review = 2,
        DayLearnRelearn = 3,
        Preview = 4,
        Suspend = -1,
        SiblingBuried = -2,
        ManuallyBuries = -3
    }
}