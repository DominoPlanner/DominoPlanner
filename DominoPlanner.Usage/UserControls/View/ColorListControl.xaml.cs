using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.Usage.UserControls.View
{
    public class ColorListControl : UserControl
    {
        public ColorListControl()
        {
            this.InitializeComponent();
        }

        public bool ExportVisible
        {
            get { return GetValue(ExportVisibleProperty); }
            set { SetValue(ExportVisibleProperty, value); }
        }
        public static readonly StyledProperty<bool> ExportVisibleProperty = AvaloniaProperty.Register<ColorListControl, bool>(nameof(ExportVisible));


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
