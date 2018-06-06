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
        public abstract void Undo();
        public abstract void Redo();
        public abstract bool Save();
        #endregion

        #region prope
        private bool _UnsavedChanges;
        public bool UnsavedChanges
        {
            get { return _UnsavedChanges; }
            set
            {
                if (_UnsavedChanges != value)
                {
                    _UnsavedChanges = value;
                    if (Changes != null)
                        Changes(this, value);
                }
            }
        }

        public abstract TabItemType tabType { get; }
        #endregion

        #region EventHandler
        public event EventHandler<bool> Changes;
        #endregion

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);
            if(!propertyName.Equals("SelectedStone"))
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
