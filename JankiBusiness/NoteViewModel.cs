using LibAnkiCards;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JankiBusiness
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

        public CardType Type { get; }

        public IEnumerable<Field> Fields { get; }
        public string ShortField { get; private set; }

        public IEnumerable<CardViewModel> Cards { get; }

        public NoteViewModel(Collection collection, Note note)
        {
            Type = note.GetCardType(collection);

            while (note.Fields.Count < Type.Fields.Count)
            {
                note.Fields.Add("");
            }

            Fields = Type.Fields.Zip(note.Fields, (x, y) => new Field(x) { Value = y }).ToArray();

            foreach (var item in Fields)
            {
                item.PropertyChanged += (x, y) => RaisePropertyChanged(nameof(Fields));
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

            Cards = note.Cards.Select(x => new CardViewModel(collection, x, this)).ToList();
        }
    }
}