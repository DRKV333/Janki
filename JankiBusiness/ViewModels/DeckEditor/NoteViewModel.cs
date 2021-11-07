using JankiBusiness.ViewModels.Study;
using JankiCards.Janki;
using JankiCards.Janki.Context;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JankiBusiness.ViewModels.DeckEditor
{
    public class NoteViewModel : ViewModel
    {
        public class Field : ViewModel
        {
            public CardFieldType Definition { get; }
            public CardField TheField { get; }

            private string value;

            public string Value
            {
                get => value;
                set { TheField.Content = value; Set(ref this.value, value); }
            }

            public Field(CardFieldType definition, CardField field)
            {
                Definition = definition;
                TheField = field;
                value = field.Content;
            }
        }

        private readonly Card card;

        public CardType Type { get; }

        public IEnumerable<Field> Fields { get; }
        public string ShortField { get; private set; }

        public IEnumerable<CardViewModel> Cards { get; }

        public CardCarouselViewModel CardCarousel { get; }

        private bool dirty = false;

        public NoteViewModel(Card card)
        {
            this.card = card;

            Type = card.CardType;

            foreach (var item in Type.Fields.Where(x => !card.Fields.Any(y => y.CardFieldType == x)))
            {
                card.Fields.Add(new CardField() { CardFieldType = item, Content = "" });
                dirty = true;
            }

            Fields = card.Fields.OrderBy(x => x.CardFieldType?.Order ?? int.MaxValue)
                        .Select(x => new Field(x.CardFieldType, x)).ToList();

            foreach (var item in Fields)
            {
                item.PropertyChanged += (x, y) =>
                {
                    dirty = true;
                    RaisePropertyChanged(nameof(Fields));
                };
            }

            if (Fields.Any())
            {
                SetShortField(Fields.First().Value);
                Fields.First().PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(Field.Value))
                    {
                        SetShortField(((Field)s).Value);
                    }
                };
            }

            IList<CardViewModel> cards = Type.Variants.Select(x => new CardViewModel(x, this)).ToList();
            Cards = cards;

            CardCarousel = new CardCarouselViewModel(cards);
        }

        private void SetShortField(string html)
        {
            ShortField = Regex.Replace(html, "<.*?>", "");
            RaisePropertyChanged(nameof(ShortField));
        }

        public void SaveChanges(JankiContext context)
        {
            if (!dirty)
                return;

            context.CardFields.UpdateRange(Fields.Select(x => x.TheField));
            context.TheCards.Update(card);

            dirty = false;
        }

        public void Delete(JankiContext context)
        {
            context.TheCards.Remove(card);
        }
    }
}