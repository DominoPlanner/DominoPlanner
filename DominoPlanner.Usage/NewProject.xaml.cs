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
    /// Interaction logic for NewProject.xaml
    /// </summary>
    public partial class NewProject : Window
    {
        public NewProject(NewProjectVM npvm)
        {
            DataContext = npvm;
            InitializeComponent();
            npvm.PropertyChanged += Npvm_PropertyChanged;
        }

        private void Npvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals("Close"))
                this.Close();
        }
    }
}
