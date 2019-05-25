using System;
using System.Text;
using System.Windows;

namespace DominoPlanner.Usage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NamedPipeManager PipeManager;
        

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            PipeManager = new NamedPipeManager("DominoPlanner");
            PipeManager.StartServer();
            PipeManager.ReceiveString += HandleNamedPipe_OpenRequest;
            var args = Environment.GetCommandLineArgs();
            string filesToOpen = "";
            if (args != null && args.Length > 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 1; i < args.Length; i++)
                {
                    sb.AppendLine(args[i]);
                }
                filesToOpen = sb.ToString();
            }
            PipeManager.Write(filesToOpen);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(DataContext is MainWindowViewModel mwvm)
            {
                if (!mwvm.CloseAllTabs())
                {
                    e.Cancel = true;
                }
            }
            PipeManager.StopServer();
        }
        public void HandleNamedPipe_OpenRequest(string filesToOpen)
        {
            Dispatcher.Invoke(() =>
            {
                ((MainWindowViewModel)DataContext).OpenFile(filesToOpen);

                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;

                this.Topmost = true;
                this.Activate();
                Dispatcher.BeginInvoke(new Action(() => { this.Topmost = false; }));
            });
        }
    }
}
