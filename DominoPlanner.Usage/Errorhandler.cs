using MessageBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace DominoPlanner.Usage
{
    internal static class Errorhandler
    {
        internal static async void RaiseMessage(string message, string header, MessageType messageType)
        {
            var image = messageType switch
            {
                MessageType.Error => Icon.Error,
                MessageType.Warning => Icon.Warning,
                _ => Icon.Info,
            };
            var box = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(header, message, ButtonEnum.Ok, image);
            if (MainWindowViewModel.GetWindow() != null)
                await box.Show();
        }

        internal enum MessageType { Info, Error, Warning }
    }
}
