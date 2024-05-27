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
using System.Diagnostics;
using static DominoPlanner.Usage.Localizer;
using MessageBox.Avalonia.Enums;

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
                RaisePropertyChanged(nameof(AbsolutePath));
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
            _("Open Folder");
            Process.Start(new ProcessStartInfo(Path.GetDirectoryName(AbsolutePath)) { UseShellExecute = true });

        }
        public async void OpenInternal()
        {
            if (brokenReference)
            {
                await new ReferenceManager().ShowDialogWithParent<MainWindow>();
                Parent?.AssemblyModel.Save();
                PostReferenceRestoration();
            }
            Open();
        }

        internal virtual void PostReferenceRestoration()
        {
            _ContextMenu = null;
            RaisePropertyChanged(nameof(ContextMenu));
            RaisePropertyChanged(nameof(ImagePath));
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
            RaisePropertyChanged(nameof(AbsolutePath));
            RaisePropertyChanged(nameof(Name));
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
            catch (InvalidDataException)
            {
                this.BrokenFile = true;
            }
        }
        internal override void PostReferenceRestoration()
        {
            this.LoadChildren();
            RaisePropertyChanged(nameof(Children));
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
            _("Open color list");
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
            _("Add new object");
            NewObjectVM novm = new NewObjectVM(Path.GetDirectoryName(AbsolutePath), AssemblyModel.Obj);
            await new NewObject(novm).ShowDialogWithParent<MainWindow>() ;
            if (!novm.Close || novm.ResultNode == null) return;
            Children.Where(x => x.Model == novm.ResultNode).FirstOrDefault()?.Open();
        }
        [ContextMenuAttribute("Add existing object", "Icons/add.ico", index: 2)]
        public async void AddExistingItem()
        {
            _("Add existing object");
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filters = new List<FileDialogFilter>() {
                    new FileDialogFilter() { Extensions = new List<string> { Declares.ProjectExtension, Declares.ObjectExtension }, Name = _("All DominoPlanner files") },
                    new FileDialogFilter() { Extensions = new List<string> { Declares.ObjectExtension }, Name = _("Object files") },
                    new FileDialogFilter() { Extensions = new List<string> { Declares.ProjectExtension }, Name = _("Project files") }
                },
                Directory = this.GetInitialDirectory()
            };
            var result = await openFileDialog.ShowAsyncWithParent<MainWindow>();
            if (result != null && result.Length == 1 && File.Exists(result[0]))
            {
                string extension = Path.GetExtension(result[0]).ToLower();
                if (extension == "." + Declares.ObjectExtension.ToLower())
                {
                    try
                    {
                        IDominoWrapper node = IDominoWrapper.CreateNodeFromPath(AssemblyModel.Obj, result[0]);
                        AssemblyModel.Save();
                        Children.Where(x => x.Model == node).FirstOrDefault()?.Open();
                    }
                    catch (FileNotFoundException)
                    {
                        await Errorhandler.RaiseMessage(_("Error loading file"), _("Error"), Errorhandler.MessageType.Error);
                    }
                }
                else if (extension == "." +Declares.ProjectExtension.ToLower())
                {
                    try
                    {
                        string relativePath = Workspace.MakeRelativePath(AbsolutePath, result[0]);
                        var assy = Workspace.Load<DominoAssembly>(result[0]);
                        if (assy == AssemblyModel.Obj || relativePath == "" || assy.ContainsReferenceTo(AssemblyModel.Obj))
                        {
                            await Errorhandler.RaiseMessage(_("This operation would create a circular dependency between assemblies. This is not supported."), _("Circular Reference"), Errorhandler.MessageType.Error);
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
            _("Rename");
            RenameObject ro = new RenameObject(Path.GetFileName(AbsolutePath));
            if (await ro.GetDialogResultWithParent<MainWindow, bool>())
            {
                var temp_path = AbsolutePath;
                var new_path = Path.Combine(Path.GetDirectoryName(temp_path), ((RenameObjectVM)ro.DataContext).NewName);
                File.Move(temp_path, new_path);
                if (Parent == null)
                    OpenProjectSerializer.RenameProject(temp_path, new_path);
                AssemblyModel.Path = new_path;
            }
        }
        [ContextMenuAttribute("Remove", "Icons/remove.ico", index: 5)]
        public void Remove()
        {
            _("Remove");
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
            _("Export all");
            ExportOptions exp = new ExportOptions();
            bool collapsed = false;
            bool drawBorders = false;
            Color background = Colors.Transparent;
            if (await exp.GetDialogResultWithParent<MainWindow, bool>())
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

            OpenFolderDialog openFolderDialog = new OpenFolderDialog
            {
                Directory = this.GetInitialDirectory()
            };
            string exportDirectory = await openFolderDialog.ShowAsyncWithParent<MainWindow>();;
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
        public async void ShowProperties()
        {
            _("Properties");
            PropertiesWindow pw = new PropertiesWindow(Model);
            await pw.ShowDialogWithParent<MainWindow>();
        }
        public static async Task<AssemblyNode> RestoreAssembly(string projectpath, string colorlistPath = null)
        {
            string colorpath = Path.Combine(Path.GetDirectoryName(projectpath), "Planner Files");
            var colorres = Directory.EnumerateFiles(colorpath, $"*.{Declares.ColorExtension}");
            // restore project if colorfile exists
            if (colorlistPath == null && colorres.First() == null)
            {
                throw new InvalidDataException(_("Color file not found"));
            }
            colorlistPath ??= colorres.First();
            Workspace.CloseFile(projectpath);
            if (File.Exists(projectpath))
                File.Copy(projectpath, Path.Combine(Path.GetDirectoryName(projectpath), $"backup_{DateTime.Now.ToLongTimeString().Replace(":", "_")}.{Declares.ProjectExtension}"));
            DominoAssembly newMainNode = new DominoAssembly();
            newMainNode.Save(projectpath);
            newMainNode.ColorPath = Workspace.MakeRelativePath(projectpath, colorlistPath);
            foreach (string path in Directory.EnumerateDirectories(colorpath))
            {
                try
                {
                    var assembly = Directory.EnumerateFiles(path, $"*.{Declares.ProjectExtension}").
                        Where(x => Path.GetFileName(x).Contains("backup_")).FirstOrDefault();
                    if (string.IsNullOrEmpty(assembly)) continue;
                    AssemblyNode an = new AssemblyNode(Workspace.MakeRelativePath(projectpath, assembly), newMainNode);
                    newMainNode.children.Add(an);
                }
                catch { } // if error on add assembly, don't add assembly
            }
            foreach (string path in Directory.EnumerateFiles(colorpath, "*." + Declares.ObjectExtension))
            {
                try
                {
                    var node = (DocumentNode)IDominoWrapper.CreateNodeFromPath(newMainNode, path);
                }
                catch { } // if error on add of file, don't add file 
                
            }
            newMainNode.Save();
            Workspace.CloseFile(projectpath);
            await Errorhandler.RaiseMessage(string.Format(_("The project file {0} was damaged. An attempt has been made to restore the file."), projectpath), _("Damaged File"), Errorhandler.MessageType.Info);

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

        public bool BrokenFile { get; private set; }
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
            _("Export as Image");
            ExportImage(false);
        }
        [ContextMenuAttribute("Custom Image Export", "Icons/image.ico", index: 5)]
        public void ExportImageCustom()
        {
            _("Custom Image Export");
            //ExportImage(true);

            List<string> svgData = new List<string>();
            System.Globalization.CultureInfo usCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            svgData.Add($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{(DocumentModel.Obj.Last.PhysicalExpandedLength).ToString(usCulture)}mm\" height=\"{((DocumentModel.Obj.Last.PhysicalExpandedHeight)).ToString(usCulture)}mm\">");

            foreach (RectangleDomino rectangleDomino in DocumentModel.Obj.Last.shapes.Where(x => x.Color != 0))
            {
                Color currentColor = DocumentModel.Obj.Last.colors[rectangleDomino.Color].mediaColor;
                svgData.Add($"<rect x=\"{(rectangleDomino.x).ToString(usCulture)}mm\" y=\"{(rectangleDomino.y).ToString(usCulture)}mm\" width=\"{(rectangleDomino.ExpandedWidth).ToString(usCulture)}mm\" height=\"{(rectangleDomino.ExpandedHeight).ToString(usCulture)}mm\" fill=\"rgb({currentColor.R},{currentColor.G},{currentColor.B})\" stroke-width=\"0.01mm\" stroke=\"rgb(0,0,0)\" />");
            }

            svgData.Add("</svg>");

            string filePath = @"C:\Users\johan\Downloads\test.svg";
            File.Create(filePath).Dispose();
            File.WriteAllLines(filePath, svgData.ToArray());
                
        }
        [ContextMenu("Export Floor Print", "Icons/image.ico", isVisible: nameof(CanExportFloorPrint), index: 6)]
        public void ExportFloorPrint()
        {
            _("Export Floor Print");
            _ExportFloorPrint();
        }
        
        public bool CanExportFloorPrint()
        {
            return DocumentModel is CircleNode || DocumentModel is SpiralNode;
        }

        private async void _ExportFloorPrint()
        {
            try
            {
                if (DocumentModel?.Obj?.Last != null && DocumentModel.Obj.Last.PhysicalExpandedHeight > 1700)
                {
                    var box = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("To large", "The project ist to large.", ButtonEnum.Ok, Icon.Error);
                    await box.ShowDialogWithParent<MainWindow>();
                    return;
                }
                ExportFloorPlan();
            }
            catch (Exception ex) { await Errorhandler.RaiseMessage(_("Export failed: ") + ex, _("Error"), Errorhandler.MessageType.Error); }
        }

        public async void ExportImage(string exportPath, bool collapsed, bool drawBorders, Color background, int width = 0)
        {
            string finalExportPath = await ExportImage_PrepairPath(exportPath);
            DocumentModel.Obj.Generate(new System.Threading.CancellationToken()).GenerateImage(background, width, drawBorders, collapsed).Save(finalExportPath);
        }

        private async Task<string> ExportImage_PrepairPath()
        {
            return await ExportImage_PrepairPath(string.Empty);
        }

        private async Task<string> ExportImage_PrepairPath(string checkPath)
        {
            Task<string> createPath = Task.Run(async () =>
            {
                string exportPath = checkPath;
                if (string.IsNullOrEmpty(exportPath))
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog()
                    {
                        Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Extensions = new List<string> { "png" }, Name = _("PNG files") } },
                        Directory = this.GetInitialDirectory(),
                        InitialFileName = $"{Name}.png",
                    };
                    exportPath = await saveFileDialog.ShowAsyncWithParent<MainWindow>();
                }

                if (!string.IsNullOrWhiteSpace(exportPath))
                {
                    if (File.Exists(exportPath))
                    {
                        File.Delete(exportPath);
                    }
                }
                return exportPath;
            });

            await createPath;
            return createPath.Result;
        }

        public async void ExportFloorPlan()
        {
            string exportPath = await ExportImage_PrepairPath();
            DocumentModel.Obj.Generate(new System.Threading.CancellationToken()).GenerateFloorPlan().Save(exportPath, 300);
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
                    if (await exp.GetDialogResultWithParent<MainWindow, bool>())
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
            catch (Exception ex) { await Errorhandler.RaiseMessage(_("Export failed: ") + ex, _("Error"), Errorhandler.MessageType.Error); }
        }
        [ContextMenuAttribute("Show protocol", "Icons/file_export.ico", nameof(HasFieldProtocol), true, 6)]
        public async void ShowProtocol()
        {
            _("Show protocol");
            if (!DocumentModel.Obj.HasProtocolDefinition)
            {
                await Errorhandler.RaiseMessage(_("Could not generate a protocol. This structure type has no protocol definition."), _("No Protocol definition"), Errorhandler.MessageType.Warning);
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
            _("Open");
            try
            {
                openTab(getTab(this) ?? new TabItem(this));
            }
            catch (FileNotFoundException)
            {
                BrokenReference = true;
            }
            catch (InvalidDataException ex)
            {
                RemoveNodeFromProject();
                await Errorhandler.RaiseMessage(string.Format(_("The file {0} is broken. \nIt has been removed from the project."), this.Name) + "\n" + _("Additional information: ") + ex.Message, _("File not readable"), Errorhandler.MessageType.Error);
            }
            
        }
        [ContextMenuAttribute("Remove from project", "Icons/remove.ico", Index = 7)]
        public async void RemoveNodeFromProject()
        {
            _("Remove from project");
            try
            {
                var msgbox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(_("Delete?"), 
                string.Format(_("Remove reference to file {0} from project {1}?\nThe file won't be permanently deleted."), Name, Parent.Name), MessageBox.Avalonia.Enums.ButtonEnum.YesNo, MessageBox.Avalonia.Enums.Icon.Warning);
                if (await closeTab(this) && await msgbox.ShowDialogWithParent<MainWindow>()  == MessageBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    Parent.RemoveChild(this);
                    await Errorhandler.RaiseMessage(string.Format(_("{0} has been removed!"), Name), _("Removed"), Errorhandler.MessageType.Error);
                }
            }
            catch (Exception)
            {
                await Errorhandler.RaiseMessage(_("Could not remove the object!"), _("Error"), Errorhandler.MessageType.Error);
            }
            
        }
        [ContextMenuAttribute("Rename", "Icons/draw_freehand.ico", index: 3)]
        public async void Rename()
        {
            _("Rename");
            try
            {
                RenameObject ro = new RenameObject(Path.GetFileName(AbsolutePath));
                if (await closeTab(this) && await ro.GetDialogResultWithParent<MainWindow, bool>())
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
                await Errorhandler.RaiseMessage(_("Renaming object failed!"), _("Error"), Errorhandler.MessageType.Error);
            }
        }
        [ContextMenuAttribute("Properties", "Icons/properties.ico", index: 20)]
        public async void ShowProperties()
        {
            _("Properties");
            PropertiesWindow pw = new PropertiesWindow(Model);
            await pw.ShowDialogWithParent<MainWindow>();
        }
    }
}
