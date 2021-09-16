using Avalonia;
using Avalonia.Media;

namespace JankiAvalonia.Converters
{
    internal class DynamicBoxShadowProvider : AvaloniaObject
    {
        public static readonly StyledProperty<bool> IsInsetProperty =
            AvaloniaProperty.Register<DynamicBoxShadowProvider, bool>(nameof(IsInset));
        
        public static readonly StyledProperty<double> OffsetXProperty =
            AvaloniaProperty.Register<DynamicBoxShadowProvider, double>(nameof(OffsetX));
        
        public static readonly StyledProperty<double> OffsetYProperty =
            AvaloniaProperty.Register<DynamicBoxShadowProvider, double>(nameof(OffsetY));
        
        public static readonly StyledProperty<double> BlurProperty =
            AvaloniaProperty.Register<DynamicBoxShadowProvider, double>(nameof(Blur));
        
        public static readonly StyledProperty<double> SpreadProperty =
            AvaloniaProperty.Register<DynamicBoxShadowProvider, double>(nameof(Spread));

        public static readonly StyledProperty<Color> ColorProperty =
            AvaloniaProperty.Register<DynamicBoxShadowProvider, Color>(nameof(Color));

        public static readonly DirectProperty<DynamicBoxShadowProvider, BoxShadows> ShadowProperty =
            AvaloniaProperty.RegisterDirect<DynamicBoxShadowProvider, BoxShadows>(nameof(Shadow), x => x.Shadow);

        public bool IsInset
        {
            get => GetValue(IsInsetProperty);
            set => SetValue(IsInsetProperty, value);
        }
        
        public double OffsetX
        {
            get => GetValue(OffsetXProperty);
            set => SetValue(OffsetXProperty, value);
        }
        
        public double OffsetY
        {
            get => GetValue(OffsetYProperty);
            set => SetValue(OffsetYProperty, value);
        }
        
        public double Blur
        {
            get => GetValue(BlurProperty);
            set => SetValue(BlurProperty, value);
        }
        
        public double Spread
        {
            get => GetValue(SpreadProperty);
            set => SetValue(SpreadProperty, value);
        }

        public Color Color
        {
            get => GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        private BoxShadows shadow;

        public BoxShadows Shadow
        {
            get => shadow;
            private set => SetAndRaise(ShadowProperty, ref shadow, value);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            if (change.Property != ShadowProperty)
            {
                Shadow = new BoxShadows(new BoxShadow()
                {
                    IsInset = IsInset,
                    OffsetX = OffsetX,
                    OffsetY = OffsetY,
                    Blur = Blur,
                    Spread = Spread,
                    Color = Color
                });
            }

            base.OnPropertyChanged(change);
        }
    }
}