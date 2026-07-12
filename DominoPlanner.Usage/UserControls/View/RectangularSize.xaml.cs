using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.Usage.UserControls.View
{
    public partial class RectangularSize : UserControl
    {
        public RectangularSize()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
