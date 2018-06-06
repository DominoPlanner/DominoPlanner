using DominoPlanner.Document_Classes;
using DominoPlanner.Util;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml.Linq;

namespace DominoPlanner
{
    /// <summary>
    /// Interaktionslogik für AddNewField.xaml
    /// </summary>
    public partial class AddNewObject : Window
    {
        public int type { get; set; }
        public Document doc { get; set; }
        public String project_path { get; set; }
        private String image_path;
        private int structure_index;
        private int field_index;
        public List<DominoColor> colors { get; set; }
        public ObservableCollection<String> templates { get; set; }
        public ObservableCollection<String> field_templates { get; set; }
        private int dominoes;
        public AddNewObject()
        {
            InitializeComponent();
            List_SelectionChanged(null, null);
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxMain.SelectedIndex == 0 && ContentC != null)
            {
                ContentC.ContentTemplate = Resources["FieldTemplate"] as DataTemplate;
                TextBoxFileName.Text = "NewField";
                FileEndingName.Content = ".fld";
                field_templates = new ObservableCollection<string>();
                // read field properties
                string[] s = Properties.Settings.Default.FieldTemplates.Split('\n');
                for (int x = 0; x < s.Length; x++)
                {
                    field_templates.Add(s[x].Trim('\r').Split(':')[0]);
                }
            }
            if (ListBoxMain.SelectedIndex == 1 && ContentC != null)
            {
                ContentC.ContentTemplate = Resources["StructureTemplate"] as DataTemplate;
                TextBoxFileName.Text = "NewStructure";
                FileEndingName.Content = ".sct";
                templates = new ObservableCollection<string>();

                // read settings
                string template = Properties.Settings.Default.StructureTemplates;
                XElement xml = XElement.Parse(template);
                List<ClusterStructure> templist = new List<ClusterStructure>();
                foreach (XElement definition in xml.Elements())
                {
                    ClusterStructure d = new ClusterStructure(definition);
                    templates.Add(d.name);
                }
                dominoes = 1000;
            }
        }

