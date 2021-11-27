using JankiBusiness.Services;
using JankiBusiness.ViewModels.Study;
using JankiCards.Janki;
using JankiCards.Janki.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                if (definition == null)
                    Definition = new CardFieldType() { Name = "Removed Field" };
                else
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
                card.Fields.Add(new CardField() { CardFieldType = item, Content = "", Media = new List<Media>() });
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

        public async Task SaveChanges(JankiContext context, IMediaUnimporter unimporter)
        {
            if (!dirty)
                return;

            foreach (var item in Fields.Select(x => x.TheField))
            {
                CardField dbField = await context.CardFields.FindAsync(item.Id);
                dbField.Content = item.Content;
            }

            foreach (var item in Fields)
            {
                List<string> currentImages = Regex.Matches(item.Value, @"src=""(.*)""").Cast<Match>().Select(x => x.Groups[1].Value).ToList();

                foreach (var newItem in currentImages.Where(x => !item.TheField.Media.Any(y => y.FilePath == x)))
                {
                    Media media = new Media()
                    {
                        CardField = item.TheField,
                        FilePath = newItem,
                        Name = newItem
                    };

                    item.TheField.Media.Add(media);
                    context.Medias.Add(media);
                }

                foreach (var oldItem in item.TheField.Media.Where(x => !currentImages.Any(y => y == x.FilePath)).ToList())
                {
                    item.TheField.Media.Remove(oldItem);
                    context.Medias.Remove(oldItem);
                    await unimporter.UnimportMedia(oldItem.FilePath);
                }
            }

            dirty = false;
        }

        public void Delete(JankiContext context)
        {
            context.TheCards.Remove(card);
        }
    }
}