﻿using SharpShell;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DominoPlanner.Usage
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Mutex mutex;
        public App()
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
            
        }
        public void SingleInstanceCheck()
        {
            bool isOnlyInstance = false;
            mutex = new Mutex(true, @"DominoPlanner", out isOnlyInstance);
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
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //var args = e.Args;
            //FileStream fs = new FileStream("args", FileMode.Append);
            //using (StreamWriter sw = new StreamWriter(fs))
            //{
            //    foreach (var arg in args)
            //    {
            //        sw.WriteLine(arg);
            //    }
            //    sw.WriteLine("--END--");
            //}
            //MainWindow mw = new MainWindow();
            //mw.Show();
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
                        server.WaitForConnection();

                        using (StreamReader reader = new StreamReader(server))
                        {
                            text = reader.ReadToEnd();
                        }
                    }

                    if (text == EXIT_STRING)
                        break;

                    OnReceiveString(text);

                    if (_isRunning == false)
                        break;
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
                    writer.Write(text);
                    writer.Flush();
                }
            }
            return true;
        }
    }
}
