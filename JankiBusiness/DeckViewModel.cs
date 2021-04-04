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

        public ObservableCollection<NoteViewModel> Cards { get; }

        public string Name => deck.Name;

        public DeckViewModel(IAnkiContext context, Deck deck)
        {
            this.deck = deck;
            Cards = new ObservableCollection<NoteViewModel>(deck.GetCards(context).Include(x => x.Note).ThenInclude(x => x.Cards).Select(x => x.Note).Distinct().AsEnumerable().Select(x => new NoteViewModel(context.Collection, x)));
        }
    }
}
