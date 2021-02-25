using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DominoPlanner.Usage
{
    public class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            DataContext = new AboutWindowViewModel();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
    public class AboutWindowViewModel : ModelBase
    {
        public string Version { get; private set; } = "";
        public string Authors { get; private set; } = "";

        public string IssueTracker { get; private set; } = "https://github.com/jhofinger/DominoPlanner/issues";

        public string DonateLink { get; private set; } = "https://paypal.me/DominoPlannerSupport";
        public AboutWindowViewModel()
        {

            
            Version = ReadResource(new Uri("avares://DominoPlanner.Usage/version.txt"));
            Authors = ReadResource(new Uri("avares://DominoPlanner.Usage/AUTHORS.txt"));



        }
        public string ReadResource(Uri uri)
        {
            var resources = AvaloniaLocator.Current.GetService<IAssetLoader>();
            if (resources.Exists(uri))
            {
                using (var stream = resources.Open(uri))
                {
                    using (var streamreader = new StreamReader(stream))
                    {
                        return streamreader.ReadToEnd();
                    }
                }
                
            }
            return "";
        }
        public async void CopyVersionToClipboard()
        {
            await Application.Current.Clipboard.SetTextAsync("DominoPlanner: " + Version);
        }
        public void ReportBug()
        {
            OpenBrowser(IssueTracker);
        }
        public void OpenPayPal()
        {
            OpenBrowser(DonateLink);
        }
        private bool OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
                return true;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
                return true;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
                return true;
            }
            return false;
        }
    }
}
