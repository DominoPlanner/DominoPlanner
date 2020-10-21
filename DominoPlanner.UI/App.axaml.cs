using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Logging;
using System.Diagnostics;
using System.Collections.Generic;

namespace DominoPlanner.UI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            Logger.Sink = new CustomLogSink(LogEventLevel.Information);
            Debug.WriteLine(Logger.IsEnabled(LogEventLevel.Information, "Binding"));
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
