using DominoPlanner.Core;
using DominoPlanner.Usage.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
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
        public static DominoProviderTabItem ViewModelGenerator(ProjectComposite project)
        {
            DominoProviderTabItem Content = null;
            if (((DocumentNode)project.Project.documentNode).obj != null)
            {
                DocumentNode dn = ((DocumentNode)project.Project.documentNode);
                if (((DocumentNode)project.Project.documentNode).obj.Editing)
                {
                    Content = new EditProjectVM((DocumentNode)project.Project.documentNode);
                }
                else
                {
                    switch (dn)
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

                (Content as DominoProviderTabItem).assemblyname =
                            OpenProjectSerializer.GetOpenProjects().Where(x => x.id == project.ParentProjectID).First().name;

                (Content as DominoProviderTabItem).name = System.IO.Path.GetFileNameWithoutExtension(dn.relativePath);
            }
            return Content;
        }

        internal void ResetContent()
        {
            if (Content is EditProjectVM editProject)
            {
                editProject.ClearCanvas();
            }
            if (ProjectComp.Project.documentNode is DocumentNode documentNode)
            {
                Content = ViewModelGenerator(ProjectComp);
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
    public abstract class DominoProviderTabItem : TabBaseVM
    {
        #region fields

        public string name { get; set; }
        public string assemblyname { get; set; }

        public Stack<PostFilter> undoStack = new Stack<PostFilter>();
        public Stack<PostFilter> redoStack = new Stack<PostFilter>();
        #endregion
        #region properties
        private IDominoProvider _CurrentProject;
        public override IDominoProvider CurrentProject
        {
            get { return _CurrentProject; }
            set
            {
                if (_CurrentProject != value)
                {
                    _CurrentProject = value;
                    TabPropertyChanged("VisibleFieldplan", ProducesUnsavedChanges: false);
                    TabPropertyChanged("Collapsible", ProducesUnsavedChanges: false);
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }

        public Visibility VisibleFieldplan
        {
            get
            {
                if (CurrentProject?.HasProtocolDefinition == true)
                    return Visibility.Visible;
                else return Visibility.Collapsed;
            }
        }

        private int _physicalLength;
        public int PhysicalLength
        {
            get
            {
                return _physicalLength;
            }
            set
            {
                if (_physicalLength != value)
                {
                    _physicalLength = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }

        private int _physicalHeight;
        public int PhysicalHeight
        {
            get { return _physicalHeight; }
            set
            {
                if (_physicalHeight != value)
                {
                    _physicalHeight = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }
        public virtual Visibility Collapsible
        {
            get
            {
                if (CurrentProject != null && CurrentProject is FieldParameters)
                {
                    return Visibility.Visible;
                }
                else return Visibility.Collapsed;
            }
        }

        private bool _undostate;

        public bool undostate
        {
            get { return _undostate; }
            set { _undostate = value; }
        }
        public Func<DominoProviderTabItem, DominoProviderTabItem> GetNewViewModel;
        public Action<DominoProviderTabItem, DominoProviderTabItem> RegisterNewViewModel;
        public bool Editing
        {
            get { return CurrentProject.Editing; }
            set
            {
                if (this is EditProjectVM vm)
                {
                    EditingDeactivatedOperation op = new EditingDeactivatedOperation(vm);
                    op.Apply();
                    undoStack.Push(op);
                }
                else if (this is DominoProviderVM vm2)
                {
                    EditingActivatedOperation op = new EditingActivatedOperation(vm2);
                    op.Apply();
                    undoStack.Push(op);
                }
            }
        }

        #endregion
        #region methods
        public override void Undo()
        {
            undostate = true;
            if (undoStack.Count != 0)
            {
                PostFilter undoFilter = undoStack.Pop();
                redoStack.Push(undoFilter);
                undoFilter.Undo();
                if (undoStack.Count == 0) UnsavedChanges = false;
            }
            undostate = false;
        }

        public override void Redo()
        {
            undostate = true;
            if (redoStack.Count != 0)
            {
                PostFilter redoFilter = redoStack.Pop();
                undoStack.Push(redoFilter);
                redoFilter.Apply();
            }
            undostate = false;
        }
        private void OpenBuildTools()
        {
            ProtocolV protocolV = new ProtocolV();
            protocolV.DataContext = new ProtocolVM(CurrentProject, name, assemblyname);
            protocolV.ShowDialog();
        }
        public override bool Save()
        {
            try
            {
                CurrentProject.Save();
                UnsavedChanges = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public DominoProviderTabItem()
        {
            BuildtoolsClick = new RelayCommand(o => { OpenBuildTools(); });
        }

        #endregion
        #region commands
        private ICommand _BuildtoolsClick;
        public ICommand BuildtoolsClick { get { return _BuildtoolsClick; } set { if (value != _BuildtoolsClick) { _BuildtoolsClick = value; } } }

        #endregion
    }
}
