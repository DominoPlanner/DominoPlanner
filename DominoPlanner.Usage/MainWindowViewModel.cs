using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using DominoPlanner.Usage.Serializer;
using DominoPlanner.Usage.UserControls.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage
{
    class MainWindowViewModel : ModelBase
    {
        #region CTOR
        public MainWindowViewModel()
        {
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.StructureTemplates = Properties.Settings.Default.Properties["StructureTemplates"].DefaultValue.ToString();
            if (Properties.Settings.Default.FirstStartup)
            {
                Properties.Settings.Default.StandardColorArray = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Appdata", "Local", "DominoPlanner", "colors.DColor");
                Properties.Settings.Default.StandardProjectPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Appdata", "Local", "DominoPlanner");
                Properties.Settings.Default.OpenProjectList = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Appdata", "Local", "DominoPlanner", "OpenProjects.xml");
                Directory.CreateDirectory(Path.GetDirectoryName(Properties.Settings.Default.StandardColorArray));
                OpenProjectSerializer.Create();
                Properties.Settings.Default.FirstStartup = false;
            }
            Properties.Settings.Default.Save();

            while (!File.Exists(Properties.Settings.Default.StandardColorArray))
            {
                Errorhandler.RaiseMessage("Please create a defaultcolortable.", "Missing Color Table", Errorhandler.MessageType.Info);
                new SetStandardV().ShowDialog();
            }

            NewFieldStruct = new RelayCommand(o => { NewFieldStructure(); });
            MenuSetStandard = new RelayCommand(o => { new SetStandardV().ShowDialog(); });
            AddExistingProject = new RelayCommand(o => { AddProject_Exists(); });
            AddExistingItem = new RelayCommand(o => { AddItem_Exists(); });
            NewProject = new RelayCommand(o => { CreateNewProject(); });
            SaveAll = new RelayCommand(o => { SaveAllOpen(); });
            SaveCurrentOpen = new RelayCommand(o => { SaveCurrentOpenProject(); });
            FileListClickCommand = new RelayCommand(o => { OpenItemFromOpenedFiles(o); });
            Tabs = new ObservableCollection<TabItem>();
            Workspace.del = UpdateReference;
            loadProjectList();
        }

        internal bool CloseAllTabs()
        {
            while (Tabs.Count > 0)
            {
                if (!RemoveItem(Tabs.First()))
                    return false;
            }
            return true;
        }
        private string UpdateReference(string absolutePath, string parentPath)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Path.GetDirectoryName(absolutePath);
            ofd.Title = $"Locate file {Path.GetFileName(absolutePath)}";
            Errorhandler.RaiseMessage($"The object {parentPath} contains a reference to the file {absolutePath}," +
                $"which could not be located. Please find the file.", "Missing file", Errorhandler.MessageType.Error);
            string extension = Path.GetExtension(absolutePath);
            ofd.Filter = $"{extension} files|*{extension}|all files|*.*";
            if (ofd.ShowDialog() == true && File.Exists(ofd.FileName))
            {
                return Workspace.MakeRelativePath(parentPath, ofd.FileName);
            }

            return "";
        }
        private void CurrentProject_EditingChanged(object sender, EventArgs args)
        {
            TabItem tabItem = Tabs.Where(x => x.Content.CurrentProject == sender).FirstOrDefault();
            //((IDominoProvider)tabItem.Content.CurrentProject).Generate();
            Stack<PostFilter> undoStack = new Stack<PostFilter>();
            Stack<PostFilter> redoStack = new Stack<PostFilter>();
            if (tabItem.Content is DominoProviderVM vm)
            {
                undoStack = vm.undoStack;
                redoStack = vm.redoStack;
            }
            else if (tabItem.Content is EditProjectVM ep)
            {
                undoStack = ep.undoStack;
                redoStack = ep.redoStack;
            }
            tabItem.Content.Save();

            tabItem.ResetContent();
            if (tabItem.Content is DominoProviderVM vm2)
            {
                vm2.undoStack = undoStack;
                vm2.redoStack = redoStack;
            }
            else if (tabItem.Content is EditProjectVM ep2)
            {
                ep2.redoStack = redoStack;
                ep2.undoStack = undoStack;
            }
        }
        #endregion

        #region prop
        public ObservableCollection<TabItem> Tabs { get; set; }
        private TabItem _SelectedTab;
        public TabItem SelectedTab
        {
            get { return _SelectedTab; }
            set
            {
                if (_SelectedTab != value)
                {
                    if (_SelectedTab != null)
                    {
                        if (_SelectedTab.Content is ColorListControlVM colorList)
                        {
                            //hässlich aber tut... :D
                            colorList.DifColumns.Clear();
                        }
                    }
                    _SelectedTab = value;
                    if (SelectedTab != null)
                        _SelectedTab.Content.ResetContent();
                    RaisePropertyChanged();
                }
            }
        }
        private ProjectComposite _SelectedProject;
        public ProjectComposite SelectedProject
        {
            get { return _SelectedProject; }
            set
            {
                if (_SelectedProject != value)
                {
                    _SelectedProject = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<ProjectComposite> _Projects;
        public ObservableCollection<ProjectComposite> Projects
        {
            get { return _Projects; }
            set
            {
                if (_Projects != value)
                {
                    _Projects = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Command
        private ICommand _AddExistingProject;
        public ICommand AddExistingProject { get { return _AddExistingProject; } set { if (value != _AddExistingProject) { _AddExistingProject = value; } } }

        private ICommand _AddExistingItem;
        public ICommand AddExistingItem { get { return _AddExistingItem; } set { if (value != _AddExistingItem) { _AddExistingItem = value; } } }

        private ICommand _NewProject;
        public ICommand NewProject { get { return _NewProject; } set { if (value != _NewProject) { _NewProject = value; } } }

        private ICommand _NewFieldStruct;
        public ICommand NewFieldStruct { get { return _NewFieldStruct; } set { if (value != _NewFieldStruct) { _NewFieldStruct = value; } } }

        private ICommand _MenuSetStandard;
        public ICommand MenuSetStandard { get { return _MenuSetStandard; } set { if (value != _MenuSetStandard) { _MenuSetStandard = value; } } }

        private ICommand _SaveAll;
        public ICommand SaveAll { get { return _SaveAll; } set { if (value != _SaveAll) { _SaveAll = value; } } }

        private ICommand _SaveCurrentOpen;
        public ICommand SaveCurrentOpen { get { return _SaveCurrentOpen; } set { if (value != _SaveCurrentOpen) { _SaveCurrentOpen = value; } } }

        private ICommand _FileListClickCommand;
        public ICommand FileListClickCommand { get { return _FileListClickCommand; } set { if (value != _FileListClickCommand) { _FileListClickCommand = value; } } }

        #endregion

        #region Methods
        #region Eventmethods
        /// <summary>
        /// "Create new Field/Structure"-Event über ein Objekt in der Baumstruktur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateMI_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ((ProjectComposite)((System.Windows.Controls.MenuItem)sender).DataContext).IsSelected = true;
            NewFieldStructure();
        }
        private void OpenMI_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenItem((ProjectComposite)((System.Windows.Controls.MenuItem)sender).DataContext);
        }
    
        /// <summary>
        /// Remove selected Project
        /// </summary>
        private void RemoveMI_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            List<TabItem> removeList = Tabs.Where(x => x.ProjectID == ((ProjectListComposite)((System.Windows.Controls.MenuItem)sender).DataContext).OwnID).ToList<TabItem>();
            for (int i = 0; i < removeList.Count; i++)
            {
                RemoveItem(removeList[0]);
            }

            if (OpenProjectSerializer.RemoveOpenProject(((ProjectListComposite)((System.Windows.Controls.MenuItem)sender).DataContext).OwnID))
            {
                Projects.Remove(((ProjectListComposite)((System.Windows.Controls.MenuItem)sender).DataContext));
            }
            else
            {
                Errorhandler.RaiseMessage("Could not remove the project!", "Error", Errorhandler.MessageType.Error);
            }
        }
        private void RenameMI_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var proj = (ProjectListComposite)((System.Windows.Controls.MenuItem)sender).DataContext;
            foreach (TabItem item in Tabs.Where(x => x.ProjectID == proj.OwnID).ToList())
                RemoveItem(item);
            var dn = (AssemblyNode)proj.Project.documentNode;
            RenameObject ro = new RenameObject(Path.GetFileName(proj.FilePath));
            if (ro.ShowDialog() == true)
            {
                Workspace.CloseFile(proj.FilePath);
                OpenProjectSerializer.RemoveOpenProject(proj.OwnID);
                dn.Path = Path.Combine(Path.GetDirectoryName(dn.Path), ((RenameObjectVM)ro.DataContext).NewName);
                proj.Name = Path.GetFileNameWithoutExtension(((RenameObjectVM)ro.DataContext).NewName);
                string old_path = proj.FilePath;
                proj.FilePath = Path.Combine(Path.GetDirectoryName(proj.FilePath), ((RenameObjectVM)ro.DataContext).NewName);
                File.Move(old_path, proj.FilePath);
                var projectcomposite = OpenProjectSerializer.AddOpenProject(Path.GetFileNameWithoutExtension(proj.FilePath), Path.GetDirectoryName(proj.FilePath));
                Projects.Remove(proj);
                loadProject(projectcomposite);
                Workspace.Load<DominoAssembly>(proj.FilePath);
            }


        }
        /// <summary>
        /// Clickevent wenn in der Baumstruktur ein Projektnode geklickt wird
        /// </summary>
        /// <param name="sender">Gecklicktes Node</param>
        /// <param name="e"></param>
        private void Item_IsClicked(object sender, EventArgs e)
        {
            OpenItem((ProjectComposite)sender);
        }
        private void OpenItemFromOpenedFiles(object param)
        {
            ProjectComposite comp = null;
            foreach (ProjectListComposite p in Projects)
            {
                foreach (ProjectComposite pp in p.Children)
                {
                    if (Path.GetFullPath(Path.Combine(Path.GetDirectoryName(p.FilePath), pp.FilePath))
                        == Path.GetFullPath(param.ToString())) comp = pp;
                }
            }
            if (comp != null)
            {
                OpenItem(comp);
            }
        }

        private void OpenItem(ProjectComposite toOpen)
        {
            TabItem selTab = null;
            if (toOpen.ActType == NodeType.ColorListNode)
            {
                ProjectListComposite parent = null;
                foreach (ProjectListComposite p in Projects.OfType<ProjectListComposite>())
                {
                    if (p.Children.Contains(toOpen))
                    {
                        parent = p;
                        break;
                    }
                }
                if (parent != null)
                {
                    selTab = Tabs.FirstOrDefault(x => x.Content is ColorListControlVM && ((ColorListControlVM)x.Content).DominoAssembly == ((AssemblyNode)parent.Project.documentNode).obj);
                    if (selTab == null)
                    {
                        selTab = new TabItem(toOpen.OwnID, toOpen.ParentProjectID, toOpen.Name, toOpen.PicturePath, toOpen.FilePath, new ColorListControlVM(((AssemblyNode)parent.Project.documentNode).obj));
                        Tabs.Add(selTab);
                    }
                }
            }
            else if (toOpen.ActType == NodeType.ProjectNode)
            {
                selTab = Tabs.FirstOrDefault(x => x.ProjectComp == toOpen);
                if (selTab == null)
                {
                    try
                    {
                        selTab = new TabItem(toOpen);
                        Tabs.Add(selTab);
                    }
                    catch (FileNotFoundException)
                    {
                        DocumentNode dn = (DocumentNode)toOpen.Project.documentNode;
                        dn.parent.children.Remove(dn);
                        Workspace.CloseFile(toOpen.FilePath);
                        ((ProjectListComposite)Projects.Where(x => x.OwnID == toOpen.ParentProjectID).First()).Children.Remove(toOpen);
                        Workspace.Save(dn.parent);
                    }
                }
            }
            if (selTab != null) // && !tryAgain
            {
                selTab.CloseIt = MainWindowViewModel_CloseIt;
                if (selTab.Content.CurrentProject != null)
                {
                    selTab.Content.CurrentProject.EditingChanged += CurrentProject_EditingChanged;
                }
                SelectedTab = selTab;
            } 
        }
        /// <summary>
        /// Selection Changed in der Baumstruktur (damit das akteuelle Item refreshed werden kann)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowViewModel_SelectedEvent(object sender, EventArgs e)
        {
            if (((ProjectComposite)sender).IsSelected)
                SelectedProject = (ProjectComposite)sender;
        }
        /// <summary>
        /// Aktuelles TabItem schließen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool MainWindowViewModel_CloseIt(TabItem tabItem)
        {
            return RemoveItem(tabItem);
        }
        private bool RemoveProjectComposite(ProjectComposite comp)
        {
            bool result = true;
            foreach (TabItem tabItem in Tabs.Where(x => x.ProjectComp == comp).ToArray())
            {
                result = result && RemoveItem(tabItem);
            }
            return result;
        }
        private bool RemoveItem(TabItem tabItem)
        {
            bool remove = false;
            if (tabItem.Content.UnsavedChanges)
            {
                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show($"Save unsaved changes of {tabItem.Header.TrimEnd('*')}?", "Warning", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Warning);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    tabItem.Content.Save();
                    remove = true;
                }
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    remove = true;
                }
            }
            else
            {
                remove = true;
            }
            if (remove) Tabs.Remove(tabItem);
            return remove;
        }

        #endregion
        /// <summary>
        /// Projektliste laden
        /// </summary>
        private void loadProjectList()
        {
            Projects = new ObservableCollection<ProjectComposite>();
            List<OpenProject> OpenProjects = OpenProjectSerializer.GetOpenProjects();
            if (OpenProjects != null)
            {
                foreach (OpenProject curOP in OpenProjects)
                {
                    loadProject(curOP);
                }
            }
            else
            {
                Errorhandler.RaiseMessage("Error loading opened projects!", "Error", Errorhandler.MessageType.Error);
                OpenProjectSerializer.Create();
            }
        }

        private void loadProject(OpenProject newProject)
        {
            bool remove = false;
            if (Directory.Exists(newProject.path))
            {
                string projectpath = Path.Combine(newProject.path, string.Format("{0}.DProject", newProject.name));

                AssemblyNode mainnode = new AssemblyNode(projectpath);
                //if (mainnode.obj.colorPath != null && File.Exists(Workspace.AbsolutePathFromReference(mainnode.obj.colorPath, mainnode.obj)))
                // {
                try
                {
                    // check if the file can be deserialized properly
                    DominoAssembly assembly = mainnode.obj;
                    string colorPath = mainnode.obj.colorPath;
                    bool colorpathExists = File.Exists(Workspace.AbsolutePathFromReference(ref colorPath, mainnode.obj));
                }
                catch (Exception)
                {
                    string colorpath = Path.Combine(newProject.path, "Planner Files");
                    var colorres = Directory.EnumerateFiles(colorpath, "*.DColor");
                    // restore project if colorfile exists
                    if (colorres.First() != null)
                    {
                        try
                        {
                            Workspace.CloseFile(projectpath);
                            if (File.Exists(projectpath))
                                File.Copy(projectpath, Path.Combine(Path.GetDirectoryName(projectpath), $"backup_{DateTime.Now.ToLongTimeString().Replace(":", "_")}.DProject"));
                            DominoAssembly newMainNode = new DominoAssembly();
                            newMainNode.Save(projectpath);
                            newMainNode.colorPath = Workspace.MakeRelativePath(projectpath, colorres.First());
                            foreach (string path in Directory.EnumerateFiles(Path.Combine(newProject.path, "Planner Files"), "*.DObject"))
                            {
                                try
                                {
                                    var node = (DocumentNode)IDominoWrapper.CreateNodeFromPath(newMainNode, path);
                                }
                                catch
                                { // if error on add of file, don't add file 
                                }
                            }
                            newMainNode.Save();
                            Workspace.CloseFile(projectpath);
                            mainnode = new AssemblyNode(projectpath);
                            Errorhandler.RaiseMessage($"The main project file of project {projectpath} was damaged. An attempt has been made to restore the file.", "Damaged File", Errorhandler.MessageType.Info);
                        }
                        catch
                        {
                            Errorhandler.RaiseMessage($"The main project file of project {projectpath} was damaged. An attempt to restore the file has been unsuccessful. \nThe project will be removed from the list of opened projects.", "Damaged File", Errorhandler.MessageType.Error);
                            remove = true;
                        }
                    }
                    else
                    {
                        remove = true;
                    }
                }
                if (!remove)
                {
                    ProjectListComposite actPLC = new ProjectListComposite(newProject.id, newProject.name, newProject.path, new ProjectElement(mainnode.Path, "", mainnode));
                    actPLC.SelectedEvent += MainWindowViewModel_SelectedEvent;
                    actPLC.conMenu.createMI.Click += CreateMI_Click;
                    actPLC.conMenu.removeMI.Click += RemoveMI_Click;
                    actPLC.conMenu.renameMI.Click += RenameMI_Click;
                    actPLC.closeTabDelegate = RemoveProjectComposite;
                    actPLC.openTabDelegate = OpenItem;
                    Projects.Add(actPLC);

                    foreach (ProjectElement currPT in getProjects(mainnode.obj))
                    {
                        AddProjectToTree(actPLC, currPT);
                    }
                }
            }
            else
            {
                remove = true;
            }
            if (remove)
            {
                Errorhandler.RaiseMessage($"Unable to load project {newProject.name}. It might have been moved. \nPlease re-add it at its current location.\n\nThe project has been removed from the list of opened projects.", "Error!", Errorhandler.MessageType.Error);
                OpenProjectSerializer.RemoveOpenProject(newProject.id);
            }
        }
        private List<ProjectElement> getProjects(DominoAssembly dominoAssembly)
        {
            List<ProjectElement> returnList = new List<ProjectElement>();

            if (dominoAssembly != null)
            {
                ProjectElement color = new ProjectElement(dominoAssembly.colorPath, @".\Icons\colorLine.ico", null);
                returnList.Add(color);
            }

            foreach (DocumentNode dominoWrapper in dominoAssembly.children.OfType<DocumentNode>().ToList())
            {
                try
                {
                    string relativePath = dominoWrapper.relativePath;
                    string filepath = Workspace.AbsolutePathFromReference(ref relativePath, dominoWrapper.parent);
                    dominoWrapper.relativePath = relativePath;
                    string picturepath = ImageHelper.GetImageOfFile(filepath);
                    ProjectElement project = new ProjectElement(filepath,
                        picturepath, dominoWrapper);
                    returnList.Add(project);
                    

                }
                catch (FileNotFoundException)
                {
                    // Remove file from Project
                    dominoAssembly.children.Remove(dominoWrapper);
                    Errorhandler.RaiseMessage($"The file {dominoWrapper.relativePath} doesn't exist at the current location. \nIt has been removed from the project.", "Missing file", Errorhandler.MessageType.Error);
                    dominoAssembly.Save();
                }
            }
            dominoAssembly.Save();
            return returnList;
        }

        private ProjectComposite AddProjectToTree(ProjectListComposite parentProject, ProjectElement projectTransfer)
        {
            try
            {
                /*if (!Path.GetExtension(projectTransfer.FilePath).Equals(".DColor"))
                {
                    while (!File.Exists(projectTransfer.IcoPath))
                    {
                        MessageBox.Show(String.Format("Could not find: {0} Please reload the image.", projectTransfer.IcoPath), "Error!", MessageBoxButton.YesNo, MessageBoxImage.Error);
                        System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (File.Exists(openFileDialog.FileName))
                                File.Copy(openFileDialog.FileName, projectTransfer.IcoPath);
                        }
                        //jojo sonst einfach raus nehmen
                    }
                }*/
            }
            catch (Exception) { MessageBox.Show("Error loading open projects!", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }

            if (Path.GetExtension(projectTransfer.FilePath).ToLower().Equals(".dcolor") || Path.GetExtension(projectTransfer.FilePath).ToLower().Equals(".dobject"))
            {
                try
                {
                    ProjectComposite newItem = parentProject.AddProject(new ProjectComposite(projectTransfer, parentProject.OwnID));
                    newItem.IsClicked += Item_IsClicked;
                    newItem.conMenu.removeMI.Click += parentProject.RemoveMI_Object_Click;
                    newItem.conMenu.renameMI.Click += parentProject.RenameMI_Object_Click;
                    newItem.SelectedEvent += MainWindowViewModel_SelectedEvent;

                    newItem.conMenu.openMI.Click += OpenMI_Click;
                    return newItem;
                }
                catch (Exception)
                {
                    NotFindRemove(parentProject, projectTransfer);
                    return null;
                }
            }
            else
            {
                NotFindRemove(parentProject, projectTransfer);
                return null;
            }
        }

        private void NotFindRemove(ProjectListComposite parentProject, ProjectElement projectTransfer)
        {
            Errorhandler.RaiseMessage(string.Format("Could not find all files: {0}", Path.GetFileNameWithoutExtension(projectTransfer.FilePath)), "Not found!", Errorhandler.MessageType.Error);
            ((AssemblyNode)parentProject.Project.documentNode).obj.children.Remove(projectTransfer.documentNode);
            ((AssemblyNode)parentProject.Project.documentNode).obj.Save();
        }

        /// <summary>
        /// Neues Unterprojekt starten
        /// </summary>
        private void NewFieldStructure()
        {
            if (SelectedProject == null || !(SelectedProject is ProjectListComposite))
            {
                Errorhandler.RaiseMessage("Please choose a project folder.", "Please choose", Errorhandler.MessageType.Error);
                return;
            }
            NewObjectVM novm = new NewObjectVM(Path.GetDirectoryName(SelectedProject.FilePath), ((AssemblyNode)((ProjectListComposite)SelectedProject).Project.documentNode).obj);
            new NewObject(novm).ShowDialog();
            if (!novm.Close || novm.ResultNode == null) return;
            ProjectComposite compo = AddProjectToTree((ProjectListComposite)SelectedProject, 
                new ProjectElement(novm.ObjectPath, Path.Combine(novm.ProjectPath, "Source Image", ImageHelper.GetImageOfFile(novm.ObjectPath)), novm.ResultNode));
            OpenItem(compo);
        }

        private void AddProject_Exists()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "project files (*.DProject)|*.DProject";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    OpenProject openProject = OpenProjectSerializer.AddOpenProject(Path.GetFileNameWithoutExtension(openFileDialog.FileName), Path.GetDirectoryName(openFileDialog.FileName));
                    loadProject(openProject);
                }
            }
        }

        private void AddItem_Exists()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "project files (*.DObject)|*.DObject";
            openFileDialog.RestoreDirectory = true;
            if (SelectedProject == null || !(SelectedProject is ProjectListComposite))
            {
                Errorhandler.RaiseMessage("Please choose a project folder.", "Please choose", Errorhandler.MessageType.Error);
                return;
            }
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    try
                    {
                        DocumentNode node = (DocumentNode)IDominoWrapper.CreateNodeFromPath(((AssemblyNode)SelectedProject.Project.documentNode).obj, openFileDialog.FileName);
                        string picturepath = ImageHelper.GetImageOfFile(openFileDialog.FileName);
                        ProjectComposite compo = AddProjectToTree((ProjectListComposite)SelectedProject, 
                            new ProjectElement(openFileDialog.FileName, Path.Combine(SelectedProject.FilePath, "Source Image", picturepath), node));
                        ((AssemblyNode)SelectedProject.Project.documentNode).obj.Save();
                        OpenItem(compo);
                        
                    }
                    catch (FileNotFoundException)
                    {
                        // Unable to load project
                    }
                }
            }
        }

        private void CreateNewProject()
        {
            NewProjectVM curNPVM = new NewProjectVM();
            new NewProject(curNPVM).ShowDialog();
            if (curNPVM.Close == true)
            {
                OpenProject newProj = OpenProjectSerializer.AddOpenProject(curNPVM.ProjectName, string.Format(@"{0}\{1}", curNPVM.SelectedPath, curNPVM.ProjectName));
                if (newProj == null)
                {
                    Errorhandler.RaiseMessage("Could not create new Project!", "Error!", Errorhandler.MessageType.Error);
                    return;
                }
                loadProject(newProj);
            }
        }

        /// <summary>
        /// Save all open projects
        /// </summary>
        private void SaveAllOpen()
        {
            foreach (TabItem curTI in Tabs)
            {
                if (curTI.Content.UnsavedChanges)
                {
                    if (!curTI.Content.Save())
                    {
                        Errorhandler.RaiseMessage("Error Saving files!", string.Format("Stop saving, because could not save {0}", curTI.Header), Errorhandler.MessageType.Error);
                        return;
                    }
                }
            }
            Errorhandler.RaiseMessage("Save all files", "Saves all files!", Errorhandler.MessageType.Info);
        }
        /// <summary>
        /// Save current project
        /// </summary>
        private void SaveCurrentOpenProject()
        {
            if (SelectedTab.Content.Save())
                Errorhandler.RaiseMessage("Save all changes!", "Save all changes", Errorhandler.MessageType.Info);
            else
                Errorhandler.RaiseMessage("Error!", "Error saving changes!", Errorhandler.MessageType.Error);
        }
        #endregion
    }

    public sealed class TabItem : ModelBase
    {
        #region CTOR
        public TabItem(int projectID, string path)
        {
            Path = path;
            Close = new RelayCommand(o => CloseThis());
            zusatz = "";
        }

        public TabItem(ProjectComposite project) : this(project.OwnID, project.ParentProjectID, project.Name, project.PicturePath, project.FilePath)
        {
            ProjectComp = project;
            ResetContent();
        }

        internal void ResetContent()
        {
            if (Content is EditProjectVM editProject)
            {
                editProject.ClearCanvas();
            }
            if (ProjectComp.Project.documentNode is DocumentNode documentNode)
            {
                if (documentNode.obj != null)
                {
                    if (documentNode.obj.Editing)
                    {
                        Content = new EditProjectVM(documentNode);
                    }
                    else
                    {
                        switch (documentNode)
                        {
                            case FieldNode fieldNode:
                                Content = new CreateFieldVM((FieldParameters)fieldNode.obj, true);
                                break;
                            case StructureNode structureNode:
                                Content = new CreateRectangularStructureVM((StructureParameters)structureNode.obj, true);
                                break;
                            case SpiralNode spiralNode:
                                Content = new CreateSpiralVM((SpiralParameters)spiralNode.obj, true);
                                break;
                            case CircleNode circleNode:
                                Content = new CreateCircleVM((CircleParameters)circleNode.obj, true);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            Content.UnsavedChanges = false;
        }

        public TabItem(int ownID, int projectID, string Header, string picturePath, string path) : this(projectID, path)
        {
            OwnID = ownID;
            ProjectID = projectID;
            this.Header = Header;
            this.picture = picturePath;
        }

        public TabItem(int ownID, int projectID, string Header, string picturePath, string path, TabBaseVM content) : this(ownID, projectID, Header, picturePath, path)
        {
            this.Content = content;
        }
        #endregion

        #region EventHandler
        public delegate bool CloseDelegate(TabItem TI);

        public CloseDelegate CloseIt;
        #endregion

        #region prope
        private ProjectComposite _ProjectComp;
        public ProjectComposite ProjectComp
        {
            get { return _ProjectComp; }
            set
            {
                if (_ProjectComp != value)
                {
                    _ProjectComp = value;
                    RaisePropertyChanged();
                }
            }
        }

        //public WriteableBitmap picture { get; set; }

        public string picture { get; set; }

        private int _OwnID;
        public int OwnID
        {
            get { return _OwnID; }
            set
            {
                if (_OwnID != value)
                {
                    _OwnID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ProjectID;
        public int ProjectID
        {
            get { return _ProjectID; }
            set
            {
                if (_ProjectID != value)
                {
                    _ProjectID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string zusatz { get; set; }

        private string _Header;
        public string Header
        {
            get { return string.Format("{0}{1}", _Header, zusatz); }
            set
            {
                if (_Header != value)
                {
                    _Header = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Path { get; set; }

        private TabBaseVM _Content;
        public TabBaseVM Content
        {
            get { return _Content; }
            set
            {
                if (_Content != value)
                {
                    if (_Content != null)
                        _Content.Changes -= _Content_Changes;
                    _Content = value;
                    _Content.Changes += _Content_Changes;
                    RaisePropertyChanged();
                }
            }
        }

        private void _Content_Changes(object sender, bool e)
        {
            if (e == true)
                zusatz = "*";
            else
                zusatz = "";
            RaisePropertyChanged("Header");
        }
        #endregion

        #region METHODS
        private void CloseThis()
        {
            if (CloseIt?.Invoke(this) == true)
            {
                Content?.Close();
            }
        }
        #endregion

        #region Command
        private ICommand _Close;
        public ICommand Close { get { return _Close; } set { if (value != _Close) { _Close = value; } } }
        #endregion
    }
}
