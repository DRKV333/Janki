using JankiBusiness.Abstraction;
using JankiCards.Janki;
using JankiCards.Janki.Context;
using System.Collections.Generic;
using System.Linq;

namespace JankiBusiness.ViewModels.DeckEditor
{
    public class CardAdderViewModel : ViewModel
    {
        public IEnumerable<CardType> AvailableTypes { get; private set; } = new CardType[0];

        private CardType selectedType;

        public CardType SelectedType
        {
            get => selectedType;
            set => Set(ref selectedType, value);
        }

        public GenericCommand AddCard { get; }

        public CardAdderViewModel(DeckEditorPageViewModel page)
        {
            AddCard = new GenericDelegateCommand(async (p) =>
            {
                if (page.SelectedDeck == null)
                    return;

                Card card = new Card()
                {
                    CardType = SelectedType,
                    DeckId = page.SelectedDeck.Id,
                    Fields = new List<CardField>()
                };

                using (JankiContext context = page.ContextProvider.CreateContext())
                {
                    context.CardTypes.Attach(SelectedType);
                    context.TheCards.Add(card);

                    foreach (var item in selectedType.Variants)
                    {
                        context.CardStudyDatas.Add(new CardStudyData()
                        {
                            Card = card,
                            Variant = item
                        });
                    }

                    await context.SaveChangesAsync();

                    CardViewModel noteVm = new CardViewModel(card);
                    page.SelectedDeck.Cards.Add(noteVm);
                    page.SelectedCard = noteVm;
                }
            });
        }

        public void LoadTypes(IEnumerable<CardType> cardTypes)
        {
            AvailableTypes = cardTypes;
            SelectedType = AvailableTypes.FirstOrDefault();
        }
    }
}