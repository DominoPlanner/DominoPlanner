using System.Windows;

namespace DominoPlanner.Usage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(DataContext is MainWindowViewModel mwvm)
            {
                mwvm.CloseAllTabs();
            }
        }
    }
}
