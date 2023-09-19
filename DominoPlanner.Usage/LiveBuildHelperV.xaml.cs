using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;

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

        private void ListBox_GotFocus(object sender, GotFocusEventArgs e)
        {
            try
            {
                var mainGrid = this.Get<Grid>("MG");
                mainGrid?.Focus();
            }catch(Exception ex) { }
        }

        private void LiveBuildHelperV_LayoutUpdated(object sender, System.EventArgs e)
        {
            if(sender is TextBlock textBox)
            {
                if(textBox.DesiredSize.Width < Math.Floor(textBox.TextLayout.Size.Width))
                {
                    textBox.RenderTransformOrigin = new RelativePoint(new Point(0, 0), RelativeUnit.Relative);

                    RotateTransform rotate = new RotateTransform(90);

                    TranslateTransform translate = new TranslateTransform
                    {
                        Y = 0,
                        X = (textBox.TextLayout.Size.Width / 2) + (textBox.TextLayout.Size.Height / 2)
                    };

                    TransformGroup transformGroup = new TransformGroup();
                    transformGroup.Children.Add(rotate);
                    transformGroup.Children.Add(translate);

                    textBox.RenderTransform = transformGroup;

                    textBox.Width = textBox.TextLayout.Size.Width + 5;
                }
            }
        }

        private void LiveBuildHelperV_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Space || e.Key == Key.P)
            {
                ((LiveBuildHelperVM)DataContext).PressedKey(e.Key);
                var mainGrid = this.Get<Grid>("MG");
                mainGrid.Focus();
                e.Handled = true;
            }
        }

        public void ContentControl_MouseDown(object sender, PointerPressedEventArgs e)
        {
            var mainGrid = this.Get<Grid>("MG");
            mainGrid.Focus();
        }

        private void IntegerUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Space)
            {
                ((LiveBuildHelperVM)DataContext).PressedKey(e.Key);
                var mainGrid = this.Get<Grid>("MG");
                mainGrid.Focus();
                e.Handled = true;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
