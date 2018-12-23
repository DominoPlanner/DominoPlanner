using DominoPlanner.Core;
using DominoPlanner.Usage.Serializer;
using DominoPlanner.Usage.UserControls.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage
{
    public class NewObjectVM : ModelBase
    {
        #region CTOR
        public NewObjectVM(string folderpath)
        {
            _ProjectPath = folderpath;
            CreateIt = new RelayCommand(o => { mCreateIt(); });
            selectedType = 7;
            selectedType = 0;
        }
        #endregion

        #region fields
        public string internPictureName { get; private set; }
        public int ObjectID { get; private set; }
        #endregion

        #region prop
        private string _ProjectPath;
        public string ProjectPath { get { return _ProjectPath; } }
        public string ObjectPath { get { return string.Format("{0}\\{1}{2}", _ProjectPath, _filename, _endung); } }

        private int _selectedType;
        public int selectedType
        {
            get { return _selectedType; }
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    switch (value)
                    {
                        case 0:
                            endung = ".dobject";
                            CurrentViewModel = new AddFieldVM();
                            break;
                        /*case 1:
                            endung = ".dobject";
                            CurrentViewModel = new FieldSizeVM(false);
                            break;*/
                        case 1:
                            endung = ".dobject";
                            CurrentViewModel = new AddStructureVM(StructureType.Rectangular);
                            break;
                        case 2:
                            endung = ".dobject";
                            CurrentViewModel = new AddStructureVM(StructureType.Round);
                            break;
                        default: break;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private string _filename;
        public string filename
        {
            get { return _filename; }
            set
            {
                if (_filename != value)
                {
                    _filename = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _endung;
        public string endung
        {
            get { return _endung; }
            set
            {
                if (_endung != value)
                {
                    _endung = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        private bool _Close;
        public bool Close
        {
            get { return _Close; }
            set
            {
                if (_Close != value)
                {
                    _Close = value;
                    CloseChanged(this, EventArgs.Empty);
                    RaisePropertyChanged();
                }
            }
        }

        public event EventHandler CloseChanged;

        #endregion

        #region command

        private ICommand _CreateIt;
        public ICommand CreateIt { get { return _CreateIt; } set { if (value != _CreateIt) { _CreateIt = value; } } }

        #endregion
        #region Methods
        private void mCreateIt()
        {
            try
            {
                if (string.IsNullOrEmpty(filename) || string.IsNullOrWhiteSpace(filename))
                {
                    MessageBox.Show("You forget to choose a name.", "Missing Values", MessageBoxButton.OK);
                    return;
                }
                string colorlist = @"C:\Users\johan\Desktop\colors.DColor";
                switch (selectedType)
                {
                    case 0: //Field with Picture
                        if (string.IsNullOrEmpty(((AddFieldVM)CurrentViewModel).sPath) || string.IsNullOrWhiteSpace(((AddFieldVM)CurrentViewModel).sPath))
                        {
                            MessageBox.Show("You forget to choose an image.", "Missing Values", MessageBoxButton.OK);
                            return;
                        }
                        
                        internPictureName = string.Format("{0}{1}", filename, Path.GetExtension(((AddFieldVM)CurrentViewModel).sPath));
                        File.Copy(((AddFieldVM)CurrentViewModel).sPath, string.Format("{0}\\Source Image\\{1}{2}", _ProjectPath, filename, Path.GetExtension(((AddFieldVM)CurrentViewModel).sPath)));
                        FieldParameters p = new FieldParameters(((AddFieldVM)CurrentViewModel).pImage, colorlist, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.a, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.b, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.c, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.d, ((AddFieldVM)CurrentViewModel).fieldSizeVM.FieldSize, Emgu.CV.CvEnum.Inter.Lanczos4, ColorDetectionMode.CieDe2000Comparison, new Dithering(), new NoColorRestriction());
                        p.Save(Path.Combine(this.ProjectPath, filename));
                        break;
                    /*case 1: //Free Field
                        internPictureName = "";
                        break;*/
                    case 1: //Rectangular Structure

                        internPictureName = string.Format("{0}{1}", filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath));
                        File.Copy(((AddStructureVM)CurrentViewModel).sPath, string.Format("{0}\\Source Image\\{1}{2}", _ProjectPath, filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath)));
                        
                        StructureParameters sp = new StructureParameters(((AddStructureVM)CurrentViewModel).pImage, ((RectangularSizeVM)((AddStructureVM)CurrentViewModel).CurrentViewModel).SelectedStructureElement, ((RectangularSizeVM)((AddStructureVM)CurrentViewModel).CurrentViewModel).StrucSize, colorlist, ColorDetectionMode.CieDe2000Comparison, new Dithering(), AverageMode.Corner, new NoColorRestriction());
                        sp.Save(Path.Combine(this.ProjectPath, filename));
                        break;
                    case 2: //Round Structure
                        internPictureName = string.Format("{0}{1}", filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath));
                        File.Copy(((AddStructureVM)CurrentViewModel).sPath, string.Format("{0}\\Source Image\\{1}{2}", _ProjectPath, filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath)));
                        RoundSizeVM rsvm = (RoundSizeVM)((AddStructureVM)CurrentViewModel).CurrentViewModel;
                        CircularStructure circularStructure;
                        if (rsvm.TypeSelected.Equals("Spiral"))
                        {
                            circularStructure = new SpiralParameters(((AddStructureVM)CurrentViewModel).pImage, rsvm.StrucSize, colorlist, ColorDetectionMode.CieDe2000Comparison, new Dithering(), AverageMode.Corner, new NoColorRestriction());
                        }
                        else
                        {

                            circularStructure = new CircleParameters(((AddStructureVM)CurrentViewModel).pImage, rsvm.StrucSize, colorlist, ColorDetectionMode.CieDe2000Comparison, new Dithering(), AverageMode.Corner, new NoColorRestriction());
                        }
                        circularStructure.Save(Path.Combine(this.ProjectPath, filename));
                        break;
                    default: break;
                }
                if(!string.IsNullOrEmpty(internPictureName))
                     ObjectID = ProjectSerializer.AddProject(_ProjectPath, filename + endung, internPictureName);

                Close = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Could not create a new Project!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
