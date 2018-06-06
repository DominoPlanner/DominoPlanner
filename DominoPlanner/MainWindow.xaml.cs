using DominoPlanner.Document_Classes;
using DominoPlanner.Util;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DominoPlanner
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Prop
        private bool _SizeGridEnabled;

        public bool SizeGridEnabled
        {
            get
            {
                return _SizeGridEnabled;
            }
            set
            {
                if (_SizeGridEnabled != value)
                {
                    _SizeGridEnabled = value;
                    OnPropertyChanged("SizeGridEnabled");
                }
            }
        }
        #endregion
        private List<DominoUI> clipboardStones;
        public List<Document> OpenedDocuments;
        //jup, ist hässlich.. mach ich unter MVVM schöner!!
        public List<DominoUI> selectedStones = new List<DominoUI>();

        public MainWindow()
        {
            InitializeComponent();
            SizeGridEnabled = true;
        }

        private void CloseTab(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is TabItem))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            Document Data = (parent as TabItem).DataContext as Document;
            InternalCloseTab(Data);
        }

        private void CreateProject(object sender, RoutedEventArgs e)
        {
            NewProject np = new NewProject();
            np.ShowDialog();
            if (np.DialogResult == true)
            {
                String folder_path = Path.Combine(np.path, np.name);
                String color_path = np.color_path;
                ProjectCreator(folder_path, color_path);
            }

        }
        private void ProjectCreator(String path, String colorpath)
        {
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "Source Images"));
            Directory.CreateDirectory(Path.Combine(path, "Field Protocols"));
            File.Copy(colorpath, Path.Combine(path, Path.GetFileName(colorpath)));
            String mstfile = Path.Combine(path, Path.GetFileName(path) + ".mst");
            File.Create(mstfile);
            Properties.Settings.Default.DocumentList += (path + "\n");
            Properties.Settings.Default.Save();
            RefreshProjectsList();
        }
        private void UpdateTab(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.StructureTemplates = Properties.Settings.Default.Properties["StructureTemplates"].DefaultValue.ToString();
            if (Properties.Settings.Default.FirstStartup)
            {
                FirstStart fs = new FirstStart();
                var result = fs.ShowDialog();
                string path = fs.path;
                Properties.Settings.Default.StandardProjectPath = path;
                Properties.Settings.Default.StandardColorArray = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Appdata", "Local", "DominoPlanner", "dominoes.clr"));
                EditColors ec = new EditColors();
                Directory.CreateDirectory(Path.GetDirectoryName(Properties.Settings.Default.StandardColorArray));
                ec.colors = new ColorArrayDocument(Properties.Settings.Default.StandardColorArray);
                ec.ShowDialog();
            }
            Properties.Settings.Default.Save();
            OpenedDocuments = new List<Document>();
            TabControlMain.ItemsSource = OpenedDocuments;
            RefreshProjectsList();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(null));
            }
        }

        #region ColorDocumentEvents

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
                colors.Add(color);
            }
            (parent as ListView).Items.Refresh();
            (parent as ListView).ScrollIntoView((parent as ListView).Items.GetItemAt(colors.Count - 1));
        }
        private void SaveColorDocumentGlobal(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            ColorArrayDocument clr = ((TabControl)parent).SelectedItem as ColorArrayDocument;
            clr.Save(Properties.Settings.Default.StandardColorArray);
        }

        private void SaveColorDocument(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            ColorArrayDocument clr = ((TabControl)parent).SelectedItem as ColorArrayDocument;
            clr.Save(clr.path);
        }

        #endregion ColorDocumentEvents

        private void Field_Save(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            Document fld = ((TabControl)parent).SelectedItem as Document;
            fld.Save(fld.path);
        }

        private Document RefreshDepObj(object sender, DominoCanvas dc)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }

            Document fld = ((TabControl)parent).SelectedItem as Document;
            if (fld.GetType() == typeof(FieldDocument))
            {
                ((FieldDocument)fld).dominoes = ((FieldDocument)dc.ProjectDoc).dominoes;
                ((FieldDocument)fld).ChangeSize(((FieldDocument)dc.ProjectDoc).length, ((FieldDocument)dc.ProjectDoc).height);
            }
            else if (fld.GetType() == typeof(StructureDocument))
            {
                ((StructureDocument)fld).dominoes = ((StructureDocument)dc.ProjectDoc).dominoes;
            }
            return fld;
        }

        private void FieldSave(object sender, RoutedEventArgs e)
        {
            DominoCanvas dc = (DominoCanvas)((ScrollViewer)((Grid)((Grid)(Grid)((Grid)((Button)sender).Parent).Children[0]).Children[1]).Children[0]).Content;

            Document fld = RefreshDepObj(sender, dc);

            fld.Save(fld.path);
        }

        #region ProjectsTreeView Functions
        private void AddProject(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.Title = "My Title";
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
                string folder = dlg.FileName;
                bool flag_opened = false;
                foreach (String s in Properties.Settings.Default.DocumentList.Split('\n'))
                {
                    if (s == folder)
                    {
                        flag_opened = true;
                        MessageBox.Show("This project is already open.");
                    }
                }
                if (CheckIfValidProject(folder) && !flag_opened)
                {
                    Properties.Settings.Default.DocumentList += (folder + "\n");
                    Properties.Settings.Default.Save();
                    RefreshProjectsList();
                }
            }
        }

        private bool CheckIfValidProject(string path)
        {
            return true;
        }



        private void RefreshProjectsList()
        {
            HomeTVI.Items.Clear();
            String[] splitted = Properties.Settings.Default.DocumentList.Split('\n');
            foreach (String s in splitted)
            {
                try
                {
                    if (s == "") continue;
                    String[] documents = Directory.GetFiles(s);

                    // add masterplan object
                    String MasterPath = Directory.GetFiles(s, "*.mst")[0];
                    TreeViewItem tviMaster = GenerateTreeViewItem(MasterPath, "Icons/Properties.ico", true, true);
                    tviMaster.ContextMenu = ProjectsTV.Resources["MasterplanContextMenu"] as ContextMenu;
                    HomeTVI.Items.Add(tviMaster);

                    // add color array object
                    String ColorPath = Directory.GetFiles(s, "*.clr")[0];
                    TreeViewItem tviColor = GenerateTreeViewItem(ColorPath, "Icons/colorLine.ico", false, true);
                    tviMaster.Items.Add(tviColor);

                    // add all fields
                    String SourcePath = Path.Combine(Path.GetDirectoryName(ColorPath), "Source Images");
                    // add field folder
                    TreeViewItem FieldsFolder = GenerateTreeViewItem(Path.Combine(Path.GetDirectoryName(ColorPath), "Fields"), "Icons/folder_image.ico", true, false);
                    FieldsFolder.ContextMenu = ProjectsTV.Resources["FieldFolderContextMenu"] as ContextMenu;
                    tviMaster.Items.Add(FieldsFolder);
                    foreach (String FieldPath in Directory.GetFiles(s, "*.fld"))
                    {
                        String BackPath = "";
                        foreach (String ImagePath in Directory.GetFiles(SourcePath))
                        {
                            if (Path.GetFileNameWithoutExtension(ImagePath) == Path.GetFileNameWithoutExtension(FieldPath))
                                BackPath = ImagePath;
                        }
                        TreeViewItem tviField = GenerateTreeViewItem(FieldPath, BackPath, false, true);
                        tviField.ContextMenu = ProjectsTV.Resources["FieldContextMenu"] as ContextMenu;
                        FieldsFolder.Items.Add(tviField);

                        // add Source image in collapsed TreeViewItem

                        TreeViewItem tviImage = GenerateTreeViewItem(BackPath, "Icons/image.ico", false, true);
                        tviField.Items.Add(tviImage);
                    }

                    TreeViewItem StructuresFolder = GenerateTreeViewItem(Path.Combine(Path.GetDirectoryName(ColorPath), "Structures"), "Icons/folder_image.ico", true, false);
                    StructuresFolder.ContextMenu = ProjectsTV.Resources["StructureFolderContextMenu"] as ContextMenu;
                    tviMaster.Items.Add(StructuresFolder);
                    foreach (String StructPath in Directory.GetFiles(s, "*.sct"))
                    {
                        String BackPath = "";
                        foreach (String ImagePath in Directory.GetFiles(SourcePath))
                        {
                            if (Path.GetFileNameWithoutExtension(ImagePath) == Path.GetFileNameWithoutExtension(StructPath))
                                BackPath = ImagePath;
                        }
                        TreeViewItem tviField = GenerateTreeViewItem(StructPath, BackPath, false, true);
                        tviField.ContextMenu = ProjectsTV.Resources["StructureContextMenu"] as ContextMenu;
                        StructuresFolder.Items.Add(tviField);

                        // add Source image in collapsed TreeViewItem

                        TreeViewItem tviImage = GenerateTreeViewItem(BackPath, "Icons/image.ico", false, true);
                        tviField.Items.Add(tviImage);
                    }
                }
                catch
                {
                    string openedprojects = "";
                    foreach (String s2 in splitted)
                    {
                        if (s2 != s)
                        {
                            openedprojects += s2 + "\n";
                        }
                    }
                    Properties.Settings.Default.DocumentList = openedprojects;
                    Properties.Settings.Default.Save();

                    MessageBox.Show("Invalid Project removed from list. If it was removed accidentally, try to open it again.");
                    RefreshProjectsList();
                }
            }
        }

        private TreeViewItem GenerateTreeViewItem(String path, String imageSource, bool expanded, bool isEvented)
        {

            TreeViewItem item = new TreeViewItem();
            StackPanel stitem = new StackPanel();
            stitem.Orientation = Orientation.Horizontal;
            Image img = new Image();
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
            img.Height = 16;
            if (imageSource.Contains("Icons/"))
            {
                img.Source = new BitmapImage(new Uri(imageSource, UriKind.RelativeOrAbsolute));
            }
            else
            {
                try
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.DecodePixelHeight = 50; //memory leak
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(imageSource, UriKind.Absolute);
                    image.EndInit();
                    img.Source = image;
                }
                catch
                {

                }
            }
            stitem.Children.Add(img);
            Label la = new Label();
            la.Content = Path.GetFileName(path);
            stitem.Children.Add(la);
            item.Header = stitem;
            item.IsExpanded = expanded;

            if (isEvented)
            {
                item.ToolTip = path;
                item.PreviewMouseDoubleClick += TreeViewItemDoubleClick;
            }
            else
            {
                item.ToolTip = Directory.GetParent(path);
            }
            return item;
        }

        private void TreeViewItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if ((sender as TreeViewItem).IsSelected)
            {
                TreeViewItem i = sender as TreeViewItem;
                String ext = Path.GetExtension(i.ToolTip as String);
                // flag checks if document is already open
                bool flag = false;
                switch (ext)
                {
                    case ".mst":
                        break;

                    case ".clr":
                        foreach (Document cl in OpenedDocuments)
                        {
                            if (cl.path == i.ToolTip as String)
                            {
                                flag = true;
                                TabControlMain.SelectedIndex = OpenedDocuments.IndexOf(cl);
                            }
                        }
                        if (!flag)
                        {
                            OpenedDocuments.Add(ColorArrayDocument.LoadColorArray(i.ToolTip as String));
                        }
                        break;

                    case ".fld":
                        foreach (Document cl in OpenedDocuments)
                        {
                            if (cl.path == i.ToolTip as String)
                            {
                                flag = true;
                                TabControlMain.SelectedIndex = OpenedDocuments.IndexOf(cl);
                            }
                        }
                        if (!flag)
                        {
                            OpenedDocuments.Add(FieldDocument.LoadFieldDocument(i.ToolTip as String));
                        }
                        break;
                    case ".sct":
                        foreach (Document cl in OpenedDocuments)
                        {
                            if (cl.path == i.ToolTip as String)
                            {
                                flag = true;
                                TabControlMain.SelectedIndex = OpenedDocuments.IndexOf(cl);
                            }
                        }
                        if (!flag)
                        {
                            OpenedDocuments.Add(StructureDocument.LoadStructureDocument(i.ToolTip as String));
                        }
                        break;
                    case ".jpg":
                    case ".png":
                    case ".jpeg":
                    case ".jfif":
                        TreeViewItem parent = (TreeViewItem)i.Parent;
                        Document ParentDoc = null;
                        bool lockflag = false;
                        foreach (Document cl in OpenedDocuments)
                        {
                            if (cl.path == parent.ToolTip as String)
                            {
                                flag = true;
                                ParentDoc = cl;
                                if (((ProjectDocument)ParentDoc).Locked == Visibility.Visible) lockflag = true;
                            }
                        }
                        if (!flag)
                        {
                            ParentDoc = Document.Load(parent.ToolTip as String);
                            if (ParentDoc is FieldDocument)
                            {
                                if (!((ParentDoc as FieldDocument).Locked == Visibility.Visible))
                                    OpenedDocuments.Add(ParentDoc);
                                else
                                    lockflag = true;
                            }
                            else
                            {
                                OpenedDocuments.Add(ParentDoc);
                            }
                        }
                        if (lockflag) MessageBox.Show("The basic properties of this item are locked. You have to unlock it to change the basic filters.");
                        flag = false;
                        foreach (Document cl in OpenedDocuments)
                        {
                            if (cl.path == (i.ToolTip as String) && cl.filename == Path.GetFileName(i.ToolTip as String))
                            {
                                flag = true;
                                TabControlMain.SelectedIndex = OpenedDocuments.IndexOf(cl);
                            }
                        }
                        if (!flag && !lockflag)
                        {
                            if (ParentDoc is ProjectDocument)
                            {
                                OpenedDocuments.Add(((ProjectDocument)ParentDoc).filters);
                            }
                        }
                        break;
                }


                if (!flag)
                {
                    TabControlMain.Items.Refresh();
                    TabControlMain.SelectedIndex = TabControlMain.Items.Count - 1;
                }
            }
        }


        #region ContextMenus
        private void ShowProperties(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mal schauen wann hier was kommt...");
        }

        private void DeleteObject(object sender, RoutedEventArgs e)
        {
            MenuItem mn = sender as MenuItem;
            TreeViewItem tvi = null;
            if (mn != null)
            {
                tvi = ((ContextMenu)mn.Parent).PlacementTarget as TreeViewItem;
            }
            String path = tvi.ToolTip.ToString();
            if (Path.GetExtension(path) != ".mst")
            {
                if (MessageBoxResult.OK == MessageBox.Show("Do you really want to delete the file " + Path.GetFileName(path) + "?", "Confirm", MessageBoxButton.OKCancel))
                {
                    foreach (Document d in OpenedDocuments)
                    {
                        if (d.path == path)
                        {
                            OpenedDocuments.Remove(d);
                            TabControlMain.Items.Refresh();
                            break;
                        }
                    }

                    String image = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(path), "Source Images"), Path.GetFileNameWithoutExtension(path) + ".*")[0];

                    File.Delete(path);
                    File.Delete(image);
                    RefreshProjectsList();
                }

            }
            else
            {
                if (MessageBoxResult.OK == MessageBox.Show("Do you really want remove the project " + Path.GetFileNameWithoutExtension(path) + " from your list of opened projects?", "Confirm", MessageBoxButton.OKCancel))
                {
                // close open tabs
                a:
                    foreach (Document d in OpenedDocuments)
                    {
                        if (Path.GetDirectoryName(d.path) == Path.GetDirectoryName(path))
                        {
                            OpenedDocuments.Remove(d);
                            goto a;

                        }
                    }
                    TabControlMain.Items.Refresh();
                    string newlist = "";
                    foreach (string s in Properties.Settings.Default.DocumentList.Split('\n'))
                    {
                        if (s != Path.GetDirectoryName(path) && s != "")
                        {
                            newlist += s + "\n";
                        }
                    }
                    newlist.Trim('\n');
                    Properties.Settings.Default.DocumentList = newlist;
                    Properties.Settings.Default.Save();
                    RefreshProjectsList();
                }
            }

        }

        private void AddNewField(object sender, RoutedEventArgs e)
        {
            MenuItem mn = sender as MenuItem;
            TreeViewItem tvi = null;
            if (mn != null)
            {
                tvi = ((ContextMenu)mn.Parent).PlacementTarget as TreeViewItem;
            }
            AddNewObject a = new AddNewObject();
            if ((AddNewObject.GetVisualChild<Label>(tvi.Header as DependencyObject) as Label).Content.ToString() == "Structures")
            {
                a.type = 1;
            }
            else a.type = 0;
            String path = tvi.ToolTip.ToString();
            FileAttributes fa = File.GetAttributes(path);
            if (fa.HasFlag(FileAttributes.Directory))
            {
                a.project_path = path;
            }
            else
            {
                a.project_path = Path.GetDirectoryName(path);
            }
            String ColorPath = Directory.GetFiles(a.project_path, "*.clr")[0];
            a.colors = (ColorArrayDocument.LoadColorArray(ColorPath) as ColorArrayDocument).cols;
            if (a.ShowDialog() == true)
            {
                Document d = a.doc;
                if (a.doc is FieldDocument)
                {
                    //d = (a.doc as FieldDocument);
                    (d as FieldDocument).GenerateField();
                    (d as FieldDocument).img = (d as FieldDocument).DrawField();

                }
                //if (a.doc is StructureDocument)
                //{
                //    if (a.ShowDialog() == true)
                //    {
                //        //StructureDocument fd = (a.doc as StructureDocument);
                //    }
                //}
                d.Save(d.path);
                OpenedDocuments.Add(d);
                TabControlMain.Items.Refresh();
                TabControlMain.SelectedIndex = TabControlMain.Items.Count - 1;
                RefreshProjectsList();
            }
        }

        private void ExportPNG(object sender, RoutedEventArgs e)
        {
            MenuItem mn = sender as MenuItem;
            TreeViewItem tvi = null;
            if (mn != null)
            {
                tvi = ((ContextMenu)mn.Parent).PlacementTarget as TreeViewItem;
            }
            String filePath = tvi.ToolTip.ToString();

            Document d = Document.FastLoad(filePath);
            foreach (Document doc in OpenedDocuments)
            {
                if (filePath == doc.path) // document open
                {
                    if (!doc.Compare(d)) // unsaved changes
                    {
                        if (MessageBox.Show("Object is currently opened and has unsaved changes. Do you want to save?", "Save file?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            doc.Save(doc.path);
                            doc.SavePNG();
                            return;
                        }
                    }

                }
            }
            d.SavePNG();

        }

        private void GenerateFieldPlan(object sender, RoutedEventArgs e)
        {
            MenuItem mn = sender as MenuItem;
            TreeViewItem tvi = null;
            if (mn != null)
            {
                tvi = ((ContextMenu)mn.Parent).PlacementTarget as TreeViewItem;
            }
            String filePath = tvi.ToolTip.ToString();
            Document d = Document.FastLoad(filePath);
            foreach (Document doc in OpenedDocuments)
            {
                if (filePath == doc.path) // document open
                {
                    if (!doc.Compare(d)) // unsaved changes
                    {
                        if (MessageBox.Show("Object is currently opened and has unsaved changes. Do you want to save?", "Save file?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            doc.Save(doc.path);
                            InternalShowFieldPlan(doc);
                            return;
                        }
                    }

                }
            }
            InternalShowFieldPlan(d);
        }
        private void InternalShowFieldPlan(Document d)
        {
            FieldPlanEditor fe = new FieldPlanEditor();
            if (d is FieldDocument)
                fe.basedoc = d as FieldDocument;
            else if (d is StructureDocument)
            {
                if (!(d as StructureDocument).HasProtocolDefinition)
                {
                    MessageBox.Show("This type of structure does not contain a protocol definition.", "Something is missing here."); return;
                }
                else fe.basedoc = (d as StructureDocument).GenerateBaseField();
            }
            else return;
            fe.ShowDialog();
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            MenuItem mn = sender as MenuItem;
            TreeViewItem tvi = null;
            if (mn != null)
            {
                tvi = ((ContextMenu)mn.Parent).PlacementTarget as TreeViewItem;
            }
            String filePath = tvi.ToolTip.ToString();
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
        }

        #endregion

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.FirstStartup)
            {
                Properties.Settings.Default.FirstStartup = false;
                Properties.Settings.Default.Save();

            }

        }



        #endregion ProjectsTreeView Functions

        private void ManualEdit(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            Document actOpen = ((TabControl)parent).SelectedItem as Document;
            Document OldDocument = Document.FastLoad(actOpen.path);
            if (!actOpen.Compare(OldDocument))
            {
                actOpen.Save(actOpen.path);
            }
            if (actOpen.GetType() == typeof(FieldDocument))
                (actOpen as FieldDocument).Locked = Visibility.Visible;
            else if (actOpen.GetType() == typeof(StructureDocument))
                (actOpen as StructureDocument).Locked = Visibility.Visible;

            SizeGridEnabled = false;
            Document DeleteDoc = null;
            foreach (Document cl in OpenedDocuments)
            {
                if (actOpen is ProjectDocument)
                {
                    if (cl.path == (actOpen as ProjectDocument).filters.path)
                    {
                        DeleteDoc = cl;
                    }
                }

            }
            if (DeleteDoc != null)
            {
                OpenedDocuments.Remove(DeleteDoc);
                TabControlMain.Items.Refresh();
                TabControlMain.SelectedIndex = 0;
                GC.Collect();
            }
        }

        private void UnlockUI(object sender, RoutedEventArgs e)
        {
            // TODO: Vielleicht als Kopie speichern?
            if (MessageBox.Show("All manual changes will be lost, but you can change size or colors of the field.", "Do you really want to discard all changes?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
                while (!(parent is TabControl))
                {
                    parent = VisualTreeHelper.GetParent(parent as DependencyObject);
                }
                if (((TabControl)parent).SelectedItem.GetType() == typeof(FieldDocument))
                {
                    FieldDocument actOpen = ((TabControl)parent).SelectedItem as FieldDocument;
                    actOpen.Locked = Visibility.Hidden;
                }
                else if (((TabControl)parent).SelectedItem.GetType() == typeof(StructureDocument))
                {
                    StructureDocument actOpen = ((TabControl)parent).SelectedItem as StructureDocument;
                    actOpen.Locked = Visibility.Hidden;
                }
                TabControlMain.ContentTemplateSelector = new TabTemplateSelector();
                SizeGridEnabled = true;
            }
        }

        private void ListView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DominoCanvas tmpCanvas = (DominoCanvas)((ScrollViewer)((Grid)((Grid)((Grid)((ListView)sender).Parent).Parent).Children[1]).Children[0]).Content;
            if (tmpCanvas.ProjectDoc.GetType() == typeof(FieldDocument))
            {
                FieldDocument fld = (FieldDocument)tmpCanvas.ProjectDoc;
                foreach (DominoUI actDomino in selectedStones)
                {
                    if (actDomino.path == fld.path)
                    {
                        actDomino.isSelected = false;
                        actDomino.ColorValue = ((ListView)sender).SelectedIndex;
                        fld.dominoes[actDomino.stoneWidth, actDomino.stoneHeight] = actDomino.ColorValue;
                        if (!new List<DominoColor>(tmpCanvas.ProjectDoc.UsedColors).Exists(x => x.rgb == ((SolidColorBrush)actDomino.Fill).Color))
                            tmpCanvas.ProjectDoc.UsedColors.Add((DominoColor)((ListView)sender).SelectedValue);
                    }
                }
            }
            else if (tmpCanvas.ProjectDoc.GetType() == typeof(StructureDocument))
            {
                StructureDocument fld = (StructureDocument)tmpCanvas.ProjectDoc;
                foreach (DominoUI actDomino in selectedStones)
                {
                    if (actDomino.path == fld.path)
                    {
                        actDomino.isSelected = false;
                        actDomino.ColorValue = ((ListView)sender).SelectedIndex;
                        fld.dominoes[actDomino.arrayCounter] = actDomino.ColorValue;
                        if (!new List<DominoColor>(tmpCanvas.ProjectDoc.UsedColors).Exists(x => x.rgb == ((SolidColorBrush)actDomino.Fill).Color))
                            tmpCanvas.ProjectDoc.UsedColors.Add((DominoColor)((ListView)sender).SelectedValue);
                    }
                }
            }
            selectedStones.Clear();
            RefreshDepObj(sender, tmpCanvas);
        }


        private void FillList(object sender, RoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            List<DominoColor> list = lv.DataContext as List<DominoColor>;
            GridView grid = lv.View as GridView;
            //get current document
            while (grid.Columns.Count > 5)
            {
                grid.Columns.RemoveAt(5);
            }

            ColorArrayDocument doc = TabControlMain.SelectedItem as ColorArrayDocument;

            List<String> files_list = new List<String>();
            //fill projectslist with paths of all fields and objects (apart from masterplans and colorarrays)

            for (int i = 0; i < doc.cols.Count; i++)
            {
                doc.cols[i].used_in_projects = new List<int>();
            }

            // fields
            int counter = 0;
            foreach (String file in Directory.GetFiles(Path.GetDirectoryName(doc.path), "*.fld"))
            {
                FieldDocument fld = FieldDocument.LoadFieldDocumentWithoutDrawing(file);
                if (ColorArrayDocument.CompareLists(fld.Colors, doc.cols))
                {
                    fld.CalculateUsedDominoes();
                    files_list.Add(fld.filename);
                    for (int i = 0; i < doc.cols.Count; i++)
                    {
                        doc.cols[i].used_in_projects.Add(fld.used_dominoes[i]);
                    }
                    GridViewColumn c1 = new GridViewColumn();
                    DataTemplate t = new DataTemplate();
                    t.VisualTree = new FrameworkElementFactory(typeof(Label));
                    MultiBinding mb = new MultiBinding();
                    mb.Bindings.Add(new Binding("used_in_projects[" + counter + "]"));
                    mb.Bindings.Add(new Binding("count"));
                    mb.Converter = new IsTooMuchConverter();
                    t.VisualTree.SetBinding(Label.ForegroundProperty,
                        mb);
                    t.VisualTree.SetBinding(Label.ContentProperty, new Binding("used_in_projects[" + counter + "]"));
                    c1.Header = "  " + fld.filename + "  ";
                    c1.CellTemplate = t;
                    //c1.DisplayMemberBinding = new Binding("used_in_projects[" + counter + "]");
                    grid.Columns.Add(c1);
                    counter++;
                }
            }
            // structures
            foreach (String file in Directory.GetFiles(Path.GetDirectoryName(doc.path), "*.sct"))
            {
                StructureDocument std = StructureDocument.LoadStructureDocumentWithoutDrawing(file);
                if (ColorArrayDocument.CompareLists(std.Colors, doc.cols))
                {
                    int[] used = new int[std.Colors.Count];
                    for (int i = 0; i < std.dominoes.Length; i++)
                    {
                        used[std.dominoes[i]]++;
                    }
                    files_list.Add(std.filename);
                    for (int i = 0; i < doc.cols.Count; i++)
                    {
                        doc.cols[i].used_in_projects.Add(used[i]);
                    }
                    GridViewColumn c1 = new GridViewColumn();
                    DataTemplate t = new DataTemplate();
                    t.VisualTree = new FrameworkElementFactory(typeof(Label));
                    MultiBinding mb = new MultiBinding();
                    mb.Bindings.Add(new Binding("used_in_projects[" + counter + "]"));
                    mb.Bindings.Add(new Binding("count"));
                    mb.Converter = new IsTooMuchConverter();
                    t.VisualTree.SetBinding(Label.ForegroundProperty,
                        mb);
                    t.VisualTree.SetBinding(ContentProperty, new Binding("used_in_projects[" + counter + "]"));
                    c1.Header = "  " + std.filename + "  ";
                    c1.CellTemplate = t;
                    //c1.DisplayMemberBinding = new Binding("used_in_projects[" + counter + "]");
                    grid.Columns.Add(c1);
                    counter++;
                }
            }


        }

        private void ListFiller(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as ListView).Items.Count > 0)
                FillList(sender, null);
        }

        private void ShowFieldplan(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            Document fld = ((TabControl)parent).SelectedItem as Document;
            InternalShowFieldPlan(fld);

        }

        private void ShowBlockViewer(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            Document fld = ((TabControl)parent).SelectedItem as Document;

            FieldBlockViewerIntroduction fbvi = new FieldBlockViewerIntroduction();
            fbvi.fld = fld as FieldDocument;
            fbvi.ShowDialog();
        }

        #region Filter Editor Methods

        #region Filter Editor: Drag Drop between the two lists (TODO)
        ListView dragSource = null;
        private void AvailFiltersList_DragPreview(object sender, MouseButtonEventArgs e)
        {

            ListView parent = (ListView)sender;
            dragSource = parent;
            object data = GetDataFromListView(dragSource, e.GetPosition(parent));

            if (data != null)
            {
                DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
            }

        }
        private static object GetDataFromListView(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }
        #endregion
        private void AvailFiltesList_AddFilter(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            ImageEditingDocument img = ((TabControl)parent).SelectedItem as ImageEditingDocument;
            Filter f = (Filter)((Button)sender).DataContext;
            img.applied_filters.Add(Activator.CreateInstance(f.GetType()) as Filter);
            img.OnPropertyChanged(null);
        }

        private void ApplFiltersList_Drop(object sender, DragEventArgs e)
        {
            ListView parent = (ListView)sender;
            object data = e.Data.GetData(typeof(string));
            parent.Items.Add(data);
        }

        private void ApplFiltersListItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {

            }
        }

        private void ApplFiltersListItem_Remove(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            ListView lv = null;
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
                if (parent is ListView) lv = parent as ListView;

            }
            ImageEditingDocument img = ((TabControl)parent).SelectedItem as ImageEditingDocument;
            img.applied_filters.Remove(((Button)sender).DataContext as Filter);
            lv.Items.Refresh();
            img.OnPropertyChanged(null);
        }

        //refreshes the filter preview.
        private void ApplFiltersListItem_Edit(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            ListView lv = null;
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
                if (parent is ListView) lv = parent as ListView;

            }
            ImageEditingDocument img = ((TabControl)parent).SelectedItem as ImageEditingDocument;
            Filter f = (Filter)((Button)sender).DataContext;
            EditFilter fi = new EditFilter();
            fi.filter = f;

            fi.source = img.source;
            if (fi.ShowDialog() == true)
            {
                f = fi.filter;
                img.OnPropertyChanged(null);
                lv.Items.Refresh();
            }

        }

        private void ApplFiltersListItem_MoveUp(object sender, RoutedEventArgs e)
        {
            Filter f = (Filter)((Button)sender).DataContext;
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            ListView lv = null;
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
                if (parent is ListView) lv = parent as ListView;
            }
            ImageEditingDocument img = ((TabControl)parent).SelectedItem as ImageEditingDocument;
            int index = img.applied_filters.IndexOf(f);
            if (index > 0)
            {
                Filter temp = img.applied_filters[index - 1];
                img.applied_filters[index - 1] = f;
                img.applied_filters[index] = temp;
                lv.Items.Refresh();
                lv.SelectedIndex = --index;
                lv.ScrollIntoView(lv.Items.GetItemAt(index));
            }
            img.OnPropertyChanged(null);
        }

        private void ApplFiltersListItem_MoveDown(object sender, RoutedEventArgs e)
        {
            Filter f = (Filter)((Button)sender).DataContext;
            DependencyObject parent = VisualTreeHelper.GetParent(sender as DependencyObject);
            ListView lv = null;
            while (!(parent is TabControl))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
                if (parent is ListView) lv = parent as ListView;
            }
            ImageEditingDocument img = ((TabControl)parent).SelectedItem as ImageEditingDocument;
            int index = img.applied_filters.IndexOf(f);
            if (index < img.applied_filters.Count - 1)
            {
                Filter temp = img.applied_filters[index + 1];
                img.applied_filters[index + 1] = f;
                img.applied_filters[index] = temp;
                lv.Items.Refresh();
                lv.SelectedIndex = ++index;
                lv.ScrollIntoView(lv.Items.GetItemAt(index));
            }
            img.OnPropertyChanged(null);
        }

        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            /*foreach (DominoUI actDomino in selectedStones)
            {
                actDomino.isSelected = false;
            }*/
        }

        private void unselectAll()
        {
            foreach (DominoUI actDomino in selectedStones)
                actDomino.isSelected = false;
            selectedStones.Clear();
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            DominoCanvas domCan = (DominoCanvas)((ScrollViewer)sender).Content;
            LoadCanvas(domCan);
        }

        private void ScrollViewer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((DominoCanvas)((ScrollViewer)sender).Content).ProjectDoc == null)
            {
                if (((ScrollViewer)((DominoCanvas)((ScrollViewer)sender).Content).Parent).ToolTip.ToString().EndsWith(".fld"))
                    ((DominoCanvas)((ScrollViewer)sender).Content).ProjectDoc = FieldDocument.LoadFieldDocumentWithoutDrawing(((ScrollViewer)((DominoCanvas)((ScrollViewer)sender).Content).Parent).ToolTip.ToString());
                else
                    ((DominoCanvas)((ScrollViewer)sender).Content).ProjectDoc = StructureDocument.LoadStructureDocumentWithoutDrawing(((ScrollViewer)((DominoCanvas)((ScrollViewer)sender).Content).Parent).ToolTip.ToString());
            }
        }

        private void LoadCanvas(object sender)
        {
            DominoCanvas tmpCanvas = (DominoCanvas)sender;
            tmpCanvas.MouseUp += Canvas_MouseUp;
            tmpCanvas.Children.Clear();
            unselectAll();
            if (((ScrollViewer)tmpCanvas.Parent).ToolTip.ToString().EndsWith(".fld"))
            {
                if (tmpCanvas.ProjectDoc == null || ((FieldDocument)tmpCanvas.ProjectDoc).dominoes == null)
                    tmpCanvas.ProjectDoc = FieldDocument.LoadFieldDocumentWithoutDrawing(((ScrollViewer)tmpCanvas.Parent).ToolTip.ToString());
                int stoneHeight = 0, stoneWidth = 0, spaceWidth = 0, spaceHeight = 0;
                if (((FieldDocument)tmpCanvas.ProjectDoc).ShowSpaces)
                {
                    stoneWidth = ((FieldDocument)tmpCanvas.ProjectDoc).a;
                    stoneHeight = ((FieldDocument)tmpCanvas.ProjectDoc).c;
                    spaceWidth = ((FieldDocument)tmpCanvas.ProjectDoc).b;
                    spaceHeight = ((FieldDocument)tmpCanvas.ProjectDoc).d;
                }
                else
                {
                    stoneWidth = (((FieldDocument)(FieldDocument)tmpCanvas.ProjectDoc).a) + (((FieldDocument)(FieldDocument)tmpCanvas.ProjectDoc).b) / 2;
                    stoneHeight = (((FieldDocument)(FieldDocument)tmpCanvas.ProjectDoc).c);
                    spaceWidth = 0;
                    spaceHeight = 0;
                }

                tmpCanvas.projectHeight = ((stoneWidth + spaceWidth) * tmpCanvas.ProjectDoc.length);
                tmpCanvas.projectWidth = ((stoneHeight + spaceHeight) * tmpCanvas.ProjectDoc.height);
                double ScaleX = ((ScrollViewer)tmpCanvas.Parent).ActualWidth / tmpCanvas.projectHeight;
                double ScaleY = ((ScrollViewer)tmpCanvas.Parent).ActualHeight / tmpCanvas.projectWidth;

                if (ScaleX < ScaleY)
                {
                    tmpCanvas.Height = tmpCanvas.projectHeight * ScaleX;
                    tmpCanvas.Width = tmpCanvas.projectWidth * ScaleX;
                    tmpCanvas.RenderTransform = new ScaleTransform(ScaleX, ScaleX);
                }
                else
                {
                    tmpCanvas.Height = tmpCanvas.projectHeight * ScaleY;
                    tmpCanvas.Width = tmpCanvas.projectWidth * ScaleY;
                    tmpCanvas.RenderTransform = new ScaleTransform(ScaleY, ScaleY);
                }
                List<DominoColor> tmpList = new List<DominoColor>();
                for (int i = 0; i < tmpCanvas.ProjectDoc.height; i++)
                {
                    for (int k = 0; k < tmpCanvas.ProjectDoc.length; k++)
                    {
                        DominoUI rect = new DominoUI(tmpCanvas.ProjectDoc.Colors, tmpCanvas.ProjectDoc.path, k, i, ((FieldDocument)tmpCanvas.ProjectDoc).dominoes[k, i], stoneWidth, stoneHeight);
                        rect.Width = stoneWidth;
                        rect.Height = stoneHeight;
                        rect.Margin = new Thickness((stoneWidth + spaceWidth) * k, (stoneHeight + spaceHeight) * i, 0, 0);
                        rect.MouseDown += Rect_MouseDown;
                        tmpCanvas.Children.Add(rect);
                    }
                }
            }
            else
            {
                if (tmpCanvas.ProjectDoc == null || ((StructureDocument)tmpCanvas.ProjectDoc).dominoes == null)
                    tmpCanvas.ProjectDoc = StructureDocument.LoadStructureDocumentWithoutDrawing(((ScrollViewer)tmpCanvas.Parent).ToolTip.ToString());

                if (((StructureDocument)tmpCanvas.ProjectDoc).typ == structure_type.Rectangular)
                {
                    for (int i = 0; i < ((StructureDocument)tmpCanvas.ProjectDoc).dominoes.Length; i++)
                    {
                        //DominoUI rect = new DominoUI(((StructureDocument)tmpCanvas.ProjectDoc).Colors, ((StructureDocument)tmpCanvas.ProjectDoc).path, ((StructureDocument)tmpCanvas.ProjectDoc).shapes[i], ((StructureDocument)tmpCanvas.ProjectDoc).dominoes[i]);
                        DominoUI rect = new DominoUI(((StructureDocument)tmpCanvas.ProjectDoc).Colors, ((StructureDocument)tmpCanvas.ProjectDoc).path, 0, 0, ((StructureDocument)tmpCanvas.ProjectDoc).dominoes[i], (int)((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).width, (int)((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).height);
                        rect.arrayCounter = i;
                        rect.Margin = new Thickness(((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).x, ((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).y, 0, 0);
                        rect.MouseDown += Rect_MouseDown;
                        tmpCanvas.Children.Add(rect);
                        if (((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).x > tmpCanvas.projectWidth)
                            tmpCanvas.projectWidth = ((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).x + ((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).width;

                        if (((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).y > tmpCanvas.projectHeight)
                            tmpCanvas.projectHeight = ((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).y + ((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).height;
                    }
                }
                else
                {
                    tmpCanvas.projectHeight = 0;
                    tmpCanvas.projectWidth = 0;
                    for (int i = 0; i < ((StructureDocument)tmpCanvas.ProjectDoc).dominoes.Length; i++)
                    {
                        DominoUI rect = new DominoUI(((StructureDocument)tmpCanvas.ProjectDoc).Colors, ((StructureDocument)tmpCanvas.ProjectDoc).path, ((StructureDocument)tmpCanvas.ProjectDoc).shapes[i], ((StructureDocument)tmpCanvas.ProjectDoc).dominoes[i]);
                        rect.arrayCounter = i;
                        rect.MouseDown += Rect_MouseDown;
                        tmpCanvas.Children.Add(rect);
                        for (int x = 0; x < 4; x++)
                        {
                            if (tmpCanvas.projectWidth < ((PathDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).xCoordinates[x])
                                tmpCanvas.projectWidth = ((PathDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).xCoordinates[x];

                            if (tmpCanvas.projectHeight < ((PathDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).yCoordinates[x])
                                tmpCanvas.projectHeight = ((PathDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).yCoordinates[x];
                        }
                        /*if (((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).x > tmpCanvas.projectWidth)
                            tmpCanvas.projectWidth = ((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).x + ((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).width;

                        if (((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).y > tmpCanvas.projectHeight)
                            tmpCanvas.projectHeight = ((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).y + ((RectangleDomino)((StructureDocument)tmpCanvas.ProjectDoc).shapes[i]).height;*/
                    }
                }

                double ScaleX = ((ScrollViewer)tmpCanvas.Parent).ActualWidth / tmpCanvas.projectWidth;
                double ScaleY = ((ScrollViewer)tmpCanvas.Parent).ActualHeight / tmpCanvas.projectHeight;

                if (ScaleX < ScaleY)
                {
                    tmpCanvas.Height = tmpCanvas.projectHeight * ScaleX;
                    tmpCanvas.Width = tmpCanvas.projectWidth * ScaleX;
                    tmpCanvas.RenderTransform = new ScaleTransform(ScaleX, ScaleX);
                }
                else
                {
                    tmpCanvas.Height = tmpCanvas.projectHeight * ScaleY;
                    tmpCanvas.Width = tmpCanvas.projectWidth * ScaleY;
                    tmpCanvas.RenderTransform = new ScaleTransform(ScaleY, ScaleY);
                }
            }

            RefreshDepObj(sender, tmpCanvas);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DominoCanvas doCanvas = (DominoCanvas)sender;
            DominoUI lastStone = new DominoUI();
            if (doCanvas.ProjectDoc.GetType() == typeof(StructureDocument))
            {
                if (doCanvas.rect != null && doCanvas.rect.Visibility == Visibility.Hidden)
                {
                    doCanvas.Children.Remove(((DominoCanvas)sender).rect);
                    doCanvas.rect = null;
                    return;
                }
                double asdf = e.GetPosition((Canvas)sender).X;
                double ysdf = e.GetPosition((Canvas)sender).Y;
                for (int i = 0; i < ((DominoCanvas)sender).Children.Count - 1; i++)
                {
                    if (((DominoUI)((DominoCanvas)sender).Children[i]).Margin.Top < ysdf && (((DominoUI)((DominoCanvas)sender).Children[i]).Margin.Top + ((DominoUI)((DominoCanvas)sender).Children[i]).ActualHeight) > ysdf)
                    {
                        if (((DominoUI)((DominoCanvas)sender).Children[i]).Margin.Left < asdf && (((DominoUI)((DominoCanvas)sender).Children[i]).Margin.Left + ((DominoUI)((DominoCanvas)sender).Children[i]).ActualWidth) > asdf)
                        {
                            lastStone = ((DominoUI)((DominoCanvas)sender).Children[i]);
                        }
                    }
                }

                System.Windows.Shapes.Rectangle rect = ((DominoCanvas)sender).rect;
                if (rect == null) return;
                double x = Canvas.GetLeft(rect);
                double y = Canvas.GetTop(rect);
                ((Canvas)sender).Children.Remove(((DominoCanvas)sender).rect);
                ((DominoCanvas)sender).rect = null;

                foreach (DominoUI actDomino in doCanvas.Children)
                {
                    if (((StructureDocument)doCanvas.ProjectDoc).typ != structure_type.Rectangular)
                    {
                        if (actDomino.RenderedGeometry.Bounds.TopLeft.Y > y && actDomino.RenderedGeometry.Bounds.TopLeft.Y < (y + rect.Height) && actDomino.RenderedGeometry.Bounds.TopLeft.X > x && actDomino.RenderedGeometry.Bounds.TopLeft.X < (x + rect.Width)) {
                            actDomino.isSelected = true;
                            if (!selectedStones.Exists(act => act == actDomino))
                            {
                                selectedStones.Add(actDomino);
                            }
                        }
                        if (actDomino.RenderedGeometry.Bounds.TopRight.Y > y && actDomino.RenderedGeometry.Bounds.TopRight.Y < (y + rect.Height) && actDomino.RenderedGeometry.Bounds.TopRight.X > x && actDomino.RenderedGeometry.Bounds.TopRight.X < (x + rect.Width)) {
                            actDomino.isSelected = true;
                            if (!selectedStones.Exists(act => act == actDomino))
                            {
                                selectedStones.Add(actDomino);
                            }
                        }
                        if (actDomino.RenderedGeometry.Bounds.BottomLeft.Y > y && actDomino.RenderedGeometry.Bounds.BottomLeft.Y < (y + rect.Height) && actDomino.RenderedGeometry.Bounds.BottomLeft.X > x && actDomino.RenderedGeometry.Bounds.BottomLeft.X < (x + rect.Width)) {
                            actDomino.isSelected = true;
                            if (!selectedStones.Exists(act => act == actDomino))
                            {
                                selectedStones.Add(actDomino);
                            }
                        }
                        if (actDomino.RenderedGeometry.Bounds.BottomRight.Y > y && actDomino.RenderedGeometry.Bounds.BottomRight.Y < (y + rect.Height) && actDomino.RenderedGeometry.Bounds.BottomRight.X > x && actDomino.RenderedGeometry.Bounds.BottomRight.X < (x + rect.Width)) {
                            actDomino.isSelected = true;
                            if (!selectedStones.Exists(act => act == actDomino))
                            {
                                selectedStones.Add(actDomino);
                            }
                        }
                    }
                    else
                    {

                        if (actDomino.Margin.Top > y && actDomino.Margin.Top < (y + rect.Height) || actDomino.Margin.Top < y && (actDomino.Margin.Top + actDomino.Height) > y)
                        {
                            if (actDomino.Margin.Left > x && actDomino.Margin.Left < (x + rect.Width) || actDomino.Margin.Left < x && (actDomino.Margin.Left + actDomino.Width) > x)
                            {
                                //if (actDomino.isSelected == false)
                                //{
                                actDomino.isSelected = true;
                                if (!selectedStones.Exists(act => act == actDomino))
                                {
                                    selectedStones.Add(actDomino);
                                }
                                /*}else
                                {
                                    actDomino.isSelected = false;
                                    selectedStones.Remove(actDomino);
                                }*/
                            }

                        }
                    }
                }
                return;
            }
            if (doCanvas.rect == null) return;
            if (doCanvas.rect.Visibility == Visibility.Hidden)
            {
                doCanvas.Children.Remove(((DominoCanvas)sender).rect);
                doCanvas.rect = null;
                return;
            }
            if (doCanvas.rect == null) return;
            int stoneHeight = 0, stoneWidth = 0, spaceWidth = 0, spaceHeight = 0;
            if (((FieldDocument)doCanvas.ProjectDoc).ShowSpaces)
            {
                stoneWidth = ((FieldDocument)doCanvas.ProjectDoc).a;
                stoneHeight = ((FieldDocument)doCanvas.ProjectDoc).c;
                spaceWidth = ((FieldDocument)doCanvas.ProjectDoc).b;
                spaceHeight = ((FieldDocument)doCanvas.ProjectDoc).d;
            }
            else
            {
                stoneWidth = (((FieldDocument)doCanvas.ProjectDoc).a) + (((FieldDocument)doCanvas.ProjectDoc).b) / 2;
                stoneHeight = (((FieldDocument)doCanvas.ProjectDoc).c);
                spaceWidth = 0;
                spaceHeight = 0;
            }

            int left = 0, right = 0, bottom = 0, height = 0;

            int setX = (stoneWidth + spaceWidth) * (((int)((DominoCanvas)sender).SelectionStartPoint.X) / (stoneWidth + spaceWidth));
            int setY = (stoneHeight + spaceHeight) * (((int)((DominoCanvas)sender).SelectionStartPoint.Y) / (stoneHeight + spaceHeight));

            if (doCanvas.SelectionStartPoint.X - e.GetPosition((Canvas)sender).X <= 0)
            {
                left = (int)(doCanvas.SelectionStartPoint.X) / (stoneWidth + spaceWidth) + 1;
                right = (int)((doCanvas.rect.ActualWidth + doCanvas.SelectionStartPoint.X)) / (stoneWidth + spaceWidth) + 1;
            }
            else
            {
                setX += (stoneWidth + spaceWidth);
                right = (int)(doCanvas.SelectionStartPoint.X) / (stoneWidth + spaceWidth) + 1;
                left = (int)(doCanvas.SelectionStartPoint.X - doCanvas.rect.ActualWidth) / (stoneWidth + spaceWidth) + 1;
            }

            if (doCanvas.SelectionStartPoint.Y - e.GetPosition((Canvas)sender).Y <= 0)
            {
                height = (int)(doCanvas.SelectionStartPoint.Y) / (stoneHeight + spaceHeight) + 1;
                bottom = (int)((doCanvas.rect.ActualHeight + doCanvas.SelectionStartPoint.Y)) / (stoneHeight + spaceHeight) + 1;
            }
            else
            {
                setY += (stoneHeight + spaceHeight);
                bottom = (int)(doCanvas.SelectionStartPoint.Y) / (stoneHeight + spaceHeight) + 1;
                height = (int)(doCanvas.SelectionStartPoint.Y - doCanvas.rect.ActualHeight) / (stoneHeight + spaceHeight) + 1;
            }

            ((DominoCanvas)sender).SelectionStartPoint = new Point(setX, setY);

            for (int i = height - 1; i < bottom; i++)
            {
                for (int k = left - 1; k < right; k++)
                {
                    /*if (selectedStones.Contains(((DominoUI)doCanvas.Children[(i * doCanvas.actDocument.length) + k])))
                    {
                        ((DominoUI)doCanvas.Children[(i * doCanvas.actDocument.length) + k]).isSelected = false;
                        selectedStones.Remove(((DominoUI)doCanvas.Children[(i * doCanvas.actDocument.length) + k]));
                    }
                    else
                    {*/
                    ((DominoUI)doCanvas.Children[(i * doCanvas.ProjectDoc.length) + k]).isSelected = true;
                    if (!selectedStones.Exists(x => x == ((DominoUI)doCanvas.Children[(i * doCanvas.ProjectDoc.length) + k])))
                    {
                        selectedStones.Add(((DominoUI)doCanvas.Children[(i * doCanvas.ProjectDoc.length) + k]));
                    }
                    //}
                }
            }

            ((Canvas)sender).Children.Remove(((DominoCanvas)sender).rect);
            ((DominoCanvas)sender).rect = null;
        }

        private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DominoUI tmpDomino = (DominoUI)sender;
            if (tmpDomino.isSelected == true)
            {
                selectedStones.Remove(tmpDomino);
                tmpDomino.isSelected = false;
            }
            else
            {
                if (!selectedStones.Exists(x => x == tmpDomino))
                {
                    selectedStones.Add(tmpDomino);
                    (tmpDomino).isSelected = true;
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((int)e.OldValue == (int)e.NewValue || (int)e.NewValue == 0) return;

            DominoCanvas actCanvas = (DominoCanvas)((ScrollViewer)((Grid)((Grid)((Grid)((Slider)sender).Parent).Parent).Parent).Children[0]).Content;

            double Scale = actCanvas.RenderTransform.Value.M11 / (int)e.OldValue * (int)e.NewValue;

            actCanvas.Height = actCanvas.Height / actCanvas.RenderTransform.Value.M11 * Scale;
            actCanvas.Width = actCanvas.Width / actCanvas.RenderTransform.Value.M11 * Scale;
            actCanvas.RenderTransform = new ScaleTransform(Scale, Scale);

            if (((int)e.NewValue) == 1)
            {
                double ScaleX = ((ScrollViewer)actCanvas.Parent).ActualWidth / actCanvas.projectWidth;
                double ScaleY = ((ScrollViewer)actCanvas.Parent).ActualHeight / actCanvas.projectHeight;

                if (ScaleX < ScaleY)
                {
                    actCanvas.Height = ((ScrollViewer)actCanvas.Parent).ActualHeight;
                    actCanvas.Width = ((ScrollViewer)actCanvas.Parent).ActualWidth;
                    actCanvas.RenderTransform = new ScaleTransform(ScaleX, ScaleX);
                }
                else
                {
                    actCanvas.Height = ((ScrollViewer)actCanvas.Parent).ActualHeight;
                    actCanvas.Width = ((ScrollViewer)actCanvas.Parent).ActualWidth;
                    actCanvas.RenderTransform = new ScaleTransform(ScaleY, ScaleY);
                }
            }
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DominoCanvas actCanvas = (DominoCanvas)((ScrollViewer)sender).Content;
            double ScaleX = ((ScrollViewer)actCanvas.Parent).ActualWidth / actCanvas.projectWidth;
            double ScaleY = ((ScrollViewer)actCanvas.Parent).ActualHeight / actCanvas.projectHeight;

            if (ScaleX < ScaleY)
            {
                actCanvas.Height = ((ScrollViewer)actCanvas.Parent).ActualHeight;
                actCanvas.Width = ((ScrollViewer)actCanvas.Parent).ActualWidth;
                actCanvas.RenderTransform = new ScaleTransform(ScaleX, ScaleX);
            }
            else
            {
                actCanvas.Height = ((ScrollViewer)actCanvas.Parent).ActualHeight;
                actCanvas.Width = ((ScrollViewer)actCanvas.Parent).ActualWidth;
                actCanvas.RenderTransform = new ScaleTransform(ScaleY, ScaleY);
            }
        }

        private void DominoCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                for (int i = 0; i < selectedStones.Count; i++)
                {
                    if (selectedStones[i].path == ((DominoCanvas)(((ScrollViewer)sender).Content)).ProjectDoc.path)
                    {
                        selectedStones[i].isSelected = false;
                        selectedStones.Remove(selectedStones[i]);
                        i--;
                    }
                }
            }
        }

        private void ScrollViewer_MouseLeave(object sender, MouseEventArgs e)
        {
            ((DominoCanvas)((ScrollViewer)sender).Content).Children.Remove(((DominoCanvas)((ScrollViewer)sender).Content).rect);
            ((DominoCanvas)((ScrollViewer)sender).Content).rect = null;
        }

        private void ScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((DominoCanvas)((ScrollViewer)sender).Content).Children.Remove(((DominoCanvas)((ScrollViewer)sender).Content).rect);
            ((DominoCanvas)((ScrollViewer)sender).Content).rect = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (((ComboBox)((Grid)((Button)sender).Parent).Children[1]).SelectedIndex == -1) return;

            DominoCanvas tmpCanvas = (DominoCanvas)((ScrollViewer)((Grid)((Grid)((Grid)((Button)sender).Parent).Parent).Parent).Children[0]).Content;

            if (tmpCanvas.ProjectDoc.GetType() == typeof(FieldDocument))
            {
                for (int i = 0; i < selectedStones.Count; i++)
                {
                    if (((SolidColorBrush)selectedStones[i].Fill).Color == ((DominoColor)((ComboBox)((Grid)((Button)sender).Parent).Children[0]).SelectedValue).rgb && selectedStones[i].path == ((FieldDocument)tmpCanvas.ProjectDoc).path)
                    {
                        selectedStones[i].ColorValue = ((ComboBox)((Grid)((Button)sender).Parent).Children[1]).SelectedIndex;
                        ((FieldDocument)tmpCanvas.ProjectDoc).dominoes[selectedStones[i].stoneWidth, selectedStones[i].stoneHeight] = selectedStones[i].ColorValue;
                        selectedStones[i].isSelected = false;
                        selectedStones.Remove(selectedStones[i]);
                        i--;
                    }
                }
            }
            else if (tmpCanvas.ProjectDoc.GetType() == typeof(StructureDocument))
            {
                for (int i = 0; i < selectedStones.Count; i++)
                {
                    if (((SolidColorBrush)selectedStones[i].Fill).Color == ((DominoColor)((ComboBox)((Grid)((Button)sender).Parent).Children[0]).SelectedValue).rgb && selectedStones[i].path == ((Document)tmpCanvas.ProjectDoc).path)
                    {
                        selectedStones[i].ColorValue = ((ComboBox)((Grid)((Button)sender).Parent).Children[1]).SelectedIndex;
                        ((StructureDocument)tmpCanvas.ProjectDoc).dominoes[selectedStones[i].arrayCounter] = selectedStones[i].ColorValue;
                        selectedStones[i].isSelected = false;
                        selectedStones.Remove(selectedStones[i]);
                        i--;
                    }
                }
            }

            if (!new List<DominoColor>(tmpCanvas.ProjectDoc.UsedColors).Exists(x => x.rgb == ((DominoColor)((ComboBox)((Grid)((Button)sender).Parent).Children[1]).SelectedValue).rgb))
                tmpCanvas.ProjectDoc.UsedColors.Add((DominoColor)((ComboBox)((Grid)((Button)sender).Parent).Children[1]).SelectedValue);

            ((ComboBox)((Grid)((Button)sender).Parent).Children[0]).SelectedIndex = -1;
            ((ComboBox)((Grid)((Button)sender).Parent).Children[1]).SelectedIndex = -1;
            unselectAll();
            RefreshDepObj(sender, tmpCanvas);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == -1) return;
            bool isSelected = false;
            DominoCanvas tmpCanvas = (DominoCanvas)((ScrollViewer)((Grid)((Grid)((Grid)((ComboBox)sender).Parent).Parent).Parent).Children[0]).Content;
            int selectedValue = -1;

            for (int i = 0; i < tmpCanvas.ProjectDoc.Colors.Count; i++)
            {
                if (tmpCanvas.ProjectDoc.Colors[i].rgb == ((DominoColor)((ComboBox)sender).SelectedValue).rgb)
                    selectedValue = i;
            }
            if (selectedValue == -1)
                return;

            foreach (DominoUI tmpDomino in selectedStones)
            {
                if (tmpDomino.path == tmpCanvas.ProjectDoc.path)
                {
                    isSelected = true;
                    break;
                }
            }

            if (isSelected != true)
            {
                foreach (DominoUI actDomino in tmpCanvas.Children)
                {
                    if (actDomino.ColorValue == selectedValue)
                    {
                        if (actDomino.isSelected != true)
                        {
                            if (!selectedStones.Exists(x => x == actDomino))
                            {
                                selectedStones.Add(actDomino);
                                (actDomino).isSelected = true;
                            }
                        }
                    }
                }
            }
        }

        private void ComboBox_Preview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                foreach (DominoUI actDomino in selectedStones)
                {
                    if (actDomino.path.CompareTo(((DominoCanvas)((ScrollViewer)((Grid)((Grid)((Grid)((ComboBox)sender).Parent).Parent).Parent).Children[0]).Content).ProjectDoc.path) == 0)
                    {
                        actDomino.SelectionColorValue = ((ComboBox)sender).SelectedIndex;
                    }
                }
            }
        }

        private void DominoPlanner_Closing(object sender, CancelEventArgs e)
        {
            TabControlMain.SelectedIndex = 0;
            Document[] temp = OpenedDocuments.ToArray();
            for (int i = 0; i < temp.Length; i++)
            {
                if (!InternalCloseTab(temp[i])) { e.Cancel = true; return; }
            }
        }
        private bool InternalCloseTab(Document Data)
        {
            Document OldDocument = Document.FastLoad(Data.path);
            if (!Data.Compare(OldDocument)) // Document was changed
            {
                switch (MessageBox.Show("Save unsaved changes in file " + Data.filename + "?", "File has been changed", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning))
                {
                    case MessageBoxResult.Yes:
                        Data.Save(Data.path);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }
            OpenedDocuments.Remove(Data);
            TabControlMain.Items.Refresh();
            TabControlMain.SelectedIndex = 0;
            GC.Collect();
            return true;
        }

        private void ShowStructureProtocol(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DominoCanvas dc = (DominoCanvas)((ScrollViewer)((Grid)((Grid)((Grid)((ComboBox)sender).Parent).Parent).Parent).Children[0]).Content;
            dc.ProjectDoc.UsedColors.Clear();
            if (selectedStones.Count > 0)
            {
                foreach (DominoUI actDomino in selectedStones)
                {
                    if (actDomino.path == dc.ProjectDoc.path)
                    {
                        if (!new List<DominoColor>(dc.ProjectDoc.UsedColors).Exists(x => x.rgb == ((SolidColorBrush)actDomino.Fill).Color))
                            dc.ProjectDoc.UsedColors.Add(dc.ProjectDoc.Colors[actDomino.ColorValue]);
                    }
                }
            }

            if (dc.ProjectDoc.UsedColors.Count == 0)
            {
                if (dc.ProjectDoc.GetType() == typeof(FieldDocument))
                {
                    for (int i = 0; i < dc.ProjectDoc.height; i++)
                    {
                        for (int k = 0; k < dc.ProjectDoc.length; k++)
                        {
                            if (!new List<DominoColor>(dc.ProjectDoc.UsedColors).Exists(x => x.name == dc.ProjectDoc.Colors[((FieldDocument)dc.ProjectDoc).dominoes[k, i]].name))
                                dc.ProjectDoc.UsedColors.Add(dc.ProjectDoc.Colors[((FieldDocument)dc.ProjectDoc).dominoes[k, i]]);
                        }
                    }
                }
                else if (dc.ProjectDoc.GetType() == typeof(StructureDocument))
                {
                    for (int i = 0; i < ((StructureDocument)dc.ProjectDoc).dominoes.Length; i++)
                    {
                        if (!new List<DominoColor>(dc.ProjectDoc.UsedColors).Exists(x => x.name == dc.ProjectDoc.Colors[((StructureDocument)dc.ProjectDoc).dominoes[i]].name))
                            dc.ProjectDoc.UsedColors.Add(dc.ProjectDoc.Colors[((StructureDocument)dc.ProjectDoc).dominoes[i]]);
                    }
                }
            }


            ((ComboBox)sender).ItemsSource = dc.ProjectDoc.UsedColors; //ich rall hier echt nicht was ich falsch mache :/ nutze ObservableList und laut Internet sollte es gehen..
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            DominoCanvas dc = (DominoCanvas)((ScrollViewer)((Grid)((Grid)((Grid)((Grid)((Button)sender).Parent).Parent).Parent).Parent).Children[0]).Content;

            if (dc.ProjectDoc.GetType() == typeof(FieldDocument))
            {
                if (selectedStones.Count > 0)
                {
                    FieldDocument actOpen = (FieldDocument)dc.ProjectDoc;
                    RemoveRowCol rrc = new RemoveRowCol();
                    rrc.ShowDialog();
                    int[,] tmpArray;
                    int actCounter = 0;
                    List<int> values = new List<int>();
                    switch (rrc.removeRC)
                    {
                        case RemoveRC.Row:
                            foreach (DominoUI actDomino in selectedStones)
                            {
                                if (!values.Exists(x => x == actDomino.stoneHeight))
                                    values.Add(actDomino.stoneHeight);
                            }
                            tmpArray = new int[actOpen.length, actOpen.height - values.Count];
                            for (int i = 0; i < actOpen.height; i++)
                            {
                                if (!values.Contains(i))
                                {
                                    for (int k = 0; k < actOpen.length; k++)
                                        tmpArray[k, i - actCounter] = actOpen.dominoes[k, i];
                                }
                                else
                                {
                                    actCounter++;
                                }
                            }
                            ((FieldDocument)dc.ProjectDoc).dominoes = tmpArray;
                            ((FieldDocument)dc.ProjectDoc).ChangeSize(actOpen.length, actOpen.height - actCounter);
                            LoadCanvas(dc);
                            break;
                        case RemoveRC.Column:
                            foreach (DominoUI actDomino in selectedStones)
                            {
                                if (!values.Exists(x => x == actDomino.stoneWidth))
                                    values.Add(actDomino.stoneWidth);
                            }
                            tmpArray = new int[actOpen.length - values.Count, actOpen.height];
                            for (int i = 0; i < actOpen.length; i++)
                            {
                                if (!values.Contains(i))
                                {
                                    for (int k = 0; k < actOpen.height; k++)
                                        tmpArray[i - actCounter, k] = actOpen.dominoes[i, k];
                                }
                                else
                                {
                                    actCounter++;
                                }
                            }
                            ((FieldDocument)dc.ProjectDoc).dominoes = tmpArray;
                            ((FieldDocument)dc.ProjectDoc).ChangeSize(actOpen.length - actCounter, actOpen.height);
                            LoadCanvas(dc);
                            break;
                        case RemoveRC.none:
                            return;
                        default:
                            break;
                    }
                }
                else
                {
                    MessageBox.Show(this, "You have to select some stones.", "No selection");
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            DominoCanvas dc = (DominoCanvas)((ScrollViewer)((Grid)((Grid)((Grid)((Grid)((Button)sender).Parent).Parent).Parent).Parent).Children[0]).Content;

            if (dc.ProjectDoc.GetType() == typeof(FieldDocument))
            {
                if (selectedStones.Count == 1)
                {
                    FieldDocument actOpen = (FieldDocument)dc.ProjectDoc;
                    ChangeProjectSize cps = new ChangeProjectSize();
                    cps.ShowDialog();
                    int[,] tmpArray;
                    switch (cps.ResizePlace)
                    {
                        case global::DominoPlanner.ResizeMode.Top:
                            tmpArray = new int[actOpen.length, actOpen.height + cps.iCounter];
                            for (int i = 0; i < selectedStones[0].stoneHeight; i++)
                            {
                                for (int k = 0; k < actOpen.length; k++)
                                    tmpArray[k, i] = actOpen.dominoes[k, i];
                            }
                            for (int i = selectedStones[0].stoneHeight; i < actOpen.height; i++)
                            {
                                for (int k = 0; k < actOpen.length; k++)
                                    tmpArray[k, i + cps.iCounter] = actOpen.dominoes[k, i];
                            }
                            actOpen.dominoes = tmpArray;
                            actOpen.ChangeSize(actOpen.length, actOpen.height + cps.iCounter);
                            break;
                        case global::DominoPlanner.ResizeMode.Bottom:
                            tmpArray = new int[actOpen.length, actOpen.height + cps.iCounter];
                            for (int i = 0; i <= selectedStones[0].stoneHeight; i++)
                            {
                                for (int k = 0; k < actOpen.length; k++)
                                    tmpArray[k, i] = actOpen.dominoes[k, i];
                            }
                            for (int i = selectedStones[0].stoneHeight + 1; i < actOpen.height; i++)
                            {
                                for (int k = 0; k < actOpen.length; k++)
                                    tmpArray[k, i + cps.iCounter] = actOpen.dominoes[k, i];
                            }
                            actOpen.dominoes = tmpArray;
                            actOpen.ChangeSize(actOpen.length, actOpen.height + cps.iCounter);
                            break;
                        case global::DominoPlanner.ResizeMode.Left:
                            tmpArray = new int[actOpen.length + cps.iCounter, actOpen.height];
                            for (int i = 0; i < selectedStones[0].stoneWidth; i++)
                            {
                                for (int k = 0; k < actOpen.height; k++)
                                    tmpArray[i, k] = actOpen.dominoes[i, k];
                            }
                            for (int i = selectedStones[0].stoneWidth; i < actOpen.length; i++)
                            {
                                for (int k = 0; k < actOpen.height; k++)
                                    tmpArray[i + cps.iCounter, k] = actOpen.dominoes[i, k];
                            }
                            actOpen.dominoes = tmpArray;
                            actOpen.ChangeSize(actOpen.length + cps.iCounter, actOpen.height);
                            break;
                        case global::DominoPlanner.ResizeMode.Right:
                            tmpArray = new int[actOpen.length + cps.iCounter, actOpen.height];
                            for (int i = 0; i <= selectedStones[0].stoneWidth; i++)
                            {
                                for (int k = 0; k < actOpen.height; k++)
                                    tmpArray[i, k] = actOpen.dominoes[i, k];
                            }
                            for (int i = selectedStones[0].stoneWidth; i < actOpen.length; i++)
                            {
                                for (int k = 0; k < actOpen.height; k++)
                                    tmpArray[i + cps.iCounter, k] = actOpen.dominoes[i, k];
                            }
                            actOpen.dominoes = tmpArray;
                            actOpen.ChangeSize(actOpen.length + cps.iCounter, actOpen.height);
                            break;
                        case global::DominoPlanner.ResizeMode.none:
                            return;
                        default:
                            break;
                    }
                    ((FieldDocument)dc.ProjectDoc).dominoes = actOpen.dominoes;
                    ((FieldDocument)dc.ProjectDoc).ChangeSize(actOpen.length, actOpen.height);
                    LoadCanvas(dc);
                }
                else
                {
                    MessageBox.Show(this, "You have to select one stone!", "Wrong Selection");
                }
            }
            else if (dc.ProjectDoc.GetType() == typeof(StructureDocument))
            {
                MessageBox.Show(this, "This funtion is a Field-Function only!", "Fields Only");
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            clipboardStones = new List<DominoUI>();
            DominoCanvas dc = (DominoCanvas)((ScrollViewer)((Grid)((Grid)((Grid)((Button)sender).Parent).Parent).Parent).Children[0]).Content;
            if (dc.ProjectDoc.GetType() == typeof(StructureDocument))
            {
                MessageBox.Show(this, "This funtion is a Field-Function only!", "Fields Only");
                return;
            }
            for (int i = 0; i < selectedStones.Count; i++)
            {
                if (selectedStones[i].path == dc.ProjectDoc.path)
                {
                    clipboardStones.Add(selectedStones[i]);
                    selectedStones[i].isSelected = false;
                    selectedStones.Remove(selectedStones[i]);
                    i--;
                }
            }
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            DominoUI firstStone = new DominoUI();
            DominoCanvas dc = (DominoCanvas)((ScrollViewer)((Grid)((Grid)((Grid)((Button)sender).Parent).Parent).Parent).Children[0]).Content;
            if (dc.ProjectDoc.GetType() == typeof(StructureDocument))
            {
                MessageBox.Show(this, "This funtion is a Field-Function only!", "Fields Only");
                return;
            }
            foreach (DominoUI tmpStone in selectedStones)
            {
                if (tmpStone.path == dc.ProjectDoc.path)
                {
                    firstStone = tmpStone;
                    counter++;
                }
                if (counter == 2)
                {
                    MessageBox.Show(this, "You have to select only one Stone!", "Select to much");
                    return;
                }
            }
            if (selectedStones.Count == 0)
            {
                MessageBox.Show(this, "You have to select one Stone!", "Select to much");
            }
            else
            {
                if (dc.ProjectDoc.GetType() == typeof(FieldDocument))
                {
                    int heightDiff = firstStone.stoneHeight - clipboardStones[0].stoneHeight;
                    int widthDiff = firstStone.stoneWidth - clipboardStones[0].stoneWidth;

                    foreach (DominoUI tmpStone in clipboardStones)
                    {
                        if (tmpStone.stoneWidth + widthDiff > 0 && tmpStone.stoneWidth + widthDiff < dc.ProjectDoc.length && tmpStone.stoneHeight + heightDiff > 0 && tmpStone.stoneHeight + heightDiff < dc.ProjectDoc.height)
                        {
                            ((FieldDocument)dc.ProjectDoc).dominoes[tmpStone.stoneWidth + widthDiff, tmpStone.stoneHeight + heightDiff] = tmpStone.ColorValue;
                        }
                    }
                }
                else if (dc.ProjectDoc.GetType() == typeof(StructureDocument))
                {
                    int difference = firstStone.stoneHeight - clipboardStones[0].stoneHeight;

                    foreach (DominoUI tmpStone in clipboardStones)
                    {
                        if (tmpStone.stoneHeight + difference > 0 && tmpStone.stoneHeight + difference < dc.ProjectDoc.height)
                        {
                            ((StructureDocument)dc.ProjectDoc).dominoes[tmpStone.stoneHeight + difference] = tmpStone.ColorValue;
                        }
                    }
                }

                LoadCanvas(dc);
                clipboardStones.Clear();
                selectedStones.Clear();
            }
        }

        private void AboutBox(object sender, RoutedEventArgs e)
        {
            AboutWindow b = new AboutWindow();
            b.ShowDialog();
        }
    }
    #endregion

    #region Converters and TemplateSelectors

    public class IsTooMuchConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value[0] != DependencyProperty.UnsetValue)
            {
                Color c = (int.Parse(value[0].ToString()) > int.Parse(value[1].ToString())) ? Color.FromArgb(255, 255, 0, 0) : Color.FromArgb(255, 0, 0, 0);
                return new SolidColorBrush(c);
            }
            return new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RadioButtonCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            bool param = bool.Parse(parameter.ToString());
            return !((bool)value ^ param);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    public class TypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (Path.GetExtension(value as String))
            {
                case ".clr":
                    return "/Icons/ColorLine.ico";

                case ".fld":
                    String BackPath = "";
                    String SourcePath = Path.Combine(Path.GetDirectoryName(value as String), "Source Images");
                    foreach (String ImagePath in Directory.GetFiles(SourcePath))
                    {
                        if (Path.GetFileNameWithoutExtension(ImagePath) == Path.GetFileNameWithoutExtension((value as String)))
                            BackPath = ImagePath;
                    }
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(BackPath);
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    return bi;

                case ".mst":
                    return "/Icons/home.ico";

                case ".sct":
                    String BackPath2 = "";
                    String SourcePath2 = Path.Combine(Path.GetDirectoryName(value as String), "Source Images");
                    foreach (String ImagePath in Directory.GetFiles(SourcePath2))
                    {
                        if (Path.GetFileNameWithoutExtension(ImagePath) == Path.GetFileNameWithoutExtension((value as String)))
                            BackPath2 = ImagePath;
                    }
                    BitmapImage bi2 = new BitmapImage();
                    bi2.BeginInit();
                    bi2.UriSource = new Uri(BackPath2);
                    bi2.CacheOption = BitmapCacheOption.OnLoad;
                    bi2.EndInit();
                    return bi2;

                default:
                    return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RegressionModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((int)value)
            {
                case 0: return "Nearest Neighbor";
                case 1: return "Bicubic";
                default: return "High Quality Bicubic";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorComparisonToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String s = "Color Approximation Mode: \n";
            switch ((int)value)
            {
                case 0: s += "CIE-76 Comparison (ISO 12647)"; break;
                case 1: s += "CMC (I:c) Comparison"; break;
                case 2: s += "CIE-94 Comparison (DIN 99)"; break;
                case 3: s += "CIE-\u2206E-2000 Comparison"; break;
                default: break;
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Do the conversion from visibility to bool
            throw new NotImplementedException();
        }
    }

    public class DiffusionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String s = "Diffusion Mode: \n";
            switch ((int)value)
            {
                case 0: s += "No Diffusion"; break;
                case 1: s += "Floyd/Steinberg Dithering"; break;
                case 2: s += "Jarvis/Judice/Ninke Dithering"; break;
                case 3: s += "Stucki Dithering"; break;
                default: break;
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Do the conversion from visibility to bool
            throw new NotImplementedException();
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Media.Color)
            {
                return new SolidColorBrush((Color)value);
            }
            else
            {
                System.Drawing.Color c = (System.Drawing.Color)value;
                return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PathToHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as String;
            string result = "";
            result += Path.GetFileName(s);
            if (Path.GetExtension(s) != ".mst")
            {
                string[] folders = Path.GetDirectoryName(s).Split('\\');
                result += " (" + folders[folders.Length - 1] + ")";
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ColorToHTMLConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Media.Color)
            {
                Color c = (Color)value;
                return c.ToString();
            }
            System.Drawing.Color c2 = (System.Drawing.Color)value;
            return System.Drawing.ColorTranslator.ToHtml(c2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FilterToParametersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Filter f = value as Filter;
            return f.PropertiesToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TabTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return null;

            switch (((Document)item).t)
            {
                case type.clr: return ColorArrayTemplate;
                case type.mst: return MasterplanTemplate;
                case type.fld:
                case type.sct:
                    var ds = item as ProjectDocument;
                    if (ds == null)
                    {
                        return base.SelectTemplate(item, container);
                    }
                    PropertyChangedEventHandler lambda = null;
                    lambda = (o, args) =>
                    {
                        if (args.PropertyName == "Locked")
                        {
                            ds.PropertyChanged -= lambda;
                            var cp = (ContentPresenter)container;
                            cp.ContentTemplateSelector = null;
                            cp.ContentTemplateSelector = this;
                        }
                    };
                    ds.PropertyChanged += lambda;
                    if ((item as ProjectDocument).Locked == Visibility.Visible)
                    {
                        return ProjectEditorTemplate;
                    }
                    if (item is FieldDocument)
                    {
                        return FieldPlanTemplate;
                    }
                    else return StructureTemplate;

                case type.flp: return FieldProtocolTemplate;
                case type.img: return ImageEditorTemplate;
                default: return ColorArrayTemplate;
            }
        }

        public DataTemplate MasterplanTemplate { get; set; }

        public DataTemplate ColorArrayTemplate { get; set; }

        public DataTemplate FieldPlanTemplate { get; set; }

        public DataTemplate StructureTemplate { get; set; }

        public DataTemplate FieldProtocolTemplate { get; set; }

        public DataTemplate ImageEditorTemplate { get; set; }

        public DataTemplate ProjectEditorTemplate { get; set; }
    }


    public class StructureTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SpiralStructureTemplate { get; set; }
        public DataTemplate RectangleStructureTemplate { get; set; }
        public DataTemplate CircleStructureTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ClusterStructureProvider)
                return RectangleStructureTemplate;
            else if (item is SpiralProvider)
                return SpiralStructureTemplate;
            else if (item is CircleProvider)
                return CircleStructureTemplate;
            return null;
        }
    }
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
    #endregion Converters and TemplateSelectors
}