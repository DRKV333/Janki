using JankiBusiness.ViewModels.Study;
using LibAnkiCards.AnkiCompat;
using LibAnkiCards.AnkiCompat.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JankiBusiness.ViewModels.DeckEditor
{
    public class NoteViewModel : ViewModel
    {
        public class Field : ViewModel
        {
            public CardField Definition { get; }

            private string value = "";

            public string Value
            {
                get => value;
                set => Set(ref this.value, value);
            }

            public Field(CardField definition)
            {
                Definition = definition;
            }
        }

        private readonly Note note;

        public CardType Type { get; }

        public IEnumerable<Field> Fields { get; }
        public string ShortField { get; private set; }

        public IEnumerable<CardViewModel> Cards { get; }

        public CardCarouselViewModel CardCarousel { get; }

        private bool dirty = false;

        public NoteViewModel(Collection collection, Note note)
        {
            this.note = note;

            Type = note.GetCardType(collection);

            while (note.Fields.Count < Type.Fields.Count)
            {
                note.Fields.Add("");
                dirty = true;
            }

            Fields = Type.Fields.OrderBy(x => x.Id).Zip(note.Fields, (x, y) => new Field(x) { Value = y }).ToArray();

            foreach (var item in Fields)
            {
                item.PropertyChanged += (x, y) =>
                {
                    dirty = true;
                    RaisePropertyChanged(nameof(Fields));
                };
            }

            ShortField = note.ShortField;

            if (Fields.Any())
            {
                Fields.First().PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(Field.Value))
                    {
                        ShortField = Regex.Replace(((Field)s).Value, "<.*?>", "");
                        RaisePropertyChanged(nameof(ShortField));
                    }
                };
            }

            IList<CardViewModel> cards = note.Cards.Select(x => new CardViewModel(collection, x, this)).ToList();
            Cards = cards;

            CardCarousel = new CardCarouselViewModel(cards);
        }

        public void SaveChanges(IAnkiContext context)
        {
            if (!dirty)
                return;

            note.Fields = Fields.Select(x => x.Value).ToList();
            note.ShortField = ShortField;
            note.LastModified = DateTime.UtcNow;

            context.Notes.Update(note);

            dirty = false;
        }

        public void Delete(IAnkiContext context)
        {
            context.Notes.Remove(note);
        }
    }
}