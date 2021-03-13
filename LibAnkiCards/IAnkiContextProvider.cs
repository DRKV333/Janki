namespace LibAnkiCards
{
    public interface IAnkiContextProvider
    {
        IAnkiContext CreateContext();
    }
}