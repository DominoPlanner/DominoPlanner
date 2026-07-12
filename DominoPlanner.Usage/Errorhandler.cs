using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Linq;
using System.Threading.Tasks;

using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

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
			var box = MessageBoxManager.GetMessageBoxStandard(header, message, ButtonEnum.Ok, image);
			return await box.ShowAsPopupAsync(owner);
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
