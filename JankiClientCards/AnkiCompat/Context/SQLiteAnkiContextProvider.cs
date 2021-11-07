namespace JankiCards.AnkiCompat.Context
{
    internal class SQLiteAnkiContextProvider : IAnkiContextProvider
    {
        private readonly string path;
        private readonly bool readOnly;

        public SQLiteAnkiContextProvider(string path, bool readOnly = false)
        {
            this.path = path;
            this.readOnly = readOnly;
        }

        public IAnkiContext CreateContext() => AnkiContext.OpenSQLite(path, readOnly);
    }
}