        private void ChangeImage(object sender, MouseButtonEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Open Image";
                ofd.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                if (ofd.ShowDialog() == true)
                {
                    image_path = ofd.FileName;
                    Label infolabel = VisualTreeHelper.GetChild(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(sender as DependencyObject)), 2) as Label;
                    BitmapImage b = new BitmapImage(new Uri(ofd.FileName));
                    (sender as Image).Source = b;
                    FileInfo fi = new FileInfo(ofd.FileName);
                    infolabel.Content = "File name: " + fi.Name + "\nFile size: " + Math.Round((double)fi.Length/1024d)   + 
                        "KB \nDimensions: " + b.PixelWidth + "x" + b.PixelHeight;
                    TextBoxFileName.Text = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
                }
            }
            catch { }
        }
        public static T GetVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
       

        private void FieldSizeChanded(object sender, TextChangedEventArgs e)
        {
            int o;
            if (!(int.TryParse((sender as TextBox).Text, out o)))
            {
                (sender as TextBox).Text = 10000 + "";
                dominoes = 10000;
            }
            else dominoes = o;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxMain.SelectedIndex == 0) // Field
            {
                if (image_path != null)
                {
                    // read properties from settings
                    string prop = Properties.Settings.Default.FieldTemplates.Split('\n')[field_index];
                    prop = prop.Trim('\r').Split(':')[1];
                    string[] distance_properties = prop.Split('*');
                    int a = int.Parse(distance_properties[0]);
                    int b = int.Parse(distance_properties[1]);
                    int c = int.Parse(distance_properties[2]);
                    int d = int.Parse(distance_properties[3]);

                    String filename = TextBoxFileName.Text;
                    BitmapImage bit = new BitmapImage(new Uri(image_path));
                    String ct = GetVisualChild<TextBox>(ContentC).Text;
                    int minimum = Int32.MaxValue;
                    int length = 1;
                    while (true)
                    {

                        length++;
                        
                        int height = (int)Math.Round(((double)length * (a + b) / (c + d) * bit.PixelHeight / bit.PixelWidth));
                        int difference = Math.Abs(length * height - int.Parse(ct));
                        if (difference <= minimum)
                        {
                            minimum = difference;
                        }
                        else
                        {
                            length--;
                            break;
                        }
                    }
                    
                    // Copy Image
                    String newpath = System.IO.Path.Combine(project_path, "Source Images", filename + System.IO.Path.GetExtension(image_path));
                    if (!(System.IO.File.Exists(System.IO.Path.Combine(project_path, filename + ".fld")) || File.Exists(newpath)))
                    {
                        File.Copy(image_path, newpath, false);
                        File.SetAttributes(newpath, FileAttributes.Normal);
                        doc = new FieldDocument(System.IO.Path.Combine(project_path, filename + ".fld"), newpath, colors, length, a, b, c, d);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("A field with this name already exists. Please choose another name.");
                    }
                }
                else
                {
                    MessageBox.Show("Please select an image.");
                }
            }
            if (ListBoxMain.SelectedIndex == 1) // Structure
            {
                if (image_path != null)
                {
                    String filename = TextBoxFileName.Text;
                    BitmapImage bit = new BitmapImage(new Uri(image_path));

                    string templates = Properties.Settings.Default.StructureTemplates;
                    XElement xml = XElement.Parse(templates);
                    ClusterStructure d = new ClusterStructure(xml.Elements("StructureDefinition").ToArray()[structure_index]);
                    float center_width = d.cells[1,1].width;
                    float center_height = d.cells[1, 1].height;
                    float add_width = d.cells[0, 0].width + d.cells[2, 2].width;
                    float add_height = d.cells[0, 0].height + d.cells[2, 2].height;
                    int s_width = 1;
                    int s_height = 1;
                    int minimum = int.MaxValue;
                    while (true)
                    {
                        
                        s_height =(int)((((center_width * s_width + add_width) * (float) bit.PixelHeight / (float) bit.PixelWidth) - add_height) / center_height);
                        int difference = dominoes - d.GenerateStructure(s_width, s_height).dominoes.Length;
                        if (Math.Abs(difference) <= minimum)
                        {
                            minimum = difference;
                        }
                        else
                        {
                            s_width--;
                            s_height = (int)((((center_width * s_width + add_width) * (float)bit.PixelHeight / (float)bit.PixelWidth) - add_height) / center_height);
                            break;
                        }
                        s_width++;
                    }

                    if (s_width == 0) s_width++;
                    if (s_height == 0) s_height++;
                    // Copy Image
                    String newpath = System.IO.Path.Combine(project_path, "Source Images", filename + System.IO.Path.GetExtension(image_path));
                    if (!(System.IO.File.Exists(System.IO.Path.Combine(project_path, filename + ".sct")) || File.Exists(newpath)))
                    {
                        File.Copy(image_path, newpath, false);
                        File.SetAttributes(newpath, FileAttributes.Normal);
                        doc = new StructureDocument(System.IO.Path.Combine(project_path, filename + ".sct"), newpath, colors, new ClusterStructureProvider(s_width, s_height, structure_index)) ;
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("A structure with this name already exists. Please choose another name.");
                    }
                }
                else
                {
                    MessageBox.Show("Please select an image.");
                }
            }
        }

        private void StructureSizeChanged(object sender, TextChangedEventArgs e)
        {
            int o;
            if (!(int.TryParse((sender as TextBox).Text, out o)))
            {
                (sender as TextBox).Text = 1000 + "";
                dominoes = 1000;
            }
            else dominoes = o;
        }

        private void StructPossibilities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            structure_index = (sender as ComboBox).SelectedIndex;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ListBoxMain.SelectedIndex = type;
            List_SelectionChanged(new object(), null);
        }

        private void FieldPossibilities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            field_index = (sender as ComboBox).SelectedIndex;
        }
    }
    
   
}
