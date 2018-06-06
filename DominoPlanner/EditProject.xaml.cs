using DominoPlanner.Document_Classes;
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

namespace DominoPlanner
{
    /// <summary>
    /// Interaction logic for EditProject.xaml
    /// </summary>
    public partial class EditProject : UserControl
    {
        public static readonly DependencyProperty MainDocumentProperty = DependencyProperty.Register
            (
                 "MainDocument",
                 typeof(List<DominoColor>),
                 typeof(List<DominoColor>)
            );

        public List<DominoColor> MainDocument
        {
            get { return (List<DominoColor>)GetValue(MainDocumentProperty); }
            set { SetValue(MainDocumentProperty, value); }
        }
        
        public EditProject()
        {
            //   this.DataContext = new EditFieldViewModel(Colors);
            InitializeComponent();
        }
    }
}
