using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace DominoPlanner.Usage.UserControls.View
{
    /// <summary>
    /// Interaktionslogik für RenderOptions.xaml
    /// </summary>
    public partial class RenderOptions : UserControl
    {
        public RenderOptions()
        {
            this.InitializeComponent();
            this.Get<Grid>("LayoutRoot").DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public bool ShowImageSize
        {
            get { return (bool)GetValue(ShowImageSizeProperty); }
            set { SetValue(ShowImageSizeProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowImageSizeProperty =
            AvaloniaProperty.Register<RenderOptions, bool>("ShowImageSize", true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public int ImageSize
        {
            get { return (int)GetValue(ImageSizeProperty); }
            set { SetValue(ImageSizeProperty, value); }
        }

        // Using a AvaloniaProperty as the backing store for imageSize.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<int> ImageSizeProperty =
            AvaloniaProperty.Register<RenderOptions, int>("ImageSize", 0, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);



        public int MaxSize
        {
            get { return (int)GetValue(MaxSizeProperty); }
            set { SetValue(MaxSizeProperty, value); }
        }

        // Using a AvaloniaProperty as the backing store for MaxSize.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<int> MaxSizeProperty =
            AvaloniaProperty.Register<RenderOptions, int>("MaxSize", 2000, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);



        public bool Collapse
        {
            get { return (bool)GetValue(CollapseProperty); }
            set { SetValue(CollapseProperty, value); }
        }

        // Using a AvaloniaProperty as the backing store for Collapse.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<bool> CollapseProperty =
            AvaloniaProperty.Register<RenderOptions, bool>("Collapse", false, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public bool Collapsible
        {
            get { return (bool)GetValue(CollapsibleProperty); }
            set { SetValue(CollapsibleProperty, value); }
        }

        // Using a AvaloniaProperty as the backing store for Collapsible.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<bool> CollapsibleProperty =
            AvaloniaProperty.Register<RenderOptions, bool>("Collapsible", false, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        // Using a AvaloniaProperty as the backing store for BackgroundColor.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<Color> BackgroundColorProperty =
            AvaloniaProperty.Register<RenderOptions, Color>("BackgroundColor", Colors.Transparent, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);



        public bool DrawBorders
        {
            get { return (bool)GetValue(DrawBordersProperty); }
            set { SetValue(DrawBordersProperty, value); }
        }

        // Using a AvaloniaProperty as the backing store for DrawBorders.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<bool> DrawBordersProperty =
            AvaloniaProperty.Register<RenderOptions, bool>("DrawBorders",  false, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);


    }
}
