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
        public abstract void Undo();
        public abstract void Redo();
        public abstract bool Save();
        #endregion

        #region prope
        private IDominoProvider _CurrentProject;
        public IDominoProvider CurrentProject
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
