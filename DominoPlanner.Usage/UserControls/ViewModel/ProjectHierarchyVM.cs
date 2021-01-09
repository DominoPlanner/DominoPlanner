using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;
using System.Windows;
using Avalonia.Controls;
using DominoPlanner.Usage.Serializer;
using Avalonia.Media;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public abstract class NodeVM : ModelBase
    {
        
        public ObservableCollection<DominoWrapperNodeVM> Children { get; set; }

        private bool brokenReference;

        public bool BrokenReference
        {
            get { return brokenReference; }
            set { brokenReference = value; RaisePropertyChanged(); }
        }


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

        public AssemblyNodeVM Parent { get; set; }

        public static Action<TabItem> openTab;
        public static Func<NodeVM, Task<bool>> closeTab;
        public static Func<NodeVM, TabItem> getTab;
        
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
                    if (ContextMenuEntries != null)
                    {
                        _ContextMenu = new ContextMenu
                        {
                            Items = ContextMenuEntries
                        };
                    }
                }
                return _ContextMenu;
            }
        }

        public string Name
        {
            get =>
   Path.GetFileNameWithoutExtension(RelativePathFromParent);
        }


        public NodeVM()
        {
            MouseClickCommand = new RelayCommand((o) => OpenInternal());
        }
        private ICommand _MouseClickCommand;
        public ICommand MouseClickCommand
        {
            get => _MouseClickCommand;
            set { _MouseClickCommand = value; RaisePropertyChanged(); }
        }
        public List<MenuItem> BuildContextMenu()
        {
            if (BrokenReference)
                return null;
            return this.GetType().GetMethods()
                      .Select(m => Tuple.Create(m, m.GetCustomAttributes(typeof(ContextMenuAttribute), false)))
                      .Where(tuple => tuple.Item2.Count() > 0)
                      .OrderBy(tuple => (tuple.Item2.First() as ContextMenuAttribute).Index)
                      .Select(tuple => ContextMenuEntry.GenerateMenuItem(tuple.Item2.First() as ContextMenuAttribute, tuple.Item1, this))
                      .ToList();
        }
        public static NodeVM NodeVMFactory(IDominoWrapper node, AssemblyNodeVM parent)
        {
            switch (node)
            {
                case AssemblyNode assy:
                    return new AssemblyNodeVM(assy, parent);
                case DocumentNode dn:
                    return new DocumentNodeVM(dn);
                default:
                    break;
            }
            return null;
        }
        [ContextMenuAttribute("Open Folder", "Icons/folder_tar.ico", Index = 10)]
        public void OpenFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{ AbsolutePath}\"");
        }
        public async void OpenInternal()
        {
            if (brokenReference)
            {
                await new ReferenceManager().ShowDialog(MainWindowViewModel.GetWindow());
                Parent?.AssemblyModel.Save();
                PostReferenceRestoration();
            }
            Open();
        }

        internal virtual void PostReferenceRestoration()
        {
            _ContextMenu = null;
            RaisePropertyChanged("ContextMenu");
            RaisePropertyChanged("ImagePath");
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
        public ColorNodeVM ColorNode { get; set; }
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
            get
            {
                try
                {
                    var result = AssemblyModel.Path;
                    if (BrokenReference)
                    {
                        if (File.Exists(_AbsolutePath))
                            BrokenReference = false;
                    }
                    return result;
                }
                catch (FileNotFoundException)
                {
                    BrokenReference = true;
                    return "";
                }
            }
            set
            {
                AssemblyModel.Path = value;
                // Reload all children
                LoadChildren();
            }
        }
        public AssemblyNodeVM(AssemblyNode assembly, AssemblyNodeVM parent)
        {
            AssemblyModel = assembly;
            AssemblyModel.RelativePathChanged += (s, args) => RelativePathChanged();
            ColorNode = new ColorNodeVM(this);
            LoadChildren();
        }

        public AssemblyNodeVM(AssemblyNode assembly, Action<TabItem> openTab,
            Func<NodeVM, Task<bool>> closeTab, Func<NodeVM, TabItem> getTab) : this(assembly, null, openTab, closeTab, getTab) { }

        public AssemblyNodeVM(AssemblyNode assembly, AssemblyNodeVM parent, Action<TabItem> openTab,
            Func<NodeVM, Task<bool>> closeTab, Func<NodeVM, TabItem> getTab)
        {
            AssemblyModel = assembly;
            AssemblyModel.RelativePathChanged += (s, args) => RelativePathChanged();
            NodeVM.openTab = openTab;
            NodeVM.closeTab = closeTab;
            NodeVM.getTab = getTab;
            ColorNode = new ColorNodeVM(this);
            LoadChildren();
        }
        public void Initialize()
        {
            ColorNode = new ColorNodeVM(this);
            LoadChildren();
        }
        public async void LoadChildren()
        {
            try
            {
                Children = new ObservableCollection<DominoWrapperNodeVM>();
                Children.CollectionChanged -= ChildrenAddDelegates;
                Children.CollectionChanged += ChildrenAddDelegates;

                for (int i = 0; i < AssemblyModel.Obj.children.Count; i++)
                {
                    var node = AssemblyModel.Obj.children[i];
                    try
                    {
                        if (node is IDominoWrapper idw && (idw is AssemblyNode || idw is DocumentNode))
                        {
                            var vm = NodeVMFactory(idw, this);
                            vm.Parent = this;
                            Children.Add(vm as DominoWrapperNodeVM);
                        }
                    }
                    catch (Exception ex) when (ex is InvalidDataException || ex is ProtoBuf.ProtoException)
                    {
                        // broken Subassembly
                        var restored = await RestoreAssembly((node as AssemblyNode).AbsolutePath, ColorNode.AbsolutePath);
                        {
                            restored.parent = AssemblyModel.Obj;
                            // make color path relative
                            restored.Path = Workspace.MakeRelativePath(AbsolutePath, restored.Path);
                            AssemblyModel.Obj.children[i] = restored;
                            Children.Add(NodeVMFactory(restored, this) as AssemblyNodeVM);
                            AssemblyModel.Save();
                        }

                    }
                    catch (FileNotFoundException)
                    {
                        // missing subassembly
                        /*string path = "";
                        if (node is AssemblyNode asn) path = asn.Path;
                        Errorhandler.RaiseMessage($"The project file {path} does not exist at the current location. " +
                            $"It will be removed from the project.", "Error", Errorhandler.MessageType.Error);*/
                        // Remove file from Assembly and decrease counter
                        /*AssemblyModel.obj.children.RemoveAt(i--);
                        AssemblyModel.Save();*/
                    }
                }
                AssemblyModel.Obj.children.CollectionChanged += AssemblyModelChildren_CollectionChanged;
                this.BrokenReference = false;
            }
            catch (FileNotFoundException)
            {
                this.BrokenReference = true;
            }
        }
        internal override void PostReferenceRestoration()
        {
            this.LoadChildren();
            RaisePropertyChanged("Children");
            this.AssemblyModel.Save();
            base.PostReferenceRestoration();
        }
        private void ChildrenAddDelegates(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (NodeVM i in e.NewItems)
                {
                    if (i is DocumentNodeVM dn)
                    {
                        dn.DocumentModel.RelativePathChanged += (s, args) => dn.RelativePathChanged();
                    }
                    else if (i is AssemblyNodeVM an)
                    {
                        an.AssemblyModel.RelativePathChanged += (s, args) => an.RelativePathChanged();
                    }
                }

            }
        }
        private void AssemblyModelChildren_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (IDominoWrapper i in e.OldItems)
                {
                    if (i is AssemblyNode a)
                    {
                        Children.RemoveAll(x => x is AssemblyNodeVM vm && vm.AssemblyModel == a);
                    }
                    if (i is DocumentNode d)
                    {
                        Children.RemoveAll(x => x is DocumentNodeVM vm && vm.DocumentModel == d);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is AssemblyNode || item is DocumentNode)
                    {
                        var newNode = NodeVMFactory(item as IDominoWrapper, this);
                        newNode.Parent = this;
                        Children.Add(newNode as DominoWrapperNodeVM);
                    }
                }
            }
            AssemblyModel.Save();
        }
        public void RemoveChild(DominoWrapperNodeVM node)
        {
            AssemblyModel.Obj.children.Remove(node.Model);
        }
        [ContextMenuAttribute("Open color list", "Icons/colorLine.ico", index: 0)]
        public void OpenColorList()
        {
            try
            {
                ColorNode.Open();
                BrokenReference = false;
            }
            catch (FileNotFoundException)
            {
                BrokenReference = true;
            }
        }
        public override void Open()
        {
            // will be implemented when doing Masterplan
        }
        [ContextMenuAttribute("Add new object", "Icons/add.ico", index: 1)]
        public async void NewFieldStructure()
        {
            NewObjectVM novm = new NewObjectVM(Path.GetDirectoryName(AbsolutePath), AssemblyModel.Obj);
            await new NewObject(novm).ShowDialog(MainWindowViewModel.GetWindow()) ;
            if (!novm.Close || novm.ResultNode == null) return;
            Children.Where(x => x.Model == novm.ResultNode).FirstOrDefault()?.Open();
        }
        [ContextMenuAttribute("Add existing object", "Icons/add.ico", index: 2)]
        public async void AddExistingItem()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filters.Add(
                new FileDialogFilter() { Extensions = new List<string> {Properties.Settings.Default.ProjectExtension, Properties.Settings.Default.ObjectExtension }, Name = "All DominoPlanner files" });
            openFileDialog.Filters.Add(
               new FileDialogFilter() { Extensions = new List<string> { Properties.Settings.Default.ObjectExtension }, Name = "Object files" });
            openFileDialog.Filters.Add(
                new FileDialogFilter() { Extensions = new List<string> {Properties.Settings.Default.ProjectExtension }, Name = "Project files" });
            var result = await openFileDialog.ShowAsync(MainWindowViewModel.GetWindow());
            if (result != null && result.Length == 1 && File.Exists(result[0]))
            {
                string extension = Path.GetExtension(result[0]).ToLower();
                if (extension == "." + Properties.Settings.Default.ObjectExtension.ToLower())
                {
                    try
                    {
                        IDominoWrapper node = IDominoWrapper.CreateNodeFromPath(AssemblyModel.Obj, result[0]);
                        AssemblyModel.Save();
                        Children.Where(x => x.Model == node).FirstOrDefault()?.Open();
                    }
                    catch (FileNotFoundException)
                    {
                        await Errorhandler.RaiseMessage("Error loading file", "Error", Errorhandler.MessageType.Error);
                    }
                }
                else if (extension == "." +Properties.Settings.Default.ProjectExtension.ToLower())
                {
                    try
                    {
                        string relativePath = Workspace.MakeRelativePath(AbsolutePath, result[0]);
                        var assy = Workspace.Load<DominoAssembly>(result[0]);
                        if (assy == AssemblyModel.Obj || relativePath == "" || assy.ContainsReferenceTo(AssemblyModel.Obj))
                        {
                            await Errorhandler.RaiseMessage("This operation would create a circular dependency between assemblies. This is not supported.", "Circular Reference", Errorhandler.MessageType.Error);
                            return;
                        }
                        IDominoWrapper node = new AssemblyNode(relativePath, AssemblyModel.Obj);
                        AssemblyModel.Save();
                    }
                    catch { }
                }

            }
        }
        [ContextMenuAttribute("Rename", "Icons/draw_freehand.ico", index: 3)]
        public async void Rename()
        {
            RenameObject ro = new RenameObject(Path.GetFileName(AbsolutePath));
            if (await ro.ShowDialog<bool>(MainWindowViewModel.GetWindow()) == true)
            {
                Workspace.CloseFile(AbsolutePath);
                var new_path = Path.Combine(Path.GetDirectoryName(AbsolutePath), ((RenameObjectVM)ro.DataContext).NewName);
                File.Move(AbsolutePath, new_path);
                if (Parent == null)
                {
                    OpenProjectSerializer.RenameProject(AbsolutePath, new_path);
                    AssemblyModel.Path = new_path;

                }
                else
                {
                    AssemblyModel.Path = Workspace.MakeRelativePath(Parent.AbsolutePath, new_path);
                }
            }
        }
        [ContextMenuAttribute("Remove", "Icons/remove.ico", index: 5)]
        public void Remove()
        {
            if (Parent != null)
            {
                this.Parent.RemoveChild(this);
            }
            else
            {
                MainWindowViewModel._Projects.Remove(this);
                int index = OpenProjectSerializer.GetProjectID(AbsolutePath);
                if (index >= 0) OpenProjectSerializer.RemoveOpenProject(index);
            }
        }
        [ContextMenuAttribute("Export all", "Icons/image.ico", index: 4)]
        public async Task ExportImagesAsync()
        {
            ExportOptions exp = new ExportOptions();
            bool collapsed = false;
            bool drawBorders = false;
            Color background = Colors.Transparent;
            if (await exp.ShowDialog<bool>(MainWindowViewModel.GetWindow()))
            {
                ExportOptionVM dc = exp.DataContext as ExportOptionVM;
                collapsed = dc.Collapsed;
                drawBorders = dc.DrawBorders;
                background = dc.BackgroundColor;
            }
            else
            {
                return;
            }

            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            openFolderDialog.InitialDirectory = PathRoot;
            string exportDirectory = await openFolderDialog.ShowAsync(MainWindowViewModel.GetWindow());;
            ExportImages(exportDirectory, collapsed, drawBorders, background);
        }

        public void ExportImages(string exportDirectory, bool collapsed, bool drawBorders, Color background)
        {
            if (!Directory.Exists(exportDirectory))
            {
                Directory.CreateDirectory(exportDirectory);
            }
            foreach (DominoWrapperNodeVM child in Children)
            {
                if (child is DocumentNodeVM documentNodeVM)
                {
                    documentNodeVM.ExportImage(Path.Combine(exportDirectory, $"{documentNodeVM.Name}.png"), collapsed, drawBorders, background);
                }
                else if(child is AssemblyNodeVM assemblyNodeVM)
                {
                    assemblyNodeVM.ExportImages(Path.Combine(exportDirectory, assemblyNodeVM.Name), collapsed, drawBorders, background);
                }
            }
        }

        [ContextMenuAttribute("Properties", "Icons/properties.ico", index: 20)]
        public void ShowProperties()
        {
            PropertiesWindow pw = new PropertiesWindow(Model);
            pw.ShowDialog(MainWindowViewModel.GetWindow());
        }
        public static async Task<AssemblyNode> RestoreAssembly(string projectpath, string colorlistPath = null)
        {
            string colorpath = Path.Combine(Path.GetDirectoryName(projectpath), "Planner Files");
            var colorres = Directory.EnumerateFiles(colorpath, $"*.{Properties.Settings.Default.ColorExtension}");
            // restore project if colorfile exists
            if (colorlistPath == null && colorres.First() == null)
            {
                throw new InvalidDataException("Color file not found");
            }
            colorlistPath ??= colorres.First();
            Workspace.CloseFile(projectpath);
            if (File.Exists(projectpath))
                File.Copy(projectpath, Path.Combine(Path.GetDirectoryName(projectpath), $"backup_{DateTime.Now.ToLongTimeString().Replace(":", "_")}.{Properties.Settings.Default.ProjectExtension}"));
            DominoAssembly newMainNode = new DominoAssembly();
            newMainNode.Save(projectpath);
            newMainNode.ColorPath = Workspace.MakeRelativePath(projectpath, colorlistPath);
            foreach (string path in Directory.EnumerateDirectories(colorpath))
            {
                try
                {
                    var assembly = Directory.EnumerateFiles(path, $"*.{Properties.Settings.Default.ProjectExtension}").
                        Where(x => Path.GetFileName(x).Contains("backup_")).FirstOrDefault();
                    if (string.IsNullOrEmpty(assembly)) continue;
                    AssemblyNode an = new AssemblyNode(Workspace.MakeRelativePath(projectpath, assembly), newMainNode);
                    newMainNode.children.Add(an);
                }
                catch { } // if error on add assembly, don't add assembly
            }
            foreach (string path in Directory.EnumerateFiles(colorpath, "*." + Properties.Settings.Default.ObjectExtension))
            {
                try
                {
                    var node = (DocumentNode)IDominoWrapper.CreateNodeFromPath(newMainNode, path);
                }
                catch { } // if error on add of file, don't add file 
                
            }
            newMainNode.Save();
            Workspace.CloseFile(projectpath);
            await Errorhandler.RaiseMessage($"The project file {projectpath} was damaged. An attempt has been made to restore the file.", "Damaged File", Errorhandler.MessageType.Info);

            return new AssemblyNode(projectpath);
        }
        public override string AbsolutePath
        {
            get
            {
                try
                {
                    _AbsolutePath = AssemblyModel.AbsolutePath;
                    if (BrokenReference)
                    {
                        if (File.Exists(_AbsolutePath))
                            BrokenReference = false;
                    }
                    return _AbsolutePath;
                }
                catch (FileNotFoundException)
                {
                    BrokenReference = true;
                    return "";
                }
            }
            set
            {
                if (Parent == null)
                {
                    AssemblyModel.Path = value;
                }
            }
        }
    }
    public class ColorNodeVM : NodeVM
    {
        public override string RelativePathFromParent { get => Parent.AssemblyModel.Obj.ColorPath; set => throw new NotImplementedException(); }

        public ColorNodeVM(AssemblyNodeVM assembly)
        {
            Parent = assembly;
        }
        public override void Open()
        {
            TabItem tabItem = NodeVM.getTab(this);
            if (tabItem != null && tabItem.Content is ColorListControlVM c) 
                c.DominoAssembly = Parent.AssemblyModel;
            NodeVM.openTab(tabItem ?? new TabItem(this));
        }
        public override string AbsolutePath {
            get => Workspace.AbsolutePathFromReferenceLoseUpdate(RelativePathFromParent, Parent.AssemblyModel.Obj);
            set => throw new NotImplementedException(); }
    }
    public class DocumentNodeVM : DominoWrapperNodeVM
    {
        public override string RelativePathFromParent
        {
            get => DocumentModel.RelativePath;
            set
            {
                DocumentModel.RelativePath = value;
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
                try
                {
                    _AbsolutePath = DocumentModel.AbsolutePath;
                    BrokenReference = false;
                    return _AbsolutePath;
                }
                catch (FileNotFoundException)
                {
                    BrokenReference = true;
                    return "";
                }
            }
            set { }
        }
        public DocumentNodeVM(DocumentNode dn)
        {
            DocumentModel = dn;
        }
        public bool HasFieldProtocol()
        {
            try
            {
                var result = Workspace.LoadHasProtocolDefinition<IWorkspaceLoadColorList>(AbsolutePath);
                BrokenReference = false;
                return result;
            }
            catch (FileNotFoundException)
            {
                BrokenReference = true;
                return false;
            }
        }
        [ContextMenuAttribute("Export as Image", "Icons/image.ico", index: 4 )]
        public void ExportImage()
        {
            ExportImage(false);
        }
        [ContextMenuAttribute("Custom Image Export", "Icons/image.ico", index: 5)]
        public void ExportImageCustom()
        {
            ExportImage(true);
        }

        public async void ExportImage(string exportPath, bool collapsed, bool drawBorders, Color background, int width = 0)
        {
            if (string.IsNullOrWhiteSpace(exportPath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();

                saveFileDialog.Filters.Add(new FileDialogFilter() { Extensions = new List<string> { "png" }, Name = "PNG files" });
                exportPath = await saveFileDialog.ShowAsync(MainWindowViewModel.GetWindow());
            }
            if (!string.IsNullOrWhiteSpace(exportPath))
            {
                if (File.Exists(exportPath))
                {
                    File.Delete(exportPath);
                }
                DocumentModel.Obj.Generate(new System.Threading.CancellationToken()).GenerateImage(background, width, drawBorders, collapsed).Save(exportPath);
            }
        }

        public async void ExportImage(bool userDefinedExport)
        {
            try
            {
                int width = 0;
                bool collapsed = false;
                bool drawBorders = false;

                drawBorders = true;

                Color background = Colors.Transparent;
                if (userDefinedExport)
                {
                    ExportOptions exp = new ExportOptions(DocumentModel.Obj);
                    if (await exp.ShowDialog<bool>(MainWindowViewModel.GetWindow()))
                    {
                        var dc = exp.DataContext as ProjectExportOptionsVM;
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
                ExportImage(string.Empty, collapsed, drawBorders, background, width);
            }
            catch (Exception ex) { await Errorhandler.RaiseMessage("Export failed" + ex, "Error", Errorhandler.MessageType.Error); }
        }
        [ContextMenuAttribute("Show protocol", "Icons/file_export.ico", "HasFieldProtocol", true, 6)]
        public async void ShowProtocol()
        {
            if (!DocumentModel.Obj.HasProtocolDefinition)
            {
                await Errorhandler.RaiseMessage("Could not generate a protocol. This structure type has no protocol definition.", "No Protocol", Errorhandler.MessageType.Warning);
                return;
            }
            ProtocolV protocolV = new ProtocolV();
            DocumentModel.Obj.Generate(new System.Threading.CancellationToken());
            protocolV.DataContext = new ProtocolVM(DocumentModel.Obj, Path.GetFileNameWithoutExtension(DocumentModel.RelativePath));
            protocolV.Show();
        }
        [ContextMenuAttribute("Open", "Icons/folder_tar.ico", Index = 0)]
        public override async void Open()
        {
            try
            {
                openTab(getTab(this) ?? new TabItem(this));
            }
            catch (FileNotFoundException)
            {
                BrokenReference = true;
            }
            catch (InvalidDataException)
            {
                RemoveNodeFromProject();
                await Errorhandler.RaiseMessage($"The file {this.Name} is broken. " +
                    $"It has been removed from the project.", "File not readable", Errorhandler.MessageType.Error);
            }
            
        }
        [ContextMenuAttribute("Remove from project", "Icons/remove.ico", Index = 7)]
        public async void RemoveNodeFromProject()
        {
            try
            {
                var msgbox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Delete?", $"Remove reference to file {Name} from project {Parent.Name}?\n" +
                    $"The file won't be permanently deleted.", MessageBox.Avalonia.Enums.ButtonEnum.YesNo, MessageBox.Avalonia.Enums.Icon.Warning);
                if (await closeTab(this) && await msgbox.ShowDialog(MainWindowViewModel.GetWindow())  == MessageBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    Parent.RemoveChild(this);
                    await Errorhandler.RaiseMessage($"{Name} has been removed!", "Removed", Errorhandler.MessageType.Error);
                }
            }
            catch (Exception)
            {
                await Errorhandler.RaiseMessage("Could not remove the object!", "Error", Errorhandler.MessageType.Error);
            }
            
        }
        [ContextMenuAttribute("Rename", "Icons/draw_freehand.ico", index: 3)]
        public async void Rename()
        {
            try
            {
                RenameObject ro = new RenameObject(Path.GetFileName(AbsolutePath));
                if (await closeTab(this) && await ro.ShowDialog<bool>(MainWindowViewModel.GetWindow()))
                {
                    Workspace.CloseFile(DocumentModel.AbsolutePath);
                    string old_path = AbsolutePath;
                    File.Move(old_path, Path.Combine(Path.GetDirectoryName(old_path), ((RenameObjectVM)ro.DataContext).NewName));
                    RelativePathFromParent = Path.Combine(Path.GetDirectoryName(RelativePathFromParent), 
                        ((RenameObjectVM)ro.DataContext).NewName);
                    Parent.AssemblyModel.Save();
                    Open();
                }
            }
            catch
            {
                await Errorhandler.RaiseMessage("Renaming object failed!", "Error", Errorhandler.MessageType.Error);
            }
        }
        [ContextMenuAttribute("Properties", "Icons/properties.ico", index: 20)]
        public async void ShowProperties()
        {
            PropertiesWindow pw = new PropertiesWindow(Model);
            await pw.ShowDialog(MainWindowViewModel.GetWindow());
        }
    }
}
