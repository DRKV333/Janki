using JankiBusiness;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Janki
{
    public sealed partial class CardRenderer : UserControl
    {
        public enum Side
        {
            Front,
            Back
        }

        public CardRenderer()
        {
            InitializeComponent();
        }

        public Side DisplayedSide
        {
            get { return (Side)GetValue(DisplayedSideProperty); }
            set { SetValue(DisplayedSideProperty, value); }
        }

        public static readonly DependencyProperty DisplayedSideProperty =
            DependencyProperty.Register(nameof(DisplayedSide), typeof(Side), typeof(CardRenderer), new PropertyMetadata(Side.Front,
                (d, e) => ((CardRenderer)d).Render()));

        public CardViewModel Card
        {
            get { return (CardViewModel)GetValue(CardProperty); }
            set { SetValue(CardProperty, value); }
        }

        public static readonly DependencyProperty CardProperty =
            DependencyProperty.Register(nameof(Card), typeof(CardViewModel), typeof(CardRenderer), new PropertyMetadata(null,
                (d, e) =>
                {
                    CardRenderer sender = (CardRenderer)d;

                    if (e.OldValue != null)
                        ((CardViewModel)e.OldValue).PropertyChanged -= sender.OnCardPropertyChanged;

                    if (e.NewValue != null)
                        ((CardViewModel)e.NewValue).PropertyChanged += sender.OnCardPropertyChanged;

                    sender.Render();
                }));

        private void OnCardPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DisplayedSide == Side.Front && e.PropertyName == nameof(CardViewModel.FrontHtml))
                Render();
            else if (DisplayedSide == Side.Back && e.PropertyName == nameof(CardViewModel.BackHtml))
                Render();
        }

        private void Render()
        {
            if (Card == null)
                web.NavigateToString("");

            if (DisplayedSide == Side.Front)
                web.NavigateToString(Card.FrontHtml);
            else if (DisplayedSide == Side.Back)
                web.NavigateToString(Card.BackHtml);
        }
    }
}