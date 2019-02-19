using DominoPlanner.Core;
using DominoPlanner.Usage.Serializer;
using DominoPlanner.Usage.UserControls.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace DominoPlanner.Usage
{
    public class NewObjectVM : ModelBase
    {
        #region CTOR
        public NewObjectVM(string folderpath, DominoAssembly parentProject)
        {
            this.parentProject = parentProject;
            _ProjectPath = folderpath;
            CreateIt = new RelayCommand(o => { mCreateIt(); });
            selectedType = 7;
            selectedType = 0;
        }
        #endregion

        #region fields
        public string internPictureName { get; private set; }
        public int ObjectID { get; private set; }
        private DominoAssembly parentProject;
        #endregion

        #region prop
        private DocumentNode resultNode;
        public DocumentNode ResultNode
        {
            get { return resultNode; }
        }
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
                foreach (DocumentNode dc in parentProject.children)
                {
                    if (filename == Path.GetFileNameWithoutExtension(dc.relativePath))
                    {
                        MessageBox.Show("Please choose a name which is not in this project!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                if (string.IsNullOrEmpty(filename) || string.IsNullOrWhiteSpace(filename))
                {
                    MessageBox.Show("You forget to choose a name.", "Missing Values", MessageBoxButton.OK);
                    return;
                }
                string colorlist = parentProject.colorPath;
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
                        FieldParameters p = new FieldParameters(Path.Combine(ProjectPath, "Planner Files", string.Format("{0}.DObject", filename)), ((AddFieldVM)CurrentViewModel).pImage, colorlist, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.a, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.b, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.c, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.d, ((AddFieldVM)CurrentViewModel).fieldSizeVM.FieldSize, Emgu.CV.CvEnum.Inter.Lanczos4, ColorDetectionMode.CieDe2000Comparison, new Dithering(), new NoColorRestriction());
                        p.Save();
                        resultNode = new FieldNode(Path.Combine("Planner Files", string.Format("{0}.DObject", filename)), parentProject);
                        parentProject.Save();
                        break;
                    case 1: //Rectangular Structure
                        if (string.IsNullOrEmpty(((AddStructureVM)CurrentViewModel).sPath) || string.IsNullOrWhiteSpace(((AddStructureVM)CurrentViewModel).sPath))
                        {
                            MessageBox.Show("You forget to choose an image.", "Missing Values", MessageBoxButton.OK);
                            return;
                        }
                        internPictureName = string.Format("{0}{1}", filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath));
                        try
                        {
                            File.Copy(((AddStructureVM)CurrentViewModel).sPath, string.Format("{0}\\Source Image\\{1}{2}", _ProjectPath, filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath)));
                        }
                        catch (IOException es) { }
                        StructureParameters sp = new StructureParameters(Path.Combine(ProjectPath, "Planner Files", string.Format("{0}.DObject", filename)), ((AddStructureVM)CurrentViewModel).pImage, ((RectangularSizeVM)((AddStructureVM)CurrentViewModel).CurrentViewModel).SelectedStructureElement, ((RectangularSizeVM)((AddStructureVM)CurrentViewModel).CurrentViewModel).sLength, ((RectangularSizeVM)((AddStructureVM)CurrentViewModel).CurrentViewModel).sHeight, colorlist, ColorDetectionMode.CieDe2000Comparison, new Dithering(), AverageMode.Corner, new NoColorRestriction());
                        sp.Save();
                        resultNode = new StructureNode(Path.Combine("Planner Files", string.Format("{0}.DObject", filename)), parentProject);
                        parentProject.Save();
                        break;
                    case 2: //Round Structure
                        internPictureName = string.Format("{0}{1}", filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath));
                        File.Copy(((AddStructureVM)CurrentViewModel).sPath, string.Format("{0}\\Source Image\\{1}{2}", _ProjectPath, filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath)));
                        RoundSizeVM rsvm = (RoundSizeVM)((AddStructureVM)CurrentViewModel).CurrentViewModel;
                        CircularStructure circularStructure;
                        if (rsvm.TypeSelected.Equals("Spiral"))
                        {
                            circularStructure = new SpiralParameters(Path.Combine(ProjectPath, "Planner Files", string.Format("{0}.DObject", filename)), ((AddStructureVM)CurrentViewModel).pImage, rsvm.Amount, colorlist, ColorDetectionMode.CieDe2000Comparison, new Dithering(), AverageMode.Corner, new NoColorRestriction());
                            circularStructure.Save();
                            resultNode = new SpiralNode(Path.Combine("Planner Files", string.Format("{0}.DObject", filename)), parentProject);
                        }
                        else
                        {
                            circularStructure = new CircleParameters(Path.Combine(ProjectPath, "Planner Files", string.Format("{0}.DObject", filename)), ((AddStructureVM)CurrentViewModel).pImage, rsvm.Amount, colorlist, ColorDetectionMode.CieDe2000Comparison, new Dithering(), AverageMode.Corner, new NoColorRestriction());
                            circularStructure.Save();
                            resultNode = new CircleNode(Path.Combine("Planner Files", string.Format("{0}.DObject", filename)), parentProject);
                        }
                        parentProject.Save();
                        break;
                    default: break;
                }

                Close = true;
            }
            catch (Exception es)
            {
                MessageBox.Show("Could not create a new Project!" + "\n" + es + "\n" + es.InnerException + "\n" + es.StackTrace, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
