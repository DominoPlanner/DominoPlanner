using DominoPlanner.Core;
using DominoPlanner.Core.ColorMine.Comparisons;
using DominoPlanner.Usage.Serializer;
using DominoPlanner.Usage.UserControls.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

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
                            endung = ".dpfd";
                            CurrentViewModel = new AddFieldVM();
                            break;
                        case 1:
                            endung = ".dpffd";
                            CurrentViewModel = new FieldSizeVM(false);
                            break;
                        case 2:
                            endung = ".dpst";
                            CurrentViewModel = new AddStructureVM(StructureType.Rectangular);
                            break;
                        case 3:
                            endung = ".dpst";
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

                //hier muss die colorliste geladen werden, damit amn sie dann den FieldParametern hinzufügen kann
                Path.Combine(_ProjectPath, @"Planner Files\colors.dpcol");
                Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
                List<DominoColor> ColorList = new List<DominoColor>();
                ColorList.Add(new DominoColor(Colors.Black, 1000, "black"));
                ColorList.Add(new DominoColor(Colors.White, 1000, "white"));

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
                        BitmapImage bI = new BitmapImage(new Uri(((AddFieldVM)CurrentViewModel).pImage, UriKind.Relative));
                        WriteableBitmap wbi = new WriteableBitmap(bI);
                        FieldParameters p = new FieldParameters(wbi, ColorList, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.a, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.b, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.c, ((AddFieldVM)CurrentViewModel).fieldSizeVM.SelectedItem.Sizes.d, ((AddFieldVM)CurrentViewModel).fieldSizeVM.FieldSize, BitmapScalingMode.HighQuality, DitherMode.NoDithering, ColorDetectionMode.CieDe2000Comparison);

                        DominoTransfer t = p.Generate(progress);
                        //t.Save(); Was auch immer hier dann Übergeben werden kann und so
                        
                        break;
                    case 1: //Free Field
                        internPictureName = "";
                        break;
                    case 2: //Rectangular Structure
                        progress = new Progress<string>(pr => Console.WriteLine(pr));

                        internPictureName = string.Format("{0}{1}", filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath));
                        File.Copy(((AddStructureVM)CurrentViewModel).sPath, string.Format("{0}\\Source Image\\{1}{2}", _ProjectPath, filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath)));

                        //StreamReader sr = new StreamReader(new FileStream("Structures.xml", FileMode.Open));
                        //XElement xml = XElement.Parse(sr.ReadToEnd());
                        //new StructureParameters(((AddStructureVM)CurrentViewModel).pImage, xml.Elements().ElementAt(6), ((RectangularSizeVM)((AddStructureVM)CurrentViewModel).CurrentViewModel).StrucSize, ColorList, ColorDetectionMode.CieDe2000Comparison, AverageMode.Average, true);
                        //hier muss irgendwie noch was hin zum speichern :D
                        break;
                    case 3: //Round Structure
                        progress = new Progress<string>(pr => Console.WriteLine(pr));
                        internPictureName = string.Format("{0}{1}", filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath));
                        File.Copy(((AddStructureVM)CurrentViewModel).sPath, string.Format("{0}\\Source Image\\{1}{2}", _ProjectPath, filename, Path.GetExtension(((AddStructureVM)CurrentViewModel).sPath)));
                        RoundSizeVM rsvm = (RoundSizeVM)((AddStructureVM)CurrentViewModel).CurrentViewModel;
                        BitmapImage b = new BitmapImage(new Uri(((AddStructureVM)CurrentViewModel).pImage, UriKind.Relative));
                        WriteableBitmap wb = new WriteableBitmap(b);
                        new SpiralParameters(wb, rsvm.StrucSize, rsvm.dWidth, rsvm.dHeight, rsvm.beLines, rsvm.beDominoes, ColorList, ColorDetectionMode.CieDe2000Comparison, false, AverageMode.Corner);
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
