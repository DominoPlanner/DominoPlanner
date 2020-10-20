

using MsgBox;
using System.Threading.Tasks;

namespace DominoPlanner.UI
{
    internal static class Errorhandler
    {
        internal static void RaiseMessage(string message, string header, MessageType messageType)
        {
            /*MessageBoxImage image = System.Windows.MessageBoxImage.Information;
            switch (messageType)
            {
                case MessageType.Info:
                    image = System.Windows.MessageBoxImage.Information;
                    break;
                case MessageType.Error:
                    image = System.Windows.MessageBoxImage.Error;
                    break;
                case MessageType.Warning:
                    image = System.Windows.MessageBoxImage.Warning;
                    break;
                default:
                    break;
            }*/
            Task.Run(async() => await MessageBox.Show(message, header, MessageBox.MessageBoxButtons.Ok));
        }

        internal enum MessageType { Info, Error, Warning }
    }
}
