using DominoPlanner.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DominoPlanner.Usage
{
    /// <summary>
    /// Interaktionslogik für ColorControl.xaml
    /// </summary>
    public partial class ColorControl : UserControl
    {
        public ObservableCollection<ColorListEntry> Colors
        {
            get { return (ObservableCollection<ColorListEntry>)GetValue(ColorsProperty); }
            set { SetValue(ColorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Colors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorsProperty =
            DependencyProperty.Register("Colors", typeof(ObservableCollection<ColorListEntry>), typeof(ColorControl), new PropertyMetadata(new ObservableCollection<ColorListEntry>()));
        
        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(ColorControl));



        public ColumnConfig ColumnConfig
        {
            get { return (ColumnConfig)GetValue(ColumnConfigProperty); }
            set { SetValue(ColumnConfigProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnConfig.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnConfigProperty =
            DependencyProperty.Register("ColumnConfig", typeof(ColumnConfig), typeof(ColorControl), new PropertyMetadata(new ColumnConfig()));



        public ColorListEntry SelectedColor
        {
            get { return (ColorListEntry)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(ColorListEntry), typeof(ColorControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ColorControl), new PropertyMetadata(0));



        public ColorControl()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
            ColumnConfig = new ColumnConfig();
            ColumnConfig.Columns = new ObservableCollection<Column>();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is ColorListEntry)
            {
                ClickCommand.Execute(null);
            }
        }
    }
    public class ColumnConfig
    {
        public IEnumerable<Column> Columns { get; set; }
    }
    public class Column
    {
        public string Header { get; set; }
        public string DataField { get; set; }
        public string HighlightDataField { get; set; }
    }
    public class ConfigToDynamicGridViewConverter : IValueConverter
    {
        private GridView currentGridView;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (currentGridView != null) BindingOperations.ClearAllBindings(currentGridView);
            //currentGridView?.Columns.Clear();
            var config = value as ColumnConfig;
            var grdiView = new GridView();
            if (config != null && config.Columns != null)
            {
                
                foreach (var column in config.Columns)
                {
                    int minWidth = 0;
                    DataTemplate columnLayout = new DataTemplate();
                    if (column.DataField != "DominoColor.mediaColor")
                    {
                        minWidth = 50;
                        FrameworkElementFactory labelFactory = new FrameworkElementFactory(typeof(TextBlock));
                        labelFactory.SetBinding(TextBlock.TextProperty, new Binding(column.DataField));
                        columnLayout.VisualTree = labelFactory;
                        if (!string.IsNullOrEmpty(column.HighlightDataField))
                        {
                            MultiBinding colorBinding = new MultiBinding();
                            colorBinding.Bindings.Add(new Binding(column.DataField));
                            colorBinding.Bindings.Add(new Binding(column.HighlightDataField));
                            colorBinding.Converter = new AmountToColorConverter();
                            labelFactory.SetBinding(TextBlock.ForegroundProperty, colorBinding);
                        }
                        var binding = new Binding(column.DataField);
                    }
                    else
                    {
                        FrameworkElementFactory rectFactory = new FrameworkElementFactory(typeof(Rectangle));
                        Binding binding = new Binding(column.DataField);
                        binding.Converter = new ColorToBrushConverter();
                        rectFactory.SetBinding(Rectangle.FillProperty, binding);
                        rectFactory.SetValue(Rectangle.WidthProperty, 16.0);
                        rectFactory.SetValue(Rectangle.HeightProperty, 24.0);
                        columnLayout.VisualTree = rectFactory;
                    }
                    var GridViewColumnHeader = new GridViewColumnHeader();
                    GridViewColumnHeader.MinWidth = minWidth;
                    GridViewColumnHeader.Content = column.Header;
                    grdiView.Columns.Add(new GridViewColumn { Header = GridViewColumnHeader, CellTemplate = columnLayout });
                }
            }
            currentGridView = grdiView;
            return grdiView;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
