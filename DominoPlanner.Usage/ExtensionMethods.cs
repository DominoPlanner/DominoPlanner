﻿using Avalonia.Controls;
using Avalonia.Media;
using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Usage
{
    static class ExtensionMethods
    {
    // TODO: These methods must be removed
        public static string[] ShowDialog(this OpenFileDialog ofd)
        {
            return ofd.ShowAsync(MainWindowViewModel.GetWindow()).GetAwaiter().GetResult();
        }
        /*public async static Task<string[]> ShowDialog(this OpenFileDialog ofd)
        {
            var result = await ofd.ShowAsync(MainWindowViewModel.GetWindow());
            return result;
        }*/
        public static string ShowDialog(this SaveFileDialog ofd)
        {
            return Task.Run(async () => await ofd.ShowAsync(MainWindowViewModel.GetWindow())).Result;
        }
        public static string ShowDialog(this OpenFolderDialog ofd)
        {
            return Task.Run(async () => await ofd.ShowAsync(MainWindowViewModel.GetWindow())).Result;
        }
    }
}
