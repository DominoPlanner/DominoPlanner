using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.Usage
{
    public class PropertiesWindow : Window
    {
        public PropertiesWindow()
        {
            this.InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif

        }
        public PropertiesWindow(object context) : this()
        {
            this.DataContext = new PropertiesVM(context);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
