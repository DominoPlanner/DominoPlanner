using DominoPlanner.Core;
using DominoPlanner.Usage.Serializer;
using DominoPlanner.Usage.UserControls.ViewModel;
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
            NewFieldStruct = new RelayCommand(o => { NewFieldStructure(); });
            MenuSetStandard = new RelayCommand(o => { new SetStandardV().ShowDialog(); });
            AddExistingProject = new RelayCommand(o => { AddProject_Exists(); });
            AddExistingItem = new RelayCommand(o => { AddItem_Exists(); });
            NewProject = new RelayCommand(o => { CreateNewProject(); });
            SaveAll = new RelayCommand(o => { SaveAllOpen(); });
            SaveCurrentOpen = new RelayCommand(o => { SaveCurrentOpenProject(); });

            Tabs = new ObservableCollection<TabItem>();

            loadProjectList();
        }

        private void CurrentProject_EditingChanged(object sender, EventArgs e)
        {
            TabItem tabItem = Tabs.Where(x => x.Content.CurrentProject == sender).FirstOrDefault();
            tabItem.Content.Save();
            tabItem.ResetContent();
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
                    _SelectedTab = value;
                    if(SelectedTab != null)
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
        /// <summary>
        /// Remove selected Project
        /// </summary>
        private void RemoveMI_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            List<TabItem> removeList = Tabs.Where(x => x.ProjectID == ((ProjectListComposite)((System.Windows.Controls.MenuItem)sender).DataContext).OwnID).ToList<TabItem>();
            for (int i = 0; i < removeList.Count; i++)
            {
                Tabs.Remove(removeList[0]);
            }

            if (OpenProjectSerializer.RemoveOpenProject(((ProjectListComposite)((System.Windows.Controls.MenuItem)sender).DataContext).OwnID))
            {
                Projects.Remove(((ProjectListComposite)((System.Windows.Controls.MenuItem)sender).DataContext));
            }
            else
            {
                MessageBox.Show("Could not remove the project!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    selTab = new TabItem(toOpen);
                    Tabs.Add(selTab);
                }
            }

            selTab.CloseIt += MainWindowViewModel_CloseIt;
            if (selTab.Content.CurrentProject != null)
            {
                selTab.Content.CurrentProject.EditingChanged += CurrentProject_EditingChanged;
            }

            SelectedTab = selTab;
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
        private void MainWindowViewModel_CloseIt(object sender, EventArgs e)
        {
            Tabs.Remove((TabItem)sender);
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
                MessageBox.Show("Error loading opened projects!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                OpenProjectSerializer.Create();
            }
        }

        private void loadProject(OpenProject newProject)
        {
            if (Directory.Exists(newProject.path))
            {
                AssemblyNode mainnode = new AssemblyNode(Path.Combine(newProject.path, string.Format("{0}.DProject", newProject.name)));

                ProjectListComposite actPLC = new ProjectListComposite(newProject.id, newProject.name, newProject.path, new ProjectElement(mainnode.Path, "", mainnode));
                actPLC.SelectedEvent += MainWindowViewModel_SelectedEvent;
                actPLC.conMenu.createMI.Click += CreateMI_Click;
                actPLC.conMenu.removeMI.Click += RemoveMI_Click;
                actPLC.Children.CollectionChanged += Children_CollectionChanged;
                Projects.Add(actPLC);

                foreach (ProjectElement currPT in getProjects(mainnode.obj))
                {
                    AddProjectToTree(actPLC, currPT);
                }
            }
            else
            {
                MessageBox.Show(string.Format("Could not find: {0} ", newProject.name), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach(ProjectComposite old in e.OldItems.OfType<ProjectComposite>())
                {
                    foreach(TabItem tabItem in Tabs.ToArray())
                    {
                        if(tabItem.ProjectComp == old)
                        {
                            Tabs.Remove(tabItem);
                        }
                    }
                }
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

            foreach (DocumentNode dominoWrapper in dominoAssembly.children.OfType<DocumentNode>())
            {
                string filepath = Workspace.AbsolutePathFromReference(dominoWrapper.relativePath, dominoWrapper.parent);
                string picturepath = ImageHelper.GetImageOfFile(filepath);
                ProjectElement project = new ProjectElement(Workspace.AbsolutePathFromReference(dominoWrapper.relativePath, dominoWrapper.parent),
                    picturepath, dominoWrapper); 
                returnList.Add(project);
            }

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
            catch (Exception) { MessageBox.Show("Error loading openprojects!", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            ProjectComposite newItem = parentProject.AddProject(new ProjectComposite(projectTransfer, parentProject.OwnID)); 
            newItem.IsClicked += Item_IsClicked;
            newItem.conMenu.removeMI.Click += parentProject.RemoveMI_Object_Click;
            newItem.SelectedEvent += MainWindowViewModel_SelectedEvent;
            return newItem;
        }

        /// <summary>
        /// Neues Unterprojekt starten
        /// </summary>
        private void NewFieldStructure()
        {
            if (SelectedProject == null || !(SelectedProject is ProjectListComposite))
            {
                MessageBox.Show("Please choose a project folder.");
                return;
            }
            NewObjectVM novm = new NewObjectVM(Path.GetDirectoryName(SelectedProject.FilePath), ((AssemblyNode)((ProjectListComposite)SelectedProject).Project.documentNode).obj);
            new NewObject(novm).ShowDialog();
            if (!novm.Close || novm.ResultNode == null) return;
            ProjectComposite compo = AddProjectToTree((ProjectListComposite)SelectedProject, new ProjectElement(novm.ObjectPath, Path.Combine(novm.ProjectPath, "Source Image", novm.internPictureName), novm.ResultNode));
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
            if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    DocumentNode node = (DocumentNode)IDominoWrapper.CreateNodeFromPath(((AssemblyNode)SelectedProject.Project.documentNode).obj, openFileDialog.FileName);
                    string picturepath = ImageHelper.GetImageOfFile(openFileDialog.FileName);
                    ProjectComposite compo = AddProjectToTree((ProjectListComposite)SelectedProject, new ProjectElement(openFileDialog.FileName, Path.Combine(SelectedProject.FilePath, "Source Image", picturepath), node));
                    ((AssemblyNode)SelectedProject.Project.documentNode).obj.Save();
                    OpenItem(compo);
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
                    MessageBox.Show("Could not create new Project!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show("Error Saving files!", string.Format("Stop saving, because could not save {0}", curTI.Header), MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            MessageBox.Show("Save all files", "Saves all files!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }
        /// <summary>
        /// Save current project
        /// </summary>
        private void SaveCurrentOpenProject()
        {
            if (SelectedTab.Content.Save())
                MessageBox.Show("Save all changes!", "Save all changes", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            else
                MessageBox.Show("Error!", "Error saving changes!", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if(Content is EditProjectVM editProject)
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
                                Content = new CreateFieldVM(fieldNode);
                                break;
                            case StructureNode structureNode:
                                Content = new CreateStructureVM(structureNode.obj, true);
                                break;
                            case SpiralNode spiralNode:
                            case CircleNode circleNode:
                                Content = new CreateStructureVM(documentNode.obj, false);
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
        public event EventHandler CloseIt;
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
            Content?.Close();
            CloseIt?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Command
        private ICommand _Close;
        public ICommand Close { get { return _Close; } set { if (value != _Close) { _Close = value; } } }
        #endregion
    }
}
