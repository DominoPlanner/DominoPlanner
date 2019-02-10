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
    /// Interaction logic for EditProject.xaml
    /// </summary>
    public partial class EditProject : UserControl
    {
        public EditProject()
        {
            InitializeComponent();
            this.KeyDown += LiveBuildHelperV_KeyDown;
        }

        private void LiveBuildHelperV_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ((EditProjectVM)DataContext).PressedKey(e.Key);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(DataContext != null && DataContext.GetType() == typeof(ViewModel.EditProjectVM))
            {
                ((ViewModel.EditProjectVM)DataContext).SizeChanged(sender, e);
            }
        }

        private void Grid_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            ((Grid)sender).ColumnDefinitions[2].Width = new GridLength(e.NewSize.Width - 240);
        }
    }
}
