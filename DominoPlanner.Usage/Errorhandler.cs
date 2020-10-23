

using MessageBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace DominoPlanner.Usage
{
    internal static class Errorhandler
    {
        internal static async void RaiseMessage(string message, string header, MessageType messageType)
        {
            Icon image;
            switch (messageType)
            {
                case MessageType.Error:
                    image = Icon.Error;
                    break;
                case MessageType.Warning:
                    image = Icon.Warning;
                    break;
                case MessageType.Info:
                default:
                    image = Icon.Info;
                    break;
            }
            var box = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(header, message, ButtonEnum.Ok, image);
            await box.ShowDialog(MainWindowViewModel.GetWindow());
        }

        internal enum MessageType { Info, Error, Warning }
    }
}
