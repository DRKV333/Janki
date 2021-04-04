using LibAnkiCards.Context;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace JankiBusiness
{
    public class DeckEditorPageViewModel
    {
        public IAnkiContextProvider ContextProvider { get; set; }

        private ObservableCollection<DeckViewModel> decks;
        public ObservableCollection<DeckViewModel> Decks
        {
            get
            {
                if (decks == null)
                    FetchDecks();
                return decks;
            }
        }

        private void FetchDecks()
        {
            //TODO: This has to be async!
            using (IAnkiContext context = ContextProvider.CreateContext())
            {
                decks = new ObservableCollection<DeckViewModel>(context.Collection.Decks.Select(x => new DeckViewModel(context, x.Value)));
            }
        }
    }
}
