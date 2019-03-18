using DominoPlanner.Core;
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
    /// Interaktionslogik für ExportOptions.xaml
    /// </summary>
    public partial class ExportOptions : Window
    {
        public ExportOptions(IDominoProvider provider)
        {
            InitializeComponent();
            var dc = new ExportOptionsVM(provider);
            DataContext = dc;
            dc.PropertyChanged += ExpVM_PropertyChanged;
        }

        private void ExpVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Close"))
            {
                this.DialogResult = ((ExportOptionsVM)DataContext).result;
                this.Close();
            }
        }
    }
}
