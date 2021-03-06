using LibAnkiCards.Importing;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LibAnkiCardsTests
{
    internal class MockMediaImporter : IMediaImporter
    {
        public List<string> Imported { get; } = new List<string>();

        public Task ImportMedia(string name, Stream content)
        {
            Imported.Add(name);
            return Task.CompletedTask;
        }
    }
}