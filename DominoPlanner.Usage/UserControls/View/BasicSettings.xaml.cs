using DominoPlanner.Core;
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
    /// Interaktionslogik für BasicSettings.xaml
    /// </summary>
    public partial class BasicSettings : UserControl
    {
        public BasicSettings()
        {
            this.DataContextChanged += BasicSettings_DataContextChanged;
            InitializeComponent();
        }

        private void BasicSettings_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null)
            {
                if (DataContext is DominoProviderVM d)
                {
                    d.dispatcher = Dispatcher;
                }
            }
        }
    }
}
