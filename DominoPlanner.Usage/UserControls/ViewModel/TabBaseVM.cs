using Avalonia.Input;
using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public abstract class TabBaseVM : ModelBase
    {
        #region CTOR
        public TabBaseVM()
        {
            UndoComm = new RelayCommand(o => { Undo(); });
            RedoComm = new RelayCommand(o => { Redo(); });
        }
        #endregion

        #region Methods
        internal virtual void ResetContent() { }
        public abstract bool Save();

        protected bool undostate { get; set; }

        public Stack<PostFilter> undoStack = new Stack<PostFilter>();
        public Stack<PostFilter> redoStack = new Stack<PostFilter>();

        public virtual void PropertyValueChanged(object sender, object value_new,
           [CallerMemberName] string membername = "", bool producesUnsavedChanges = true, Action PostAction = null, Action PostUndoAction = null)
        {
            if (!undostate)
            {
                try
                {
                    undostate = true;
                    if (producesUnsavedChanges)
                        UnsavedChanges = true;
                    var filter = new PropertyChangedOperation(sender, value_new, membername, PostAction);
                    if (undoStack.Count != 0)
                    {
                        var lastOnStack = undoStack.Peek();
                        if (lastOnStack is PropertyChangedOperation op)
                        {
                            if (op.sender == sender && op.membername == membername)
                            {
                                // property has been changed multiple times in a row
                                if (!op.value_old.Equals(value_new))
                                {
                                    op.value_new = value_new;
                                    undoStack.Pop();
                                    filter = op;
                                }
                            }
                        }
                    }
                    undoStack.Push(filter);
                    filter.Apply();
                    redoStack = new Stack<PostFilter>();
                }
                finally
                {
                    undostate = false;
                }
            }
        }
        public virtual void Undo()
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

        public virtual void Redo()
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

        #endregion

        #region prope
        private IDominoProvider _CurrentProject;
        public virtual IDominoProvider CurrentProject
        {
            get { return _CurrentProject; }
            set
            {
                if (_CurrentProject != value)
                {
                    _CurrentProject = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private bool _UnsavedChanges;
        public bool UnsavedChanges
        {
            get { return _UnsavedChanges; }
            set
            {
                if (_UnsavedChanges != value)
                {
                    _UnsavedChanges = value;
                    Changes?.Invoke(this, value);
                }
            }
        }

        private string _FilePath;
        public string FilePath
        {
            get { return _FilePath; }
            set
            {
                if (_FilePath != value)
                {
                    _FilePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        public abstract TabItemType tabType { get; }
        #endregion

        #region EventHandler
        public event EventHandler<bool> Changes;
        #endregion

        internal virtual void Close(){ }

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);
            TabPropertyChanged(propertyName);
        }
        protected void TabPropertyChanged([CallerMemberName] string propertyName = null, bool ProducesUnsavedChanges = true)
        {
            base.RaisePropertyChanged(propertyName);
            if (ProducesUnsavedChanges)
                UnsavedChanges = true;

        }

        #region Comm
		private ICommand _UndoComm;
        public ICommand UndoComm { get { return _UndoComm; } set { if (value != _UndoComm) { _UndoComm = value; } } }
        
	    private ICommand _RedoComm;
        public ICommand RedoComm { get { return _RedoComm; } set { if (value != _RedoComm) { _RedoComm = value; } } }

        internal virtual void KeyPressed(object sender, KeyEventArgs args) { }
        #endregion
    }

    public enum TabItemType
    {
        ColorList,
        CreateField,
        CreateStructure,
        EditProject,
        Masterplan
    }
}
