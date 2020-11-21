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
using Avalonia.Controls;
using Avalonia.Data;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Markup.Xaml;
using Avalonia.Data.Converters;
using System.Reflection.Emit;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Templates;
using ThemeEditor.Controls.ColorPicker;

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
        public static readonly StyledProperty<ObservableCollection<ColorListEntry>> ColorsProperty =
            AvaloniaProperty.Register<ColorControl, ObservableCollection<ColorListEntry>>("Colors", new ObservableCollection<ColorListEntry>());

        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly AvaloniaProperty ClickCommandProperty =
            AvaloniaProperty.Register<ColorControl, ICommand>("ClickCommand");



        public AvaloniaList<Column> ColumnConfig
        {
            get { return (AvaloniaList<Column>)GetValue(ColumnConfigProperty); }
            set { SetValue(ColumnConfigProperty, value); }
        }
        protected int HeaderRow = 0;
        internal virtual void UpdateLayout()
        {
            

            var header = this.Find<Grid>("HeaderGrid");
            header.ColumnDefinitions.Clear();

            FillHeader(header);

            var itemscontrol = this.Find<ItemsControl>("ItemsControl");
            // for content, we have to define it inside a lambda function
            var template = new FuncDataTemplate<ColorListEntry>((x, _) =>
            {
                Grid g = new Grid();
                FillTemplate(g);
                return g;
            }) ;
            itemscontrol.ItemTemplate = template;

        }
        public void FillHeader(Grid header)
        {
            int counter = 0;
            foreach (var column in this.ColumnConfig)
            {
                // create columns
                var cdef = new ColumnDefinition() { Width = GridLength.Auto };
                header.ColumnDefinitions.Add(cdef);
                cdef.SharedSizeGroup = "COL_" + counter;
                // set header
                var tb = new ContentControl() { Content = column.Header };
                tb.Classes.Add("Header");
                Grid.SetColumn(tb, counter);
                Grid.SetRow(tb, 0);
                Grid.SetRowSpan(tb, HeaderRow+1);
                header.Children.Add(tb);
                if (column.CanResize)
                {
                    var splitter = new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };
                    Grid.SetColumn(splitter, counter);
                    Grid.SetRow(splitter, HeaderRow);
                    header.Children.Add(splitter);
                }
                counter++;

            }
        }
        public void FillTemplate(Grid g)
        {
            int counter = 0;
            foreach (var column in this.ColumnConfig)
            {
                var cdef2 = new ColumnDefinition() { Width = GridLength.Auto };
                g.ColumnDefinitions.Add(cdef2);
                cdef2.SharedSizeGroup = "COL_" + counter;
                {
                    var cc = new ContentControl()
                    {
                        [!ContentProperty] = new Binding(column.DataField, BindingMode.TwoWay)
                    };

                    cc.Classes.Add(column.Class);
                    Grid.SetColumn(cc, counter);
                    g.Children.Add(cc);
                }
                counter++;
            }
            g.DoubleTapped += (o, e) =>
            {
                ClickCommand?.Execute(o);
            };
        }

        // Using a DependencyProperty as the backing store for ColumnConfig.  This enables animation, styling, binding, etc...
        public static readonly AvaloniaProperty ColumnConfigProperty =
            AvaloniaProperty.Register<ColorControl, AvaloniaList<Column>>("ColumnConfig", new AvaloniaList<Column>());



        public ColorListEntry SelectedColor
        {
            get { return (ColorListEntry)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly AvaloniaProperty SelectedColorProperty =
            AvaloniaProperty.Register<ColorControl, ColorListEntry>("SelectedColor", defaultBindingMode: BindingMode.TwoWay);



        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly AvaloniaProperty SelectedIndexProperty =
            AvaloniaProperty.Register<ColorControl, int>("SelectedIndex", 0);



        public ColorControl()
        {
            InitializeComponent();
            this.Find<Grid>("LayoutRoot").DataContext = this;
            ColumnConfigProperty.Changed.AddClassHandler<ColorControl>((o, e) => UpdateLayout());
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        /*private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            {
                if (((FrameworkElement)e.OriginalSource).DataContext is ColorListEntry)
                {
                    ClickCommand.Execute(null);
                }
            }
        }*/
        public class Column
        {
            public string Header { get; set; }
            public string DataField { get; set; }
            public string HighlightDataField { get; set; }
            public string Class { get; set; } = "";
            public bool CanResize { get; set; } = false;
        }
        /*public class ConfigToDynamicGridConverter : IValueConverter
        {
            //private GridView currentGridView;
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                //if (currentGridView != null) BindingOperations.ClearAllBindings(currentGridView);
                //currentGridView?.Columns.Clear();
                var config = value as AvaloniaList<Column>;
                var grid = new Grid();
                if (config != null)
                {
                    foreach (var column in config)
                    {
                        // add title column;


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
        }*/
        
        public void OnScrollChanged(object control, ScrollChangedEventArgs args)
        {
            this.Find<ScrollViewer>("OuterScrollViewer").Offset = new Vector(this.Find<ScrollViewer>("InnerScrollViewer").Offset.X, 0);
        }
    }
}