using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Serilog.Filters;

namespace DominoPlanner.Usage
{
    class Program
    {
        public static Mutex mutex;
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {

            var s = Environment.GetCommandLineArgs();
            int index = Array.FindIndex(s, a => a.Equals("register"));
            if (index != -1)
            {
                //ISharpShellServer sharpShell = new SharpShellServer()
                //SharpShell.ServerRegistration.ServerRegistrationManager.InstallServer(typeof(DominoPlanner.PreviewHandler.IDominoProviderPreviewHandler),
                //    SharpShell.ServerRegistration.RegistrationType.OS64Bit, false);
                Environment.Exit(0);
            }
            SingleInstanceCheck();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace(Avalonia.Logging.LogEventLevel.Information, "Binding");

        public static void SingleInstanceCheck()
        {
            mutex = new Mutex(true, @"DominoPlanner", out bool isOnlyInstance);
            if (!isOnlyInstance)
            {
                string filesToOpen = "";
                var args = Environment.GetCommandLineArgs();
                if (args != null && args.Length > 1)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 1; i < args.Length; i++)
                    {
                        sb.AppendLine(args[i]);
                    }
                    filesToOpen = sb.ToString();
                }
                var manager = new NamedPipeManager("DominoPlanner");
                manager.Write(filesToOpen);

                Environment.Exit(0);
            }
        }


    }
    public class NamedPipeManager
    {
        public string NamedPipeName = "MarkdownMonster";
        public event Action<string> ReceiveString;

        private const string EXIT_STRING = "__EXIT__";
        private bool _isRunning = false;
        private Thread Thread;

        public NamedPipeManager(string name)
        {
            NamedPipeName = name;
        }

        /// <summary>
        /// Starts a new Pipe server on a new thread
        /// </summary>
        public void StartServer()
        {
            Thread = new Thread((pipeName) =>
            {
                _isRunning = true;

                while (true)
                {
                    string text;
                    using (var server = new NamedPipeServerStream(pipeName as string))
                    {
                        try
                        {
                            server.WaitForConnection();
                            using (StreamReader reader = new StreamReader(server))
                            {
                                text = reader.ReadToEnd();
                            }
                            if (text == EXIT_STRING)
                                break;

                            OnReceiveString(text);

                            if (_isRunning == false)
                                break;
                        }
                        catch (IOException ex)
                        {

                        }


                    }


                }
            });
            Thread.Start(NamedPipeName);
        }

        /// <summary>
        /// Called when data is received.
        /// </summary>
        /// <param name="text"></param>
        protected virtual void OnReceiveString(string text) => ReceiveString?.Invoke(text);


        /// <summary>
        /// Shuts down the pipe server
        /// </summary>
        public void StopServer()
        {
            _isRunning = false;
            Write(EXIT_STRING);
            Thread.Sleep(30); // give time for thread shutdown
        }

        /// <summary>
        /// Write a client message to the pipe
        /// </summary>
        /// <param name="text"></param>
        /// <param name="connectTimeout"></param>
        public bool Write(string text, int connectTimeout = 300)
        {
            using (var client = new NamedPipeClientStream(NamedPipeName))
            {
                try
                {
                    client.Connect(connectTimeout);
                }
                catch
                {
                    return false;
                }

                if (!client.IsConnected)
                    return false;

                using (StreamWriter writer = new StreamWriter(client))
                {
                    writer.AutoFlush = true;
                    writer.Write(text);
                    #if win
                    client.WaitForPipeDrain();
                    #endif
                }
            }
            return true;
        }
    }
}
