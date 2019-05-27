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
    /// Interaktionslogik für Properties.xaml
    /// </summary>
    public partial class PropertiesWindow : Window
    {
        public PropertiesWindow(object context)
        {
            InitializeComponent();
            this.DataContext = new PropertiesVM(context);
        }
    }
}
