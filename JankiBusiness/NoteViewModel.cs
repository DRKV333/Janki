using LibAnkiCards;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace JankiBusiness
{
    public class NoteViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class Field : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public CardField Definition { get; }

            private string value = "";

            public string Value
            {
                get => value;
                set
                {
                    this.value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
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
                item.PropertyChanged += (x, y) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Fields)));
            }

            ShortField = note.ShortField;

            if (Fields.Any())
            {
                Fields.First().PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(Field.Value))
                    {
                        ShortField = Regex.Replace(((Field)s).Value, "<.*?>", "");
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShortField)));
                    }
                };
            }

            Cards = note.Cards.Select(x => new CardViewModel(collection, x, this)).ToList();
        }
    }
}