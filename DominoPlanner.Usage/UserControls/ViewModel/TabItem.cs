using Avalonia.Input;
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
        public TabItem(string path, string Zusatz)
        {
            Path = path;
            Close = new RelayCommand(o => CloseThis());
            zusatz = Zusatz;
        }

        public TabItem(string Header, string picturePath, string path) : this(path, "")
        {
            this.Header = Header;
            this.Picture = picturePath;
        }

        public TabItem(string Header, string picturePath, string path, TabBaseVM content) : this(Header, picturePath, path)
        {
            this.Content = content;
        }
        public TabItem(string path) : this(path, "")
        {
            this.Header = System.IO.Path.GetFileNameWithoutExtension(path);
            this.Picture = ImageHelper.GetImageOfFile(path);
            var ext = System.IO.Path.GetExtension(path).ToLower();
            if (ext == "." + Properties.Settings.Default.ColorExtension.ToLower())
            {
                Content = new ColorListControlVM(path);
                ResetContent();
            }
            else if (ext == "."+  Properties.Settings.Default.ObjectExtension.ToLower())
            {
                Content = ViewModelGenerator(Workspace.Load<IDominoProvider>(path), (path));
                ResetContent();
            }
            else
            {
                throw new InvalidOperationException("Incorrect file extension");
            }
        }

        public TabItem(DocumentNodeVM project) : this(project.Name, ImageHelper.GetImageOfFile(project.AbsolutePath), project.AbsolutePath)
        {
            Content = ViewModelGenerator(project.DocumentModel.Obj, project.AbsolutePath);
            ResetContent();
        }
        public TabItem(ColorNodeVM project) : this(project.Name, ImageHelper.GetImageOfFile(project.AbsolutePath), project.AbsolutePath)
        { 
            Content = new ColorListControlVM(project.Parent.AssemblyModel);
            ResetContent();
        }
        public static TabItem TabItemGenerator(NodeVM project)
        {
            switch (project)
            {
                case DocumentNodeVM dn:
                    return new TabItem(dn);
                case ColorNodeVM cn:
                    return new TabItem(cn);
                default:
                    return null;
            }
        }
        public static DominoProviderTabItem ViewModelGenerator(IDominoProvider project, string path)
        {
            DominoProviderTabItem Content = null;
            if (project != null)
            {
                if (project.Editing)
                {
                    Content = new EditProjectVM(project);
                }
                else
                {
                    switch (project)
                    {
                        case FieldParameters fieldNode:
                            Content = new CreateFieldVM(fieldNode, true);
                            break;
                        case StructureParameters structureNode:
                            Content = new CreateRectangularStructureVM(structureNode, true);
                            break;
                        case SpiralParameters spiralNode:
                            Content = new CreateSpiralVM(spiralNode, true);
                            break;
                        case CircleParameters circleNode:
                            Content = new CreateCircleVM(circleNode, true);
                            break;
                        default:
                            break;
                    }
                }

                //(Content as DominoProviderTabItem).assemblyname =
                //           OpenProjectSerializer.GetOpenProjects().Where(x => x.id == project.ParentProjectID).First().name;

                (Content as DominoProviderTabItem).name = System.IO.Path.GetFileNameWithoutExtension(path);
            }
            return Content;
        }

        internal void KeyPressed(object sender, KeyEventArgs args)
        {
            if (!args.Handled)
            {
                Content?.KeyPressed(sender, args);
            }
        }

        internal void ResetContent()
        {

            if (Content is EditProjectVM editProject)
            {
                editProject.ClearCanvas();
            }
            Content.UnsavedChanges = false;
        }

        
        #endregion

        #region EventHandler
        public delegate Task<bool> CloseDelegate(TabItem TI);

        public CloseDelegate CloseIt;
        #endregion

        #region prope

        public string Picture { get; set; }

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
        private async void CloseThis()
        {
            if (await CloseIt?.Invoke(this) == true)
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

        public bool VisibleFieldplan
        {
            get
            {
                if (CurrentProject?.HasProtocolDefinition == true)
                    return true;
                else return false;
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
        public virtual bool Collapsible
        {
            get
            {
                if (CurrentProject != null && CurrentProject is FieldParameters)
                {
                    return true;
                }
                else return false;
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
            protocolV.Show();
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
