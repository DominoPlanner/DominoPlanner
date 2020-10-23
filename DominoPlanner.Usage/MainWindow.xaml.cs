using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace DominoPlanner.Usage
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            /*PipeManager = new NamedPipeManager("DominoPlanner");
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
            PipeManager.Write(filesToOpen);*/
#if DEBUG
            //this.AttachDevTools();
#endif
        }
        // ugly hacky workaround for the fact that events don't wait if they are canceled asynchronously
        private bool should_really_close = false;
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!should_really_close)
            {
                e.Cancel = true;
            }
            if (DataContext is MainWindowViewModel mwvm)
            {
                if (await mwvm.CloseAllTabs())
                {
                    if (!should_really_close)
                    {
                        should_really_close = true;
                        this.Close();
                    }
                }
            }
            //PipeManager.StopServer();
        }
        /*public void HandleNamedPipe_OpenRequest(string filesToOpen)
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
        }*/

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public static string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "";
                return result;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
            return "";
        }

        public static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }
    }
}
