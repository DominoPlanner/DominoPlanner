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
using System.IO;
using System.Windows.Forms;

namespace DominoPlanner
{
    /// <summary>
    /// Interaktionslogik für FirstStartup.xaml
    /// </summary>
    public partial class FirstStart : Window
    {
        public string path { get; private set; }
        

        public FirstStart()
        {
            InitializeComponent();
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            
            path = PathTextBox.Text;
            this.Close();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            PathTextBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Domino Projects") ;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            fb.Description = "Please select a super folder for your domino projects as default path. \nYou can change this later in \"User Settings\".";
            if (fb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PathTextBox.Text = fb.SelectedPath;
            }

        }
    }
}
