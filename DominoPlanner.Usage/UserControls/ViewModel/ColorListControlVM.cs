using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class ColorListControlVM : TabBaseVM
    {
        #region CTOR
        public ColorListControlVM() : base()
        {
            DominoList = new ObservableCollection<DominoStone>();
            DominoList.Add(new DominoStone("Weiß", 12345, System.Windows.Media.Color.FromRgb(255, 255, 255), 0));
            DominoList.Add(new DominoStone("Schwarz", 12345, System.Windows.Media.Color.FromRgb(0, 0, 0), 1));
            BtnAddColor = new RelayCommand(o => { AddNewColor(); });
            BtnSaveColors = new RelayCommand(o => { Save(); });
            BtnRemove = new RelayCommand(o => { RemoveSelected(); });
            base.UnsavedChanges = false;
        }

        public ColorListControlVM(string path) : this()
        {
            Path = path;
        }
        #endregion

        #region Methods
        public override void Undo()
        {
            throw new NotImplementedException();
        }

        public override void Redo()
        {
            throw new NotImplementedException();
        }
        private void RemoveSelected()
        {
            if (!DominoList.Contains(SelectedStone))
                return;
            DominoList.Remove(SelectedStone);
            UnsavedChanges = true;
        }

        private void AddNewColor()
        {
            DominoList.Add(new DominoStone("New Color", 1000, System.Windows.Media.Color.FromRgb(0, 0, 0), 3));
            UnsavedChanges = true;
        }

        public override bool Save()
        {
            System.Windows.MessageBox.Show("Ja mach mal das mit speichern und so");
            UnsavedChanges = false;
            return true;
        }

        private DominoStone _SelectedStone;
        public DominoStone SelectedStone
        {
            get { return _SelectedStone; }
            set
            {
                if (_SelectedStone != value)
                {
                    if (_SelectedStone != null)
                        _SelectedStone.PropertyChanged -= SelectedStone_PropertyChanged;
                    _SelectedStone = value;
                    if(_SelectedStone != null)
                        _SelectedStone.PropertyChanged += SelectedStone_PropertyChanged;
                    RaisePropertyChanged();
                }
            }
        }

        private void SelectedStone_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UnsavedChanges = true;
        }

        #endregion

        private ICommand _BtnSendMail;
        public ICommand BtnSendMail { get { return _BtnSendMail; } set { if (value != _BtnSendMail) { _BtnSendMail = value; } } }
        
        private ICommand _BtnAddColor;
        public ICommand BtnAddColor { get { return _BtnAddColor; } set { if (value != _BtnAddColor) { _BtnAddColor = value; } } }

        private ICommand _BtnSaveColors;
        public ICommand BtnSaveColors { get { return _BtnSaveColors; } set { if (value != _BtnSaveColors) { _BtnSaveColors = value; } } }
        
		private ICommand _BtnRemove;
        public ICommand BtnRemove { get { return _BtnRemove; } set { if (value != _BtnRemove) { _BtnRemove = value; } } }



        #region prop
        private string _Path;
        public string Path
        {
            get { return _Path; }
            set
            {
                if (_Path != value)
                {
                    _Path = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override TabItemType tabType
        {
            get
            {
                return TabItemType.ColorList;
            }
        }

        private ObservableCollection<DominoStone> _DominoList;
        public ObservableCollection<DominoStone> DominoList
        {
            get { return _DominoList; }
            set
            {
                if (_DominoList != value)
                {
                    _DominoList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
    }
}