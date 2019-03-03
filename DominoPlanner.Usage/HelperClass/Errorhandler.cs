using Xceed.Wpf.Toolkit;

namespace DominoPlanner.Usage.HelperClass
{
    internal static class Errorhandler
    {
        internal static void RaiseMessage(string message, string header, MessageType messageType)
        {
            System.Windows.MessageBoxImage image = System.Windows.MessageBoxImage.Information;
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
            }
            MessageBox.Show(message, header, System.Windows.MessageBoxButton.OK, image);
        }

        internal enum MessageType { Info, Error, Warning }
    }
}
