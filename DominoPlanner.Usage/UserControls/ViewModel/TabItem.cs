using DominoPlanner.Core;
using DominoPlanner.Usage.Serializer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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

        public TabItem(string Header, string picturePath, string path) : this(path)
        {
            this.Header = Header;
            this.picture = picturePath;
        }

        public TabItem(string Header, string picturePath, string path, TabBaseVM content) : this(Header, picturePath, path)
        {
            this.Content = content;
        }
        public TabItem(string path)
        {
            Path = path;
            Close = new RelayCommand(o => CloseThis());
            this.Header = System.IO.Path.GetFileNameWithoutExtension(path);
            this.picture = ImageHelper.GetImageOfFile(path);
            var ext = System.IO.Path.GetExtension(path).ToLower();
            if (ext == Properties.Resources.ColorExtension.ToLower())
            {
                Content = new ColorListControlVM(path);
                ResetContent();
            }
            else if (ext == Properties.Resources.ObjectExtension.ToLower())
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
            if (Content == null)
            {
                Content = ViewModelGenerator(project.DocumentModel.obj, project.AbsolutePath);
                ResetContent();
            }
        }
        public TabItem(ColorNodeVM project) : this(project.Name, ImageHelper.GetImageOfFile(project.AbsolutePath), project.AbsolutePath)
        { 
            Content = new ColorListControlVM(project.parent.AssemblyModel.obj);
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

        internal void ResetContent()
        {

            if (Content is EditProjectVM editProject)
            {
                editProject.ClearCanvas();
            }
            else
            {
                Content.UnsavedChanges = false;
            }
        }

        
        #endregion

        #region EventHandler
        public delegate bool CloseDelegate(TabItem TI);

        public CloseDelegate CloseIt;
        #endregion

        #region prope

        public string picture { get; set; }

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
        

        private string _Header;
        public string Header
        {
            get { return _Header; }
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
                    _Content = value;
                    RaisePropertyChanged();
                }
            }
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

        public ObservableStack<PostFilter> undoStack = new ObservableStack<PostFilter>();
        public ObservableStack<PostFilter> redoStack = new ObservableStack<PostFilter>();
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
            undoStack.CollectionChanged += UndoStack_CollectionChanged;
        }

        private void UndoStack_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var stack = sender as ObservableStack<PostFilter>;
            if (stack.Count == 0)
            {
                UnsavedChanges = false;
            }
            else if (stack.TakeWhile(x => x is SelectionOperation).Count(x => true) == stack.Count)
            {
                UnsavedChanges = false;
            }
            else
            {
                UnsavedChanges = true;
            }
        }

        #endregion
        #region commands
        private ICommand _BuildtoolsClick;
        public ICommand BuildtoolsClick { get { return _BuildtoolsClick; } set { if (value != _BuildtoolsClick) { _BuildtoolsClick = value; } } }

        #endregion
    }
    public class ObservableStack<T> : Stack<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public ObservableStack()
        {
        }

        public ObservableStack(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                base.Push(item);
        }

        public ObservableStack(List<T> list)
        {
            foreach (var item in list)
                base.Push(item);
        }


        public new virtual void Clear()
        {
            base.Clear();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public new virtual T Pop()
        {
            var item = base.Pop();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            return item;
        }

        public new virtual void Push(T item)
        {
            base.Push(item);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }


        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;


        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.RaiseCollectionChanged(e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.RaisePropertyChanged(e);
        }


        protected virtual event PropertyChangedEventHandler PropertyChanged;


        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
                this.CollectionChanged(this, e);
        }

        private void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }
        
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { this.PropertyChanged += value; }
            remove { this.PropertyChanged -= value; }
        }
    }
}
