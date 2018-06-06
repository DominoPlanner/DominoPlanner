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
using System.Windows.Shapes;

namespace DominoPlanner.Usage
{
    /// <summary>
    /// Interaction logic for LiveBuildHelperV.xaml
    /// </summary>
    public partial class LiveBuildHelperV : Window
    {
        public LiveBuildHelperV()
        {
            InitializeComponent();
            this.KeyDown += LiveBuildHelperV_KeyDown;
        }

        private void LiveBuildHelperV_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Space)
                ((LiveBuildHelperVM)DataContext).PressedKey(e.Key);
        }
    }
}
