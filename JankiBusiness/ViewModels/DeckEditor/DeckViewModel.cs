using JankiBusiness.Abstraction;
using LibAnkiCards.AnkiCompat;
using LibAnkiCards.AnkiCompat.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.DeckEditor
{
    public class DeckViewModel : ViewModel
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

        public GenericCommand SaveCard { get; }

        public DeckViewModel(IAnkiContextProvider provider, Deck deck)
        {
            this.deck = deck;
            this.provider = provider;

            SaveCard = new GenericDelegateCommand(async (p) =>
            {
                if (!(p is NoteViewModel card))
                    return;

                using (IAnkiContext context = provider.CreateContext())
                {
                    card.SaveChanges(context);
                    await context.SaveChangesAsync();
                }
            });
        }

        public long Id => deck.Id;

        private string currentTerm = "";

        public async Task SetSearchTerm(string term)
        {
            if (currentTerm != "")
            {
                cards.Clear();
                await FetchCards();
            }

            currentTerm = term;

            if (currentTerm != "")
            {
                cards = new ObservableCollection<NoteViewModel>(cards.Where(x => x.ShortField.ToLower().Contains(term.ToLower())));
                RaisePropertyChanged(nameof(Cards));
            }
        }

        private async Task FetchCards()
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

        public async Task Delete()
        {
            using (IAnkiContext context = provider.CreateContext())
            {
                context.Cards.RemoveRange(deck.GetCards(context));

                Collection collection = context.Collection;
                collection.Decks.Remove(deck.Id);
                context.Collection = collection;

                await context.SaveChangesAsync();
            }
        }
    }
}