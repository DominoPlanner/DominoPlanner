using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    enum StructureType
    {
        Round,
        Rectangular
    }

    class AddStructureVM : ModelBase
    {
        #region CTOR
        public AddStructureVM(StructureType structType)
        {
            LoadNewImage = new RelayCommand(o => { SetNewImage(); });
            pImage = "/icons/add.ico";
            sPath = "";
            structTyp = new List<string>();
            structTyp.Add("Rectangular Structure");
            structTyp.Add("Round Structure");
            switch (structType)
            {
                case StructureType.Round:
                    CurrentViewModel = new RoundSizeVM() { PossibleTypeChange = true };
                    break;
                case StructureType.Rectangular:
                    CurrentViewModel = new RectangularSizeVM() { StandAlone = true, StrucSize = 2000 } ;
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region
        #endregion
        #region prop
        private ModelBase _CurrentViewModel;
        public ModelBase CurrentViewModel
        {
            get { return _CurrentViewModel; }
            set
            {
                if (_CurrentViewModel != value)
                {
                    _CurrentViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<string> _structType;
        public List<string> structTyp
        {
            get { return _structType; }
            set
            {
                if (_structType != value)
                {
                    _structType = value;
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
                    if(!string.IsNullOrWhiteSpace(sPath))
                        pImage = sPath; 
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
