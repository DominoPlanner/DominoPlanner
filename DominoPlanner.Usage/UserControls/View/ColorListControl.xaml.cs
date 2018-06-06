using DominoPlanner.Usage.UserControls.ViewModel;
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
    /// Interaction logic for ColorListControl.xaml
    /// </summary>
    public partial class ColorListControl : UserControl
    {
        public ColorListControl()
        {
            InitializeComponent();
        }
        public ColorListControl(ColorListControlVM clcvm)
        {
            DataContext = clcvm;
        }

        private void Color_Delete(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Delete!");
        }

        private void Color_Edit(object sender, RoutedEventArgs e)
        {

        }

        private void Color_MoveUp(object sender, RoutedEventArgs e)
        {

        }

        private void Color_MoveDown(object sender, RoutedEventArgs e)
        {

        }
    }
}
