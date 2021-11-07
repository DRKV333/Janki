namespace JankiCards.AnkiCompat.Context
{
    internal interface IAnkiContextProvider
    {
        IAnkiContext CreateContext();
    }
}