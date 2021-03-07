using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DominoPlanner.Usage.UserControls.ViewModel;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DominoPlanner.Usage
{
    static class DialogExtensions
    {
        public static string GetCurrentProjectPath()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                MainWindow parentWindow = desktopLifetime.Windows.OfType<MainWindow>().FirstOrDefault();
                if(parentWindow.DataContext is MainWindowViewModel m)
                {
                    return m.SelectedProject.GetInitialDirectory();
                }
            }
            return "";
        }
        public static string GetInitialDirectory(this DominoWrapperNodeVM node)
        {
            var project = node;
            if (project is DocumentNodeVM vm)
            {
                project = vm.Parent;
            }
            if (project is AssemblyNodeVM vm2)
            {
                var path = vm2.AbsolutePath;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    path = Path.GetDirectoryName(path);
                }
                return path;
            }
            return "";
        }
        public async static Task<string[]> ShowAsyncWithParent<T>(this OpenFileDialog ofd) where T: Avalonia.Controls.Window
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
                if (parentWindow != null)
                {
                    return await ofd.ShowAsync(parentWindow);
                }
            }
            return null;
        }
        private async static Task<string> ShowAsyncWithParentInternal<T>(this Avalonia.Controls.FileSystemDialog fileDialog) where T: Avalonia.Controls.Window
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
                if (parentWindow != null)
                {
                    if (fileDialog is OpenFolderDialog ofd)
                        return await ofd.ShowAsync(parentWindow);
                    else if (fileDialog is SaveFileDialog sfd)
                        return await sfd.ShowAsync(parentWindow);
                }
            }
            return null;
        }
        public async static Task<string> ShowAsyncWithParent<T>(this OpenFolderDialog fileDialog) where T: Avalonia.Controls.Window
        {
            return await fileDialog.ShowAsyncWithParentInternal<T>();
        }
        public async static Task<string> ShowAsyncWithParent<T>(this SaveFileDialog fileDialog) where T: Avalonia.Controls.Window
        {
            return await fileDialog.ShowAsyncWithParentInternal<T>();
        }
        public async static Task<R> GetDialogResultWithParent<T, R>(this Window window) where T: Avalonia.Controls.Window
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
                if (parentWindow != null)
                {
                    return await window.ShowDialog<R>(parentWindow);
                }
            }
            return default;
        }
        public async static Task ShowDialogWithParent<T>(this Window window) where T: Avalonia.Controls.Window
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
                if (parentWindow != null)
                {
                    await window.ShowDialog(parentWindow);
                }
            }
        }
        public static void ShowWithParent<T>(this Window window) where T: Avalonia.Controls.Window
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
                if (parentWindow != null)
                {
                    window.Show(parentWindow);
                }
            }
        }
         public async static Task<ButtonResult> ShowDialogWithParent<T>(this IMsBoxWindow<ButtonResult> window) where T: Avalonia.Controls.Window
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
                if (parentWindow != null)
                {
                    return await window.ShowDialog(parentWindow);
                }
            }
            return ButtonResult.Abort;
        }
    }
}
