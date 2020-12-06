using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DominoPlanner.Usage
{
    public class ImageButton : Button
    {
        public IImage Image
        {
            get { return GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public static readonly StyledProperty<IImage> ImageProperty = AvaloniaProperty.Register<ImageButton, IImage>(nameof(Image));

        public ImageButton() { }
    }
}
