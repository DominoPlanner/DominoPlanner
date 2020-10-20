using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.UI.UserControls.View
{
    public class DominoDimensionsV : UserControl
    {
        public DominoDimensionsV()
        {
            InitializeComponent();
            this.Get<Grid>("LayoutRoot").DataContext = this;
        }


        public int TangentialWidth
        {
            get { return (int)GetValue(TangentialWidthProperty); }
            set { SetValue(TangentialWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TangentialWidth.  This enables animation, styling, binding, etc...
        public static readonly AvaloniaProperty TangentialWidthProperty =
            AvaloniaProperty.Register<DominoDimensionsV, int>(nameof(TangentialWidth), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);



        public int NormalWidth
        {
            get { return (int)GetValue(NormalWidthProperty); }
            set { SetValue(NormalWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NormalWidth.  This enables animation, styling, binding, etc...
        public static readonly AvaloniaProperty NormalWidthProperty =
            AvaloniaProperty.Register<DominoDimensionsV, int>(nameof(NormalWidth), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);



        public int NormalDistance
        {
            get { return (int)GetValue(NormalDistanceProperty); }
            set { SetValue(NormalDistanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NormalDistance.  This enables animation, styling, binding, etc...
        public static readonly AvaloniaProperty NormalDistanceProperty =
            AvaloniaProperty.Register<DominoDimensionsV, int>(nameof(NormalDistance), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);



        public int TangentialDistance
        {
            get { return (int)GetValue(TangentialDistanceProperty); }
            set { SetValue(TangentialDistanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TangentialDistance.  This enables animation, styling, binding, etc...
        public static readonly AvaloniaProperty TangentialDistanceProperty =
            AvaloniaProperty.Register<DominoDimensionsV, int>(nameof(TangentialDistance), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);



        public string TangentialDistanceText
        {
            get { return (string)GetValue(TangentialDistanceTextProperty); }
            set { SetValue(TangentialDistanceTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TangentialDistanceText.  This enables animation, styling, binding, etc...
        public static readonly AvaloniaProperty TangentialDistanceTextProperty =
            AvaloniaProperty.Register<DominoDimensionsV, string>(nameof(TangentialDistanceText), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);



        public string NormalDistanceText
        {
            get { return (string)GetValue(NormalDistanceTextProperty); }
            set { SetValue(NormalDistanceTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NormalDistanceText.  This enables animation, styling, binding, etc...
        public static readonly AvaloniaProperty NormalDistanceTextProperty =
            AvaloniaProperty.Register<DominoDimensionsV, string>(nameof(NormalDistanceText), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
