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
            NewProject = new RelayCommand(o => { CreateNewProject(); });
            SaveAll = new RelayCommand(o => { SaveAllOpen(); });
            SaveCurrentOpen = new RelayCommand(o => { SaveCurrentOpenProject(); });

            Tabs = new ObservableCollection<TabItem>();
            Tabs.Add(new TabItem(50, 100, "Erstes Feld", @"\Icons\lock - Copy.ico", "", new CreateFieldVM(@"C:\Users\johan\Desktop\field.DObject")));
            Tabs.Last<TabItem>().CloseIt += MainWindowViewModel_CloseIt;
            Tabs.Last<TabItem>().Content.CurrentProject.EditingChanged += CurrentProject_EditingChanged;
            /*
            Tabs.Add(new TabItem(12, 100, "Erste Rechteckige Struktur", @"\Icons\lock - Copy.ico", "", new CreateStructureVM(@"C:\Users\johan\Desktop\colors.DObject", true)));
            Tabs.Last<TabItem>().CloseIt += MainWindowViewModel_CloseIt;

            Tabs.Add(new TabItem(12, 100, "Erste Runde Struktur", @"\Icons\lock - copy.ico", "", new CreateStructureVM(@"C:\Users\johan\Desktop\round.DObject", false)));
            Tabs.Last<TabItem>().CloseIt += MainWindowViewModel_CloseIt;
            */
           // Tabs.Add(new TabItem(465, 100, "Nachbearbeiten", @"\Icons\lock - Copy.ico", "", new EditProjectVM()));
            //Tabs.Last<TabItem>().CloseIt += MainWindowViewModel_CloseIt;
            
            loadProjectList();
        }

        private void CurrentProject_EditingChanged(object sender, EventArgs e)
        {
            TabItem tabItem = Tabs.Where(x => x.Content.CurrentProject == sender).FirstOrDefault();
            tabItem.Content.Save();   
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
            ProjectComposite clickedValue = (ProjectComposite)sender;
            if (clickedValue.ActType == NodeType.ColorListNode)
                Tabs.Add(new TabItem(clickedValue.OwnID, clickedValue.ParentProjectID, clickedValue.Name, clickedValue.PicturePath, clickedValue.FilePath, new ColorListControlVM(clickedValue.FilePath)));
            else if (clickedValue.ActType == NodeType.FieldNode || clickedValue.ActType == NodeType.StructureNode || clickedValue.ActType == NodeType.FreeHandFieldNode)
                Tabs.Add(new TabItem(clickedValue.OwnID, clickedValue.ParentProjectID, clickedValue.Name, clickedValue.PicturePath, clickedValue.FilePath));

            Tabs.Last<TabItem>().CloseIt += MainWindowViewModel_CloseIt;
            Tabs.Last<TabItem>().Content.CurrentProject.EditingChanged += CurrentProject_EditingChanged;
            SelectedTab = Tabs.Last<TabItem>();
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
            ProjectListComposite actPLC = new ProjectListComposite(newProject.id, newProject.name, newProject.path);
            actPLC.SelectedEvent += MainWindowViewModel_SelectedEvent;
            actPLC.conMenu.createMI.Click += CreateMI_Click;
            actPLC.conMenu.removeMI.Click += RemoveMI_Click;
            Projects.Add(actPLC);

            foreach (ProjectTransfer currPT in ProjectSerializer.GetProjects(actPLC.FilePath))
            {
                try
                {
                    if (!Path.GetExtension(currPT.FilePath).Equals(".dpcol"))
                    {
                        while (!File.Exists(currPT.IcoPath))
                        {
                            MessageBox.Show(String.Format("Could not find: {0} Please reload the image.", currPT.IcoPath), "Error!", MessageBoxButton.YesNo, MessageBoxImage.Error);
                            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                if (File.Exists(openFileDialog.FileName))
                                    File.Copy(openFileDialog.FileName, currPT.IcoPath);
                            }
                        }
                    }
                }
                catch (Exception) { MessageBox.Show("Error loading openprojects!", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                ProjectComposite newItem = actPLC.AddProject(new ProjectComposite(currPT.Id, newProject.id, Path.GetFileNameWithoutExtension(currPT.Name), currPT.IcoPath, currPT.FilePath, currPT.CurrType));
                newItem.IsClicked += Item_IsClicked;
                newItem.conMenu.removeMI.Click += actPLC.RemoveMI_Object_Click;
                newItem.SelectedEvent += MainWindowViewModel_SelectedEvent;
            }
        }

        /// <summary>
        /// Neues Unterprojekt starten
        /// </summary>
        private void NewFieldStructure()
        {
            if (SelectedProject == null || SelectedProject.GetType() != typeof(ProjectListComposite))
            {
                MessageBox.Show("Please choose a project folder.");
                return;
            }
            NewObjectVM novm = new NewObjectVM(SelectedProject.FilePath);
            new NewObject(novm).ShowDialog();
            if (!novm.Close) return;
            ProjectComposite newProject = null;
            switch (novm.selectedType)
            {
                case 0:
                    newProject = ((ProjectListComposite)SelectedProject).AddProject(new ProjectComposite(novm.ObjectID, SelectedProject.OwnID, novm.filename, Path.Combine(novm.ProjectPath, "Source Image", novm.internPictureName), novm.ObjectPath, NodeType.FieldNode));
                    break;
                case 1:
                    newProject = ((ProjectListComposite)SelectedProject).AddProject(new ProjectComposite(novm.ObjectID, SelectedProject.OwnID, novm.filename, Path.Combine(novm.ProjectPath, "Source Image", novm.internPictureName), novm.ObjectPath, NodeType.FreeHandFieldNode));
                    break;
                case 2:
                    newProject = ((ProjectListComposite)SelectedProject).AddProject(new ProjectComposite(novm.ObjectID, SelectedProject.OwnID, novm.filename, Path.Combine(novm.ProjectPath, "Source Image", novm.internPictureName), novm.ObjectPath, NodeType.StructureNode));
                    break;
                case 3:
                    newProject = ((ProjectListComposite)SelectedProject).AddProject(new ProjectComposite(novm.ObjectID, SelectedProject.OwnID, novm.filename, Path.Combine(novm.ProjectPath, "Source Image", novm.internPictureName), novm.ObjectPath, NodeType.StructureNode));
                    break;
                default: break;
            }
            newProject.SelectedEvent += MainWindowViewModel_SelectedEvent;
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
            //hier muss die DAtei unter path ausgelesen werden und dann mus Content und das Icon geladen werden
            //this.picture = new WriteableBitmap(new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute)));
            Close = new RelayCommand(o => CloseThis());
            zusatz = "";
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
