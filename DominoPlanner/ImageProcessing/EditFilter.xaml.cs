using ImageProcessor;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using System.ComponentModel;

namespace DominoPlanner
{
    /// <summary>
    /// Interaction logic for EditFilter.xaml
    /// </summary>
    public partial class EditFilter : Window, INotifyPropertyChanged
    {
        public Filter filter { get; set; }
        public BitmapImage source { get; set; }
        public BitmapImage result {
            get; set; }
        public EditFilter()
        {
            InitializeComponent();
            
        }
        protected void OnPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler tempHandler = PropertyChanged;
            if (tempHandler != null)
                tempHandler(this, new PropertyChangedEventArgs(propertyName));
           
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void Redraw()
        {
            using (MemoryStream inputstream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(inputstream);

                using (MemoryStream outputstream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(true))
                    {
                        inputstream.Position = 0;
                        imageFactory.Load(inputstream);
                        filter.Apply(imageFactory);
                        imageFactory.Save(outputstream);
                    }
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = outputstream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    result = bitmapImage;
                }
            }
            OnPropertyChanged("result");
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ContentController.Content = filter;
            Redraw();
            this.Title = filter.name + " Filter Settings";
        }

        private void NumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Redraw();
            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = true;
        }

        private void SelectColorButton(object sender, RoutedEventArgs e)
        {
            Filter f = (sender as Button).DataContext as Filter;
            ColorControl c = new ColorControl();
            c.Show_Only_Color = true;
            if (f is BackgroundColorFilter)
            {
                BackgroundColorFilter b = f as BackgroundColorFilter;
                c.ColorPicker.SelectedColor = FieldPlanDocument.SDtoSM(b.color);
                if (c.ShowDialog() == true)
                {
                    b.color = FieldBlockViewer.SMtoSD(c.ColorPicker.SelectedColor);
                }
            }
            if (f is ReplaceFilter)
            {
                ReplaceFilter b = f as ReplaceFilter;
                if ((sender as Button).ToolTip.ToString() == "First Color")
                {
                    c.ColorPicker.SelectedColor = FieldPlanDocument.SDtoSM(b.source);
                    if (c.ShowDialog() == true)
                    {
                        b.source = FieldBlockViewer.SMtoSD(c.ColorPicker.SelectedColor);
                    }
                }
                else
                {
                    c.ColorPicker.SelectedColor = FieldPlanDocument.SDtoSM(b.target);
                    if (c.ShowDialog() == true)
                    {
                        b.target = FieldBlockViewer.SMtoSD(c.ColorPicker.SelectedColor);
                    }
                }
            }
            if (f is TintFilter)
            {
                TintFilter b = f as TintFilter;
                c.ColorPicker.SelectedColor = FieldPlanDocument.SDtoSM(b.color);
                if (c.ShowDialog() == true)
                {
                    b.color = FieldBlockViewer.SMtoSD(c.ColorPicker.SelectedColor);
                }
            }
            if (f is VignetteFilter)
            {
                VignetteFilter b = f as VignetteFilter;
                c.ColorPicker.SelectedColor = FieldPlanDocument.SDtoSM(b.color);
                if (c.ShowDialog() == true)
                {
                    b.color = FieldBlockViewer.SMtoSD(c.ColorPicker.SelectedColor);
                }
            }
            Redraw();
            OnPropertyChanged(null);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Redraw();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Redraw();
        }

        private void XNumChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Redraw();
        }
    }
    public class ContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AlphaTemplate { get; set; }
        public DataTemplate BackgroundColorTemplate { get; set; }
        public DataTemplate BrightnessTemplate { get; set; }
        public DataTemplate ContrastTemplate { get; set; }
        public DataTemplate CropTemplate { get; set; }        
        public DataTemplate EdgeDetectionTemplate { get; set; }
        public DataTemplate EntropyCropTemplate { get; set; }
        public DataTemplate MatrixTemplate { get; set; }
        public DataTemplate FlipTemplate { get; set; }
        public DataTemplate GaussianBlurTemplate { get; set; }
        public DataTemplate GaussianSharpenTemplate { get; set; }
        public DataTemplate HueTemplate { get; set; }
        public DataTemplate ReplaceTemplate { get; set; }
        public DataTemplate RotateTemplate { get; set; }
        public DataTemplate SaturationTemplate { get; set; }
        public DataTemplate TintTemplate { get; set; }
        public DataTemplate VignetteTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Filter value = item as Filter;

            if (value != null)
            {
                (container as ContentPresenter).Content = value;

                if (value is AlphaFilter)                   return AlphaTemplate;
                else if (value is BackgroundColorFilter)    return BackgroundColorTemplate;
                else if (value is BrightnessFilter)         return BrightnessTemplate;
                else if (value is ContrastFilter)           return ContrastTemplate;
                else if (value is CropFilter)               return CropTemplate;
                else if (value is EdgeDetectionFilter)      return EdgeDetectionTemplate;
                else if (value is EntropyCropFilter)        return EntropyCropTemplate;
                else if (value is MatrixFilter)             return MatrixTemplate;
                else if (value is FlipFilter)               return FlipTemplate;
                else if (value is GaussianBlurFilter)       return GaussianBlurTemplate;
                else if (value is GaussianSharpenFilter)    return GaussianSharpenTemplate;
                else if (value is HueFilter)                return HueTemplate;
                else if (value is ReplaceFilter)            return ReplaceTemplate;
                else if (value is RotateFilter)             return RotateTemplate;
                else if (value is SaturationFilter)         return SaturationTemplate;
                else if (value is TintFilter)               return TintTemplate;
                else if (value is VignetteFilter)           return VignetteTemplate;

                return base.SelectTemplate(item, container);
            }
            else
                return base.SelectTemplate(item, container);
        }
    }
}
