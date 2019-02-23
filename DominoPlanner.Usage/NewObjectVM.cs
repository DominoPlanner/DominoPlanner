﻿using DominoPlanner.Core;
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

        string picturePath;

        private int _selectedType;
        public int selectedType
        {
            get { return _selectedType; }
            set
            {
                if (_selectedType != value)
                {
                    switch (_selectedType)
                    {
                        case 0:
                            if(CurrentViewModel is AddFieldVM addField)
                            {
                                picturePath = addField.sPath;
                            }
                            break;
                        case 1:
                        case 2:
                            if(CurrentViewModel is AddStructureVM addStructureVM)
                            {
                                picturePath = addStructureVM.sPath;
                            }
                            break;
                    }
                    _selectedType = value;
                    switch (value)
                    {
                        case 0:
                            endung = ".dobject";
                            CurrentViewModel = new AddFieldVM();
                            ((AddFieldVM)CurrentViewModel).sPath = picturePath;
                            break;
                        /*case 1:
                            endung = ".dobject";
                            CurrentViewModel = new FieldSizeVM(false);
                            break;*/
                        case 1:
                            endung = ".dobject";
                            CurrentViewModel = new AddStructureVM(StructureType.Rectangular);
                            ((AddStructureVM)CurrentViewModel).sPath = picturePath;
                            break;
                        case 2:
                            endung = ".dobject";
                            CurrentViewModel = new AddStructureVM(StructureType.Round);
                            ((AddStructureVM)CurrentViewModel).sPath = picturePath;
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
                        MessageBox.Show("This name is already in use in this project.\n Please choose different Name.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                if (string.IsNullOrEmpty(filename) || string.IsNullOrWhiteSpace(filename))
                {
                    MessageBox.Show("You forgot to choose a name.", "Missing Values", MessageBoxButton.OK);
                    return;
                }
                string colorlist = parentProject.colorPath;
                string relResultPath = Path.Combine("Planner Files", string.Format("{0}.DObject", filename));
                string relColorList = $@"..\{colorlist}";
                string resultPath = Path.Combine(ProjectPath, relResultPath);

                if (selectedType >= 0 && selectedType <= 2)
                {
                    // project with image
                    string originalImagePath = picturePath;
                    
                    if (string.IsNullOrEmpty(originalImagePath) || string.IsNullOrWhiteSpace(originalImagePath))
                    {
                        MessageBox.Show("Please choose an image", "Missing Values", MessageBoxButton.OK);
                        return;
                    }

                    internPictureName = string.Format("{0}{1}", filename, Path.GetExtension(originalImagePath));
                    try
                    {
                        File.Copy(originalImagePath, Path.Combine(_ProjectPath, "Source Image", internPictureName));
                    }
                    catch (IOException es)
                    {
                        MessageBox.Show("Copying the image into the project folder failed.\nPlease check the permissions to this file.");
                        return;
                    }
                    string relPicturePath = $@"..\Source Image\{internPictureName}";
                    try
                    {
                        switch (selectedType)
                        {
                            case 0: //Field with Picture
                                var fieldVM = ((AddFieldVM)CurrentViewModel).fieldSizeVM;
                                FieldParameters p = new FieldParameters(resultPath, relPicturePath, relColorList,
                                    fieldVM.SelectedItem.Sizes.a, fieldVM.SelectedItem.Sizes.b, fieldVM.SelectedItem.Sizes.c,
                                    fieldVM.SelectedItem.Sizes.d, fieldVM.FieldSize, Emgu.CV.CvEnum.Inter.Lanczos4,
                                    ColorDetectionMode.CieDe2000Comparison, new Dithering(), new NoColorRestriction());
                                p.Generate();
                                p.Save();
                                resultNode = new FieldNode(relResultPath, parentProject);
                                break;
                            case 1: //Rectangular Structure
                                var structureVM = ((RectangularSizeVM)((AddStructureVM)CurrentViewModel).CurrentViewModel);
                                StructureParameters sp = new StructureParameters(resultPath, relPicturePath,
                                    structureVM.SelectedStructureElement, structureVM.StrucSize, relColorList,
                                    ColorDetectionMode.CieDe2000Comparison, new Dithering(), AverageMode.Corner, new NoColorRestriction());
                                sp.Generate();
                                sp.Save();
                                resultNode = new StructureNode(relResultPath, parentProject);
                                break;
                            case 2: //Round Structure
                                RoundSizeVM rsvm = (RoundSizeVM)((AddStructureVM)CurrentViewModel).CurrentViewModel;
                                CircularStructure circularStructure;
                                if (rsvm.TypeSelected.Equals("Spiral"))
                                {
                                    circularStructure = new SpiralParameters(resultPath, relPicturePath, rsvm.Amount,
                                        relColorList, ColorDetectionMode.CieDe2000Comparison, new Dithering(),
                                        AverageMode.Corner, new NoColorRestriction());
                                    circularStructure.Generate();
                                    circularStructure.Save();
                                    resultNode = new SpiralNode(relResultPath, parentProject);
                                }
                                else
                                {
                                    circularStructure = new CircleParameters(resultPath, relPicturePath, rsvm.Amount, relColorList,
                                        ColorDetectionMode.CieDe2000Comparison, new Dithering(), AverageMode.Corner, new NoColorRestriction());
                                    circularStructure.Generate();
                                    circularStructure.Save();
                                    resultNode = new CircleNode(relResultPath, parentProject);
                                }
                                break;
                            default: break;
                        }
                    }
                    catch(Exception ex)
                    {
                        File.Delete(Workspace.AbsolutePathFromReference(relResultPath, parentProject));
                        File.Delete(Path.Combine(_ProjectPath, "Source Image", internPictureName));
                        resultNode = null;
                        MessageBox.Show("Project creation failed. Error mesage: \n" + ex + "\n The created files have been deleted");
                    }
                    parentProject.Save();
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
