namespace LibAnkiCards.Context
{
    public interface IAnkiContextProvider
    {
        IAnkiContext CreateContext();
    }
}