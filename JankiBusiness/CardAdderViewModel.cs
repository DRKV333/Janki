using LibAnkiCards;
using LibAnkiCards.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JankiBusiness
{
    public class CardAdderViewModel : ViewModel
    {
        private readonly DeckEditorPageViewModel page;

        public IEnumerable<CardType> AvailableTypes { get; }

        private CardType selectedType;
        public CardType SelectedType
        {
            get => selectedType;
            set => Set(ref selectedType, value);
        }

        public GenericCommand AddCard { get; }

        public CardAdderViewModel(Collection collection, DeckEditorPageViewModel page)
        {
            this.page = page;
            AvailableTypes = collection.CardTypes.Select(x => x.Value).ToList();
            selectedType = AvailableTypes.FirstOrDefault();

            AddCard = new GenericDelegateCommand(async (p) =>
            {
                if (page.SelectedDeck == null)
                    return;

                Note note = new Note()
                {
                    Data = "",
                    Fields = new List<string>(),
                    Guid = "",
                    LastModified = DateTime.UtcNow,
                    ShortField = "",
                    Tags = "",
                    CardTypeId = SelectedType.Id,
                    Cards = new List<Card>(),
                    UserId = -1
                };

                foreach (var item in SelectedType.Variants)
                {
                    note.Cards.Add(new Card()
                    {
                        VariantId = (int)item.Id,
                        LastModified = DateTime.UtcNow,
                        Data = "",
                        DeckId = page.SelectedDeck.Id,
                        Due = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        Queue = CardQueueType.New,
                        UserId = -1
                    });
                }

                using (IAnkiContext context = page.ContextProvider.CreateContext())
                {
                    context.Notes.Add(note);
                    await context.SaveChangesAsync();

                    NoteViewModel noteVm = new NoteViewModel(context.Collection, note);
                    page.SelectedDeck.Cards.Add(noteVm);
                    page.SelectedCard = noteVm;
                }
            });
        }
    }
}