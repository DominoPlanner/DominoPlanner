using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MessageBox.Avalonia.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace DominoPlanner.Usage
{
    internal static class Errorhandler
    {
        internal static async Task<ButtonResult> RaiseMessage(string message, string header, MessageType messageType, Window owner)
        {
            var image = messageType switch
            {
                MessageType.Error => Icon.Error,
                MessageType.Warning => Icon.Warning,
                _ => Icon.Info,
            };
            var box = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(header, message, ButtonEnum.Ok, image);
            return await box.ShowDialog(owner);
        }
        internal static async Task<ButtonResult> RaiseMessageWithParent<T>(string message, string header, MessageType messageType) where T : Window
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
                if (parentWindow != null)
                {
                    return await RaiseMessage(message, header, messageType, parentWindow);
                }
            }
            return ButtonResult.Cancel;
        }
        // Raise Message with MainWindow as parent
        internal static async Task<ButtonResult> RaiseMessage(string message, string header, MessageType messageType) 
        {
            return await RaiseMessageWithParent<MainWindow>(message, header, messageType);
        }

        internal enum MessageType { Info, Error, Warning }
    }
}
