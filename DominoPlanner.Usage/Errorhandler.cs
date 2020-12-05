using Avalonia.Controls;
using MessageBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace DominoPlanner.Usage
{
    internal static class Errorhandler
    {
        internal static async Task RaiseMessage(string message, string header, MessageType messageType, Window owner = null)
        {
            var image = messageType switch
            {
                MessageType.Error => Icon.Error,
                MessageType.Warning => Icon.Warning,
                _ => Icon.Info,
            };
            var box = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(header, message, ButtonEnum.Ok, image);
            if (MainWindowViewModel.GetWindow() != null)
                await box.ShowDialog(owner ?? MainWindowViewModel.GetWindow());
        }

        internal enum MessageType { Info, Error, Warning }
    }
}
