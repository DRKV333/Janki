namespace LibAnkiCards.Janki.Context
{
    internal class SQLiteAnkiContextProvider : IJankiContextProvider
    {
        private readonly string path;
        private readonly bool readOnly;

        public SQLiteAnkiContextProvider(string path, bool readOnly = false)
        {
            this.path = path;
            this.readOnly = readOnly;
        }

        public JankiContext CreateContext() => JankiContext.OpenSQLite(path, readOnly);
    }
}