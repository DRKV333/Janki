using LibAnkiCards;
using LibAnkiCards.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace JankiBusiness
{
    public class DeckViewModel
    {
        private readonly Deck deck;
        private readonly IAnkiContextProvider provider;

        private ObservableCollection<NoteViewModel> cards;
        public ObservableCollection<NoteViewModel> Cards
        {
            get
            {
                if (cards == null)
                {
                    cards = new ObservableCollection<NoteViewModel>();
                    FetchCards();
                }
                return cards;
            }
        }

        public string Name => deck.Name;

        public DeckViewModel(IAnkiContextProvider provider, Deck deck)
        {
            this.deck = deck;
            this.provider = provider;
        }

        private async void FetchCards()
        {
            List<Note> notes;
            Collection collection;

            using (IAnkiContext context = provider.CreateContext())
            {
                notes = await deck.GetCards(context).Include(x => x.Note).ThenInclude(x => x.Cards).Select(x => x.Note).Distinct().ToListAsync();
                collection = context.Collection;
            }

            foreach (var item in notes)
            {
                Cards.Add(new NoteViewModel(collection, item));
            }
        }
    }
}
