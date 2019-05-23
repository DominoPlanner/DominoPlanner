﻿using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;
using DominoPlanner.Usage.HelperClass;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using DominoPlanner.Usage.Serializer;
using System.Windows.Threading;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public abstract class NodeVM : ModelBase
    {
        
        public ObservableCollection<NodeVM> Children { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsExpanded;
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                if (_IsExpanded != value)
                {
                    _IsExpanded = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string ImagePath
        {
            get
            {
                if (AbsolutePath == "")
                    return "";
                var result = ImageHelper.GetImageOfFile(AbsolutePath);
                RaisePropertyChanged("AbsolutePath");
                return result;
            }
        }

        public AssemblyNodeVM parent { get; set; }
        public ContextMenu contextMenu { get; set; }

        public Action<TabItem> openTab;
        public Func<NodeVM, bool> closeTab;
        public Func<NodeVM, TabItem> getTab;
        
        public abstract string RelativePathFromParent { get; set; }

        public virtual bool CheckPath()
        {

            try
            {
                return AbsolutePath != "";
            }
            catch (IOException)
            {
                return false;
            }
        }
        public string PathRoot
        {
            get => Path.GetDirectoryName(AbsolutePath);
        }
        internal string _AbsolutePath;
        public abstract string AbsolutePath { get; set; }

        private ContextMenu _ContextMenu;

        public ContextMenu ContextMenu
        {
            get
            {
                if (_ContextMenu == null)
                {
                    var ContextMenuEntries = BuildContextMenu();

                    _ContextMenu = new ContextMenu();
                    foreach (var entry in ContextMenuEntries)
                    {
                        ContextMenu.Items.Add(entry);
                    }
                }
                return _ContextMenu;
            }
        }

        public string Name { get => Path.GetFileNameWithoutExtension(RelativePathFromParent); }

        public NodeVM()
        {
            MouseClickCommand = new RelayCommand((o) => Open());
        }
        private ICommand _MouseClickCommand;
        public ICommand MouseClickCommand
        {
            get => _MouseClickCommand;
            set { _MouseClickCommand = value; RaisePropertyChanged(); }
        }
        public List<MenuItem> BuildContextMenu()
        {
            return this.GetType().GetMethods()
                      .Select(m => Tuple.Create(m, m.GetCustomAttributes(typeof(ContextMenuAttribute), false)))
                      .Where(tuple => tuple.Item2.Count() > 0)
                      .OrderBy(tuple => (tuple.Item2.First() as ContextMenuAttribute).Index)
                      .Select(tuple => (MenuItem) new ContextMenuEntry(tuple.Item2.First() as ContextMenuAttribute, tuple.Item1, this))
                      .ToList();
        }
        public static NodeVM NodeVMFactory(IDominoWrapper node, AssemblyNodeVM parent)
        {
            switch (node)
            {
                case AssemblyNode assy:
                    return new AssemblyNodeVM(assy, parent.openTab, parent.closeTab, parent.getTab);
                case DocumentNode dn:
                    return new DocumentNodeVM(dn);
            }
            return null;
        }
        [ContextMenuAttribute("Open Folder", "Icons/folder_tar.ico", Index = 10)]
        public void OpenFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{ AbsolutePath}\"");
        }
        public abstract void Open();
        
    }
    public abstract class DominoWrapperNodeVM : NodeVM
    {
        private IDominoWrapper dominoWrapper;

        public IDominoWrapper Model
        {
            get { return dominoWrapper; }
            set { dominoWrapper = value; RaisePropertyChanged(); }
        }
        internal void RelativePathChanged()
        {
            _AbsolutePath = null;
            RaisePropertyChanged("AbsolutePath");
            RaisePropertyChanged("Name");
            Model.parent?.Save();
        }

    }

    public class AssemblyNodeVM : DominoWrapperNodeVM
    {
        public ColorNodeVM colorNode { get; set; }
        public AssemblyNode AssemblyModel
        {
            get => Model as AssemblyNode;
            set
            {
                Model = value;
                RaisePropertyChanged();
            }
        }

        public override string RelativePathFromParent
        {
            get => AssemblyModel.Path;
            set
            {
                AssemblyModel.Path = value;
                // Reload all children
                LoadChildren();
            }
        }
        public AssemblyNodeVM(AssemblyNode assembly, AssemblyNodeVM parent) :
            this(assembly, parent, parent.openTab, parent.closeTab, parent.getTab) { }

        public AssemblyNodeVM(AssemblyNode assembly, Action<TabItem> openTab,
            Func<NodeVM, bool> closeTab, Func<NodeVM, TabItem> getTab) : this(assembly, null, openTab, closeTab, getTab) { }

        public AssemblyNodeVM(AssemblyNode assembly, AssemblyNodeVM parent, Action<TabItem> openTab,
            Func<NodeVM, bool> closeTab, Func<NodeVM, TabItem> getTab)
        {
            AssemblyModel = assembly;
            AssemblyModel.RelativePathChanged = RelativePathChanged;
            this.openTab = openTab;
            this.closeTab = closeTab;
            this.getTab = getTab;
            colorNode = new ColorNodeVM(this);
            LoadChildren();
        }
        public void LoadChildren()
        {
            Children = new ObservableCollection<NodeVM>();
            Children.CollectionChanged -= ChildrenAddDelegates;
            Children.CollectionChanged += ChildrenAddDelegates;
            Children.CollectionChanged -= Children_CollectionChanged;
            Children.CollectionChanged += Children_CollectionChanged;
            foreach (var node in AssemblyModel.obj.children)
            {
                var vm = NodeVMFactory(node, this);
                vm.parent = this;
                Children.Add(vm);

            }
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (NodeVM i in e.OldItems)
                {
                    if (i is AssemblyNodeVM a)
                    {
                        a.AssemblyModel.parent.children.Remove(a.AssemblyModel);
                        Workspace.CloseFile(a.AssemblyModel.AbsolutePath);
                    }
                    if (i is DocumentNodeVM d)
                    {
                        d.DocumentModel.parent.children.Remove(d.DocumentModel);
                        Workspace.CloseFile(d.DocumentModel.AbsolutePath);
                    }
                }
                AssemblyModel.Save();
            }
        }

        private void ChildrenAddDelegates(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (NodeVM i in e.NewItems)
                {
                    if (i is DocumentNodeVM dn)
                    {
                        dn.getTab = this.getTab;
                        dn.openTab = this.openTab;
                        dn.closeTab = this.closeTab;
                        dn.DocumentModel.RelativePathChanged = dn.RelativePathChanged;
                    }
                    else if (i is AssemblyNodeVM an)
                    {
                        an.AssemblyModel.RelativePathChanged = an.RelativePathChanged;
                    }
                    if (!i.CheckPath())
                    {
                        if (i is DominoWrapperNodeVM)
                        {
                            Children.Remove(i);
                            Errorhandler.RaiseMessage($"The file {RelativePathFromParent} doesn't exist at the current location. \nIt has been removed from the project.", "Missing file", Errorhandler.MessageType.Error);
                        }
                    }
                }

            }
        }
        public void RemoveChild(NodeVM node)
        {
            if (!(node is DominoWrapperNodeVM))
            {
                throw new InvalidOperationException("You can't remove a colorNode");
            }
            Children.Remove(node);
        }
        [ContextMenuAttribute("Open color list", "Icons/colorLine.ico", index: 0)]
        public void OpenColorList()
        {
            colorNode.Open();
        }
        public override void Open()
        {
            // will be implemented when doing Masterplan
        }
        [ContextMenuAttribute("Add new object", "Icons/add.ico", index: 1)]
        public void NewFieldStructure()
        {
            NewObjectVM novm = new NewObjectVM(Path.GetDirectoryName(AbsolutePath), AssemblyModel.obj);
            new NewObject(novm).ShowDialog();
            if (!novm.Close || novm.ResultNode == null) return;
            var newNode = NodeVMFactory(novm.ResultNode, this);
            Children.Add(newNode);
            newNode.Open();
        }
        [ContextMenuAttribute("Add existing object", "Icons/add.ico", index: 2)]
        public void AddExistingItem()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "project files (*.DObject)|*.DObject";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    try
                    {
                        IDominoWrapper node = IDominoWrapper.CreateNodeFromPath(AssemblyModel.obj, openFileDialog.FileName);
                        var newNodeVM = NodeVMFactory(node, this);
                        Children.Add(newNodeVM);
                        AssemblyModel.Save();
                        newNodeVM.Open();
                    }
                    catch (FileNotFoundException)
                    {
                        // Unable to load project
                    }
                }
            }
        }
        [ContextMenuAttribute("Rename", "Icons/draw_freehand.ico", index: 3)]
        public void Rename()
        {
            var dn = (AssemblyNode)Model;
            RenameObject ro = new RenameObject(Path.GetFileName(AbsolutePath));
            if (ro.ShowDialog() == true)
            {
                Workspace.CloseFile(AbsolutePath);
                var new_path = Path.Combine(Path.GetDirectoryName(AbsolutePath), ((RenameObjectVM)ro.DataContext).NewName);
                File.Move(AbsolutePath, new_path);
                if (parent == null)
                {
                    OpenProjectSerializer.RenameProject(AbsolutePath, new_path);
                    AssemblyModel.Path = new_path;
                    
                }
                else
                {
                    AssemblyModel.Path = Workspace.MakeRelativePath(parent.AbsolutePath, new_path);
                }
            }
        }
        [ContextMenuAttribute("Remove", "Icons/remove.ico", index: 4)]
        public void Remove()
        {
            if (parent != null)
            {
                this.parent.RemoveChild(this);
            }
            else
            {
                MainWindowViewModel._Projects.Remove(this);
                int index = OpenProjectSerializer.GetProjectID(AbsolutePath);
                if (index >= 0) OpenProjectSerializer.RemoveOpenProject(index);
            }
        }
        public static AssemblyNode RestoreAssembly(string projectpath, string colorlistPath = null)
        {
            AssemblyNode mainnode = null;
            string colorpath = Path.Combine(Path.GetDirectoryName(projectpath), "Planner Files");
            var colorres = Directory.EnumerateFiles(colorpath, "*.DColor");
            // restore project if colorfile exists
            if (colorlistPath == null && colorres.First() == null)
            {
                throw new InvalidDataException("Color file not found");
            }
            colorlistPath = colorlistPath ?? colorres.First();
            Workspace.CloseFile(projectpath);
            if (File.Exists(projectpath))
                File.Copy(projectpath, Path.Combine(Path.GetDirectoryName(projectpath), $"backup_{DateTime.Now.ToLongTimeString().Replace(":", "_")}.DProject"));
            DominoAssembly newMainNode = new DominoAssembly();
            newMainNode.Save(projectpath);
            newMainNode.colorPath = Workspace.MakeRelativePath(projectpath, colorlistPath);
            foreach (string path in Directory.EnumerateFiles(colorpath, "*.DObject"))
            {
                try
                {
                    var node = (DocumentNode)IDominoWrapper.CreateNodeFromPath(newMainNode, path);
                }
                catch { } // if error on add of file, don't add file 
            }
            foreach (string path in Directory.EnumerateFiles(colorpath, "*.DProject"))
            {
                var assembly = (AssemblyNodeVM.RestoreAssembly(path, colorlistPath));
                mainnode.obj.children.Add(assembly);
            }
            newMainNode.Save();
            Workspace.CloseFile(projectpath);
            return mainnode = new AssemblyNode(projectpath);
        }
        public override string AbsolutePath
        {
            get
            {
                _AbsolutePath = AssemblyModel.AbsolutePath;
                return _AbsolutePath;
            }
            set
            {
                if (parent == null)
                {
                    AssemblyModel.Path = value;
                }
            }
        }
    }
    public class ColorNodeVM : NodeVM
    {
        public override string RelativePathFromParent { get => parent.AssemblyModel.obj.colorPath; set => throw new NotImplementedException(); }

        public ColorNodeVM(AssemblyNodeVM assembly)
        {
            parent = assembly;
        }
        public override void Open()
        {
            TabItem tabItem = parent.getTab(this);
            if (tabItem != null && tabItem.Content is ColorListControlVM c) 
                c.DominoAssembly = parent.AssemblyModel.obj;
            parent.openTab(tabItem ?? new TabItem(this));
        }
        public override string AbsolutePath {
            get => Workspace.AbsolutePathFromReferenceLoseUpdate(RelativePathFromParent, parent.AssemblyModel.obj);
            set => throw new NotImplementedException(); }
    }
    public class DocumentNodeVM : DominoWrapperNodeVM
    {
        public override string RelativePathFromParent
        {
            get => DocumentModel.relativePath;
            set
            {
                DocumentModel.relativePath = value;
                _AbsolutePath = null;
            }
        }
        public DocumentNode DocumentModel
        {
            get => Model as DocumentNode;
            set
            {
                Model = value;
                RaisePropertyChanged();
            }
        }
        public override string AbsolutePath
        {
            get
            {
                _AbsolutePath = DocumentModel.AbsolutePath;
                return _AbsolutePath;
            }
            set { }
        }
        public DocumentNodeVM(DocumentNode dn)
        {
            DocumentModel = dn;
        }
        public bool HasFieldProtocol()
        {
            return Workspace.LoadHasProtocolDefinition<IWorkspaceLoadColorList>(AbsolutePath);
        }
        [ContextMenuAttribute("Export as Image", "Icons/image.ico", index: 4 )]
        public void ExportImage()
        {
            ExportImage(true);
        }
        [ContextMenuAttribute("Custom Image Export", "Icons/image.ico", index: 5)]
        public void ExportImageCustom()
        {
            ExportImage(false);
        }
        public void ExportImage(bool userDefinedExport)
        {
            try
            {
                int width = 2000;
                bool collapsed = false;
                bool drawBorders = false;
                Color background = Colors.Transparent;
                if (userDefinedExport)
                {
                    ExportOptions exp = new ExportOptions(DocumentModel.obj);
                    if (exp.ShowDialog() == true)
                    {
                        var dc = exp.DataContext as ExportOptionsVM;
                        width = dc.ImageSize;
                        collapsed = dc.Collapsed;
                        drawBorders = dc.DrawBorders;
                        background = dc.BackgroundColor;
                    }
                    else
                    {
                        return;
                    }
                }
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "png files (*.png)|*.png";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (File.Exists(saveFileDialog.FileName))
                    {
                        File.Delete(saveFileDialog.FileName);
                    }
                    DocumentModel.obj.Generate(new System.Threading.CancellationToken()).GenerateImage(background, width, drawBorders, collapsed).Save(saveFileDialog.FileName);

                }
            }
            catch (Exception ex) { Errorhandler.RaiseMessage("Export failed" + ex, "Error", Errorhandler.MessageType.Error); }
        }
        [ContextMenuAttribute("Show protocol", "Icons/file_export.ico", "HasFieldProtocol", Visibility.Visible, 6)]
        public void ShowProtocol()
        {
            if (!DocumentModel.obj.HasProtocolDefinition)
            {
                Errorhandler.RaiseMessage("Could not generate a protocol. This structure type has no protocol definition.", "No Protocol", Errorhandler.MessageType.Warning);
                return;
            }
            ProtocolV protocolV = new ProtocolV();
            DocumentModel.obj.Generate(new System.Threading.CancellationToken());
            protocolV.DataContext = new ProtocolVM(DocumentModel.obj, Path.GetFileNameWithoutExtension(DocumentModel.relativePath));
            protocolV.ShowDialog();
        }
        [ContextMenuAttribute("Open", "Icons/folder_tar.ico", Index = 0)]
        public override void Open()
        {
            try
            {
                openTab(getTab(this) ?? new TabItem(this));
            }
            catch (FileNotFoundException)
            {
                RemoveNodeFromProject();
            }
            catch (InvalidDataException)
            {
                RemoveNodeFromProject();
                Errorhandler.RaiseMessage($"The file {this.Name} is broken. " +
                    $"It has been removed from the project.", "File not readable", Errorhandler.MessageType.Error);
            }
            
        }
        [ContextMenuAttribute("Remove from project", "Icons/remove.ico", Index = 7)]
        public void RemoveNodeFromProject()
        {
            try
            {
                if (closeTab(this) &&
                    MessageBox.Show($"Remove reference to file {Name} from project {parent.Name}?\n" +
                    $"The file won't be permanently deleted.", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    parent.RemoveChild(this);
                    Errorhandler.RaiseMessage($"{Name} has been removed!", "Removed", Errorhandler.MessageType.Error);
                }
            }
            catch (Exception)
            {
                Errorhandler.RaiseMessage("Could not remove the object!", "Error", Errorhandler.MessageType.Error);
            }
            
        }
        [ContextMenuAttribute("Rename", "Icons/draw_freehand.ico", index: 3)]
        public void Rename()
        {
            try
            {
                RenameObject ro = new RenameObject(Path.GetFileName(AbsolutePath));
                if (closeTab(this) && ro.ShowDialog() == true)
                {
                    Workspace.CloseFile(DocumentModel.AbsolutePath);
                    string old_path = AbsolutePath;
                    File.Move(old_path, Path.Combine(Path.GetDirectoryName(old_path), ((RenameObjectVM)ro.DataContext).NewName));
                    RelativePathFromParent = Path.Combine(Path.GetDirectoryName(RelativePathFromParent), 
                        ((RenameObjectVM)ro.DataContext).NewName);
                    parent.AssemblyModel.Save();
                    Open();
                }
            }
            catch
            {
                Errorhandler.RaiseMessage("Renaming object failed!", "Error", Errorhandler.MessageType.Error);
            }
        }
        
    }
}