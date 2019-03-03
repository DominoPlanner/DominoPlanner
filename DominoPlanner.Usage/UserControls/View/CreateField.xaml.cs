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
    /// Interaction logic for CreateField.xaml
    /// </summary>
    public partial class CreateField : UserControl
    {
        public CreateField()
        {
            this.DataContextChanged += CreateField_DataContextChanged;
            InitializeComponent();
        }

        private void CreateField_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(DataContext != null)
            {
                if(DataContext is CreateFieldVM)
                {
                    ((CreateFieldVM)DataContext).dispatcher = Dispatcher; 
                }
            }
        }
    }
}
