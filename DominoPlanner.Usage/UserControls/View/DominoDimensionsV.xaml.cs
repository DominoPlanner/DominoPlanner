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
    /// Interaktionslogik für DominoDimensionsV.xaml
    /// </summary>
    public partial class DominoDimensionsV : UserControl
    {
        public DominoDimensionsV()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }


        public int TangentialWidth
        {
            get { return (int)GetValue(TangentialWidthProperty); }
            set { SetValue(TangentialWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TangentialWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TangentialWidthProperty =
            DependencyProperty.Register("TangentialWidth", typeof(int), typeof(DominoDimensionsV), new FrameworkPropertyMetadata(8, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public int NormalWidth
        {
            get { return (int)GetValue(NormalWidthProperty); }
            set { SetValue(NormalWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NormalWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NormalWidthProperty =
            DependencyProperty.Register("NormalWidth", typeof(int), typeof(DominoDimensionsV), new FrameworkPropertyMetadata(8, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public int NormalDistance
        {
            get { return (int)GetValue(NormalDistanceProperty); }
            set { SetValue(NormalDistanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NormalDistance.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NormalDistanceProperty =
            DependencyProperty.Register("NormalDistance", typeof(int), typeof(DominoDimensionsV), new FrameworkPropertyMetadata(8, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public int TangentialDistance
        {
            get { return (int)GetValue(TangentialDistanceProperty); }
            set { SetValue(TangentialDistanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TangentialDistance.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TangentialDistanceProperty =
            DependencyProperty.Register("TangentialDistance", typeof(int), typeof(DominoDimensionsV), new FrameworkPropertyMetadata(8, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public string TangentialDistanceText
        {
            get { return (string)GetValue(TangentialDistanceTextProperty); }
            set { SetValue(TangentialDistanceTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TangentialDistanceText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TangentialDistanceTextProperty =
            DependencyProperty.Register("TangentialDistanceText", typeof(string), typeof(DominoDimensionsV), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public string NormalDistanceText
        {
            get { return (string)GetValue(NormalDistanceTextProperty); }
            set { SetValue(NormalDistanceTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NormalDistanceText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NormalDistanceTextProperty =
            DependencyProperty.Register("NormalDistanceText", typeof(string), typeof(DominoDimensionsV), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));






    }
}
