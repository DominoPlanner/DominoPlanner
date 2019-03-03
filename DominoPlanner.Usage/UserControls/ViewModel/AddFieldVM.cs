using Microsoft.Win32;
using System.Windows.Input;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class AddFieldVM : ModelBase
    {
        #region CTOR
        public AddFieldVM()
        {
            fieldSizeVM = new FieldSizeVM(false);
            pImage = "/icons/add.ico";
            LoadNewImage = new RelayCommand(o => { SetNewImage(); });
        }
        #endregion
        #region prop
        private FieldSizeVM _fieldSizeVM;
        public FieldSizeVM fieldSizeVM
        {
            get { return _fieldSizeVM; }
            set
            {
                if (_fieldSizeVM != value)
                {
                    _fieldSizeVM = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Image;
        public string pImage
        {
            get { return _Image; }
            set
            {
                if (_Image != value)
                {
                    _Image = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Path;
        public string sPath
        {
            get { return _Path; }
            set
            {
                if (_Path != value)
                {
                    _Path = value;
                    pImage = _Path;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Command
        private ICommand _LoadNewImage;
        public ICommand LoadNewImage { get { return _LoadNewImage; } set { if (value != _LoadNewImage) { _LoadNewImage = value; } } }
        #endregion

        #region Methods

        private void SetNewImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                sPath = openFileDialog.FileName;
                pImage = sPath;
            }
        }

        
        #endregion
    }
}
