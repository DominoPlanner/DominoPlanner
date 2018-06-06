using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaktionslogik für ColorControl.xaml
    /// </summary>
    public partial class ColorControl : Window
    {
        public int count { get; set; }
        public string name { get; set; }
        public bool ColorOnly { get; set; }
        public bool Show_Only_Color { get; set; }
        public ColorControl()
        {
            InitializeComponent();
        }

        private void Test(object sender, EventArgs e)
        {
            TbCount.Text = count + "";
            TbName.Text = name;
            if (ColorOnly)
            {
                TbName.IsEnabled = false;
                TbCount.IsEnabled = false;
            }
            if (Show_Only_Color)
            {
                TbName.Visibility = Visibility.Hidden;
                TbCount.Visibility = Visibility.Hidden;
                HideRectangle.Visibility = Visibility.Visible;
            }
        }

        private void Close(object sender, RoutedEventArgs e)
        {

                if (((string)(((Button)sender).Content)) == "Save")
                {
                    int temp = 0;
                    if (int.TryParse(TbCount.Text, out temp))
                    {
                        DialogResult = true;
                        this.Close();
                        name = TbName.Text.Replace(' ', '_');
                        count = temp;
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Please enter a number");
                        return;
                    }   
                }
            DialogResult = false;
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
