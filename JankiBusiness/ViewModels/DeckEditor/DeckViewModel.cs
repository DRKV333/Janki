using JankiBusiness.Abstraction;
using JankiCards.Janki;
using JankiCards.Janki.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace JankiBusiness.ViewModels.DeckEditor
{
    public class DeckViewModel : ViewModel
    {
        private readonly Deck deck;
        private readonly IJankiContextProvider provider;

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

        public DeckViewModel(IJankiContextProvider provider, Deck deck)
        {
            this.deck = deck;
            this.provider = provider;

            SaveCard = new GenericDelegateCommand(async (p) =>
            {
                if (!(p is NoteViewModel card))
                    return;

                using (JankiContext context = provider.CreateContext())
                {
                    card.SaveChanges(context);
                    await context.SaveChangesAsync();
                }
            });
        }

        public Guid Id => deck.Id;

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
            using (JankiContext context = provider.CreateContext())
            {
                context.Decks.Attach(deck);
                await context.Entry(deck).Collection(x => x.Cards).Query()
                    .Include(x => x.CardType).ThenInclude(x => x.Fields)
                    .Include(x => x.CardType).ThenInclude(x => x.Variants)
                    .Include(x => x.Fields)
                    .LoadAsync();
            }

            foreach (var item in deck.Cards)
            {
                Cards.Add(new NoteViewModel(item));
            }
        }

        public async Task Delete()
        {
            using (JankiContext context = provider.CreateContext())
            {
                context.Decks.Remove(deck);
                await context.SaveChangesAsync();
            }
        }
    }
}