using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.Usage
{
    public class LiveBuildHelperV : Window
    {
        public LiveBuildHelperV()
        {
            this.InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
            this.KeyDown += LiveBuildHelperV_KeyDown;
        }

        private void LiveBuildHelperV_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Space)
            {
                ((LiveBuildHelperVM)DataContext).PressedKey(e.Key);
                var cc = this.Get<ContentControl>("CC");
                cc.Focus();
                //Keyboard.Focus(CC);
                e.Handled = true;
            }
        }

        public void ContentControl_MouseDown(object sender, PointerPressedEventArgs e)
        {
            var cc = this.Get<ContentControl>("CC");
            cc.Focus();
            //Keyboard.Focus(CC);
        }

        private void IntegerUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Space)
            {
                ((LiveBuildHelperVM)DataContext).PressedKey(e.Key);
                var cc = this.Get<ContentControl>("CC");
                cc.Focus();
                //Keyboard.Focus(CC);
                e.Handled = true;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
