using IronPython.Hosting;
using LibAnkiCards;
using LibAnkiCards.Context;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LibAnkiScheduler
{
    public class PythonScheduler : IScheduler
    {
        private readonly IAnkiContextProvider contextProvider;

        private readonly dynamic py;
        private readonly PythonSchedulerCs cs;

        public PythonScheduler(IAnkiContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;

            cs = new PythonSchedulerCs(contextProvider);
            py = CreatePy();
        }

        private dynamic CreatePy()
        {
            ScriptEngine pythonEngine = Python.CreateEngine();
            LanguageContext pythonContext = HostingHelpers.GetLanguageContext(pythonEngine);
            ScriptScope scope = pythonEngine.CreateScope();

            pythonContext.DomainManager.LoadAssembly(Assembly.GetAssembly(typeof(DLRCachedCode)));

            scope.ImportModule("schedv2");

            dynamic schedv2 = scope.GetVariable("schedv2");
            return pythonEngine.Operations.CreateInstance(schedv2.Scheduler, cs);
        }

        public string Name => py.name;

        public void SetActiveDecks(IEnumerable<Deck> decks)
        {
            cs.ActiveDecks.Clear();
            if (cs.SelectedDeck != null)
                cs.ActiveDecks.Add(cs.SelectedDeck.Id);
            cs.ActiveDecks.AddRange(decks.Select(x => x.Id));
        }

        public void SetSelectedDeck(Deck deck)
        {
            cs.SelectedDeck = deck;
            cs.ActiveDecks.Clear();
            cs.ActiveDecks.Add(deck.Id);
        }

        public void Reset() => py.reset();

        public Card GetCard() => py.getCard();

        public void AnswerCard(Card card, int ease) => py.answerCard(card, ease);
    }
}