using DominoPlanner.Document_Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DominoPlanner
{
    /// <summary>
    /// Interaktionslogik für EditColors.xaml
    /// </summary>
    public partial class EditColors : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public ColorArrayDocument colors {get; set;}
        public EditColors()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lvColors.ItemsSource = colors.cols;
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        

         private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            ListView lvColors = (ListView)sender;
            List<DominoColor> colors = (lvColors.DataContext as ColorArrayDocument).cols;

            int index = lvColors.SelectedIndex;
            if (e.Key == Key.CapsLock)
            {
                if (index > 0)
                {
                    DominoColor temp = colors[index - 1];
                    colors[index - 1] = colors[index];
                    colors[index] = temp;
                    lvColors.Items.Refresh();
                    lvColors.SelectedIndex = --index;
                    lvColors.ScrollIntoView(lvColors.Items.GetItemAt(index));
                }
            }
            if (e.Key == Key.LeftShift)
            {
                if (index < colors.Count - 1)
                {
                    DominoColor temp = colors[index + 1];
                    colors[index + 1] = colors[index];
                    colors[index] = temp;
                    lvColors.Items.Refresh();
                    lvColors.SelectedIndex = ++index;
                    lvColors.ScrollIntoView(lvColors.Items.GetItemAt(index));
                }
            }
        }

        private void Color_Delete(object sender, RoutedEventArgs e)
        {
            DominoColor color = (DominoColor)((Button)sender).DataContext;
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is ListView))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            List<DominoColor> colors = ((ListView)parent).ItemsSource as List<DominoColor>;
            colors.Remove(color);
            (parent as ListView).Items.Refresh();
        }

        private void Color_Edit(object sender, RoutedEventArgs e)
        {
            DominoColor color = (DominoColor)((Button)sender).DataContext;
            ColorControl c = new ColorControl();
            c.ColorPicker.SelectedColor = color.rgb;
            c.count = color.count;
            c.name = color.name;
            c.ShowDialog();
            if (c.DialogResult == true)
            {
                color.rgb = c.ColorPicker.SelectedColor;
                color.name = c.name;
                color.count = c.count;
            }
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is ListView))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            List<DominoColor> colors = ((ListView)parent).ItemsSource as List<DominoColor>;
            (parent as ListView).Items.Refresh();
        }

        private void Color_MoveUp(object sender, RoutedEventArgs e)
        {
            DominoColor color = (DominoColor)((Button)sender).DataContext;
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is ListView))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            List<DominoColor> colors = ((ListView)parent).ItemsSource as List<DominoColor>;
            int index = colors.IndexOf(color);
            if (index > 0)
            {
                DominoColor temp = colors[index - 1];
                colors[index - 1] = color;
                colors[index] = temp;
                (parent as ListView).Items.Refresh();
                (parent as ListView).SelectedIndex = --index;
                (parent as ListView).ScrollIntoView((parent as ListView).Items.GetItemAt(index));
            }
        }

        private void Color_MoveDown(object sender, RoutedEventArgs e)
        {
            DominoColor color = (DominoColor)((Button)sender).DataContext;
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is ListView))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            List<DominoColor> colors = ((ListView)parent).ItemsSource as List<DominoColor>;
            int index = colors.IndexOf(color);
            if (index < colors.Count - 1)
            {
                DominoColor temp = colors[index + 1];
                colors[index + 1] = color;
                colors[index] = temp;
                (parent as ListView).Items.Refresh();
                (parent as ListView).SelectedIndex = ++index;
                (parent as ListView).ScrollIntoView((parent as ListView).Items.GetItemAt(index));
            }
        }

        private void AddColor(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            parent = VisualTreeHelper.GetChild(parent, 0);
            List<DominoColor> colors = ((ListView)parent).ItemsSource as List<DominoColor>;
            DominoColor color = new DominoColor("New Color", Color.FromRgb(0, 0, 0), 1000);
            ColorControl c = new ColorControl();
            c.ColorPicker.SelectedColor = color.rgb;
            c.count = color.count;
            c.name = color.name;
            c.ShowDialog();
            if (c.DialogResult == true)
            {
                color.rgb = c.ColorPicker.SelectedColor;
                color.name = c.name;
                color.count = c.count;
            }
            colors.Add(color);
            (parent as ListView).Items.Refresh();
            (parent as ListView).ScrollIntoView((parent as ListView).Items.GetItemAt(colors.Count - 1));
        }
        private void SaveColorDocumentGlobal(object sender, RoutedEventArgs e)
        {
            
            colors.Save(colors.path);
            DialogResult = true;
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            colors.Save(colors.path);
            DialogResult = true;
        }

    }
   
   
}
