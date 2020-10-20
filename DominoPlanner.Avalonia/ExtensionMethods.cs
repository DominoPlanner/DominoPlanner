using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.UI
{
    static class ExtensionMethods
    {
        public static string[] ShowDialog(this OpenFileDialog ofd)
        {
            return Task.Run(async () => await ofd.ShowAsync(MainWindowViewModel.GetWindow())).Result;
        }
        public static string ShowDialog(this SaveFileDialog ofd)
        {
            return Task.Run(async () => await ofd.ShowAsync(MainWindowViewModel.GetWindow())).Result;
        }
    }
}
