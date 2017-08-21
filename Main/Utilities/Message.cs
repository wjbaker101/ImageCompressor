using System.Windows.Media;

namespace ImageCompressor.Main.Utilities
{
    public enum MessageType
    {
        ERROR,
        WARNING,
        SUCCESS,
        DEFAULT
    }

    public class Message
    {
        public static Color GetColorFromType(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.ERROR:
                    return Color.FromArgb(255, 192, 23, 23);

                case MessageType.WARNING:
                    return Color.FromArgb(255, 255, 180, 0);

                case MessageType.SUCCESS:
                    return Color.FromArgb(255, 108, 192, 23);

                case MessageType.DEFAULT:
                default:
                    return Color.FromArgb(255, 0, 0, 0);
            }
        }
    }
}
