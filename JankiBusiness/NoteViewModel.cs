using LibAnkiCards;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
        }
    }
}