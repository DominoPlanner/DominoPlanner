using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DominoPlanner.Usage.UserControls.View
{
    /// <summary>
    /// Interaktionslogik für RenderOptions.xaml
    /// </summary>
    public partial class RenderOptions : UserControl
    {
        public RenderOptions()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }


        public Visibility ShowImageSize
        {
            get { return (Visibility)GetValue(ShowImageSizeProperty); }
            set { SetValue(ShowImageSizeProperty, value); }
        }
        
        public static readonly DependencyProperty ShowImageSizeProperty =
            DependencyProperty.Register("ShowImageSize", typeof(Visibility), typeof(RenderOptions), new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int ImageSize
        {
            get { return (int)GetValue(ImageSizeProperty); }
            set { SetValue(ImageSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for imageSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSizeProperty =
            DependencyProperty.Register("ImageSize", typeof(int), typeof(RenderOptions), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public int MaxSize
        {
            get { return (int)GetValue(MaxSizeProperty); }
            set { SetValue(MaxSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxSizeProperty =
            DependencyProperty.Register("MaxSize", typeof(int), typeof(RenderOptions), new FrameworkPropertyMetadata(2000, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public bool Collapse
        {
            get { return (bool)GetValue(CollapseProperty); }
            set { SetValue(CollapseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Collapse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollapseProperty =
            DependencyProperty.Register("Collapse", typeof(bool), typeof(RenderOptions), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Visibility Collapsible
        {
            get { return (Visibility)GetValue(CollapsibleProperty); }
            set { SetValue(CollapsibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for visibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollapsibleProperty =
            DependencyProperty.Register("Collapsible", typeof(Visibility), typeof(RenderOptions), new FrameworkPropertyMetadata(Visibility.Collapsed, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(RenderOptions), new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public bool DrawBorders
        {
            get { return (bool)GetValue(DrawBordersProperty); }
            set { SetValue(DrawBordersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DrawBorders.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawBordersProperty =
            DependencyProperty.Register("DrawBorders", typeof(bool), typeof(RenderOptions), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



    }
}
