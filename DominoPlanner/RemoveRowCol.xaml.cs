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

namespace DominoPlanner
{
    /// <summary>
    /// Interaction logic for RemoveRowCol.xaml
    /// </summary>
    public partial class RemoveRowCol : Window
    {
        public RemoveRC removeRC { get; set; }
        public RemoveRowCol()
        {
            InitializeComponent();
            removeRC = RemoveRC.none;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            removeRC = RemoveRC.Row;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            removeRC = RemoveRC.Column;
            this.Close();
        }
    }

    public enum RemoveRC
    {
        Row, Column, none
    }
}
