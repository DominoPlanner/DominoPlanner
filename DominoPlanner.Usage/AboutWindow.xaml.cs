using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Controls.ApplicationLifetimes;

namespace DominoPlanner.Usage
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            var viewModel = new AboutWindowViewModel(this);
            DataContext = viewModel;
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
        private readonly Window _window;
        
        public string Version { get; private set; } = "";
        public string Authors { get; private set; } = "";

        public string IssueTracker { get; private set; } = "https://github.com/jhofinger/DominoPlanner/issues";

        public string DonateLink { get; private set; } = "https://paypal.me/DominoPlannerSupport";
        
        public AboutWindowViewModel(Window window = null)
        {
            _window = window;
            
            Version = ReadResource(new Uri("avares://DominoPlanner.Usage/version.txt"));
            Authors = ReadResource(new Uri("avares://DominoPlanner.Usage/AUTHORS.txt"));
        }
        public string ReadResource(Uri uri)
        {
            var assets = AssetLoader.Open(uri);
            if (assets != null)
            {
                using (var streamreader = new StreamReader(assets))
                {
                    return streamreader.ReadToEnd();
                }
            }
            return "";
        }
        public async void CopyVersionToClipboard()
        {
            try
            {
                if (_window != null)
                {
                    var topLevel = TopLevel.GetTopLevel(_window);
                    if (topLevel?.Clipboard != null)
                    {
                        await topLevel.Clipboard.SetTextAsync("DominoPlanner: " + Version);
                    }
                }
            }
            catch { }
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
