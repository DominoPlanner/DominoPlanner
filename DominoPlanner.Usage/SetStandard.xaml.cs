using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.Usage
{
    public class SetStandardV : Window
    {
        public SetStandardV()
        {
            this.InitializeComponent();
            DataContext = new SetStandardVM() { window = this };
            
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
