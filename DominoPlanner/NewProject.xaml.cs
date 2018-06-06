using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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

namespace DominoPlanner
{
    /// <summary>
    /// Interaktionslogik für Window1.xaml
    /// </summary>
    public partial class NewProject: Window
    {
        public String name { get; private set; }
        public String path { get; private set; }
        public String color_path { get; private set; }
        public NewProject()
        {
            InitializeComponent();
            PathTextBox.Text = Properties.Settings.Default.StandardProjectPath;
            path = PathTextBox.Text;
            LabelColorPath.Content = "Path: " + Properties.Settings.Default.StandardColorArray;
            color_path = Properties.Settings.Default.StandardColorArray;
            NameTextBox.Text = "My Project";
        }

        private void OK(object sender, RoutedEventArgs e)
        {
            
            name = NameTextBox.Text;
            path = PathTextBox.Text;
            if (!Directory.Exists(System.IO.Path.Combine(path, name)))
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("This directory already exists. Please select another project name");
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
           DialogResult = false;
           this.Close();
        }

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.Title = "Select a Folder to store your project in.";
            dlg.IsFolderPicker = true;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = dlg.FileName;
                PathTextBox.Text = path;
                //bool flag_opened = false;
                //foreach (String s in Properties.Settings.Default.DocumentList.Split('\n'))
                //{
                //    if (s == folder) flag_opened = true;
                //    MessageBox.Show("This project is already open.");
                //}
            }
            this.Topmost = true;
        }

        private void SelectColorArray(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fld = new OpenFileDialog();
            fld.Title = "Select a color list to add to your project";
            fld.Multiselect = false;
            fld.Filter = "Color files (*.clr)|*.clr";
            if (fld.ShowDialog() == true)
            {
                color_path = fld.FileName;
                LabelColorPath.Content = "Path: " + color_path;
            }
            this.Topmost = true;
            

        }

        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender != null && LabelColorPath != null)
            {
                if (((RadioButton)sender).Content.ToString() == "Use Standard")
                {
                    LabelColorPath.Content = "Path: " + Properties.Settings.Default.StandardColorArray;
                    color_path = Properties.Settings.Default.StandardColorArray;
                }
                else
                {
                    LabelColorPath.Content = "Select file...";
                }
            }
        }
    }
}
