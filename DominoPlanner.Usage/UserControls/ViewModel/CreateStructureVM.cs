using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class CreateStructureVM : TabBaseVM
    {
        #region CTOR
        public CreateStructureVM(bool rectangular) : base()
        {
            structureIsRectangular = rectangular;
            if (structureIsRectangular)
            {
                CurrentViewModel = new RectangularSizeVM();

                BitmapImage b = new BitmapImage(new Uri(@"D:\Pictures\HintergrundOrdner\TDT2016_Teamfoto.JPG", UriKind.Relative));
                WriteableBitmap wb = new WriteableBitmap(b);
                using (StreamReader sr = new StreamReader(new FileStream(@"D:\Dropbox\Dropbox\Structures.xml", FileMode.Open)))
                {
                    xElement = XElement.Parse(sr.ReadToEnd());
                    //structureParameters = new StructureParameters(wb, xElement.Elements().ElementAt(((RectangularSizeVM)CurrentViewModel).structure_index), 1500, 
                      //  new List<DominoColor>(), ColorDetectionMode.CieDe2000Comparison, AverageMode.Corner);
                    structureParameters = new StructureParameters(new Emgu.CV.Mat(), xElement.Elements().ElementAt(((RectangularSizeVM)CurrentViewModel).structure_index), 1500,
                        @"C:\Users\johan\Desktop\colors.DColor", ColorDetectionMode.CieDe2000Comparison, AverageMode.Corner, new NoColorRestriction());
                }
                ((RectangularSizeVM)CurrentViewModel).sLength = ((StructureParameters)structureParameters).length;
                ((RectangularSizeVM)CurrentViewModel).sHeight = ((StructureParameters)structureParameters).height;
            }
            else
            {
                CurrentViewModel = new RoundSizeVM();

                BitmapImage b = new BitmapImage(new Uri(@"D:\Pictures\HintergrundOrdner\TDT2016_Teamfoto.JPG", UriKind.Relative));
                WriteableBitmap wb = new WriteableBitmap(b);
                //structureParameters = new SpiralParameters(wb, 80, 24, 8, 8, 10, new List<DominoColor>(), ColorDetectionMode.CieDe2000Comparison, false, AverageMode.Corner);
                structureParameters = new SpiralParameters(new Emgu.CV.Mat(), 80, 24, 8, 8, 10, @"C:\Users\johan\Desktop\colors.DColor", ColorDetectionMode.CieDe2000Comparison, AverageMode.Corner, new NoColorRestriction());
                ((RoundSizeVM)CurrentViewModel).dWidth = ((SpiralParameters)structureParameters).normalWidth;
                ((RoundSizeVM)CurrentViewModel).dHeight = ((SpiralParameters)structureParameters).tangentialWidth;
                ((RoundSizeVM)CurrentViewModel).beLines = ((SpiralParameters)structureParameters).normalDistance;
                ((RoundSizeVM)CurrentViewModel).beDominoes = ((SpiralParameters)structureParameters).tangentialDistance;
            }
            structureParameters.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            structureParameters.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            structureParameters.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            structureParameters.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            structureParameters.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            structureParameters.colors.Add(new DominoColor(Colors.White, 1000, "white"));

            allow_stretch = structureParameters.allowStretch;
            SinglePixel = structureParameters.average == AverageMode.Corner;
            if (structureParameters.colorMode.GetType() == typeof(Cie1976Comparison))
                iColorApproxMode = 0;
            else if (structureParameters.colorMode.GetType() == typeof(CmcComparison))
                iColorApproxMode = 1;
            else if (structureParameters.colorMode.GetType() == typeof(Cie94Comparison))
                iColorApproxMode = 2;
            else
                iColorApproxMode = 3;
            CurrentViewModel.PropertyChanged += CurrentViewModel_PropertyChanged;
            draw_borders = false;
            Refresh();
            SourceImage = new WriteableBitmap(new BitmapImage(new Uri(@"D:\Pictures\HintergrundOrdner\TDT2016_Teamfoto.JPG", UriKind.RelativeOrAbsolute)));
            UnsavedChanges = false;
            this.PropertyChanged += CreateStructureVM_PropertyChanged;
            ShowFieldPlan = new RelayCommand(o => { FieldPlan(); });
        }
        #endregion

        #region fields
        private Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
        private IColorComparison cdMode;
        RectangleDominoProvider structureParameters;
        private bool structureIsRectangular;
        XElement xElement;
        #endregion

        #region prop
        public override TabItemType tabType
        {
            get
            {
                return TabItemType.CreateStructure;
            }
        }

        private Visibility _VisibleFieldplan;
        public Visibility VisibleFieldplan
        {
            get { return _VisibleFieldplan; }
            set
            {
                if (_VisibleFieldplan != value)
                {
                    _VisibleFieldplan = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WriteableBitmap _SourceImage;
        public WriteableBitmap SourceImage
        {
            get { return _SourceImage; }
            set
            {
                if (_SourceImage != value)
                {
                    _SourceImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WriteableBitmap _DestinationImage;
        public WriteableBitmap DestinationImage
        {
            get { return _DestinationImage; }
            set
            {
                if (_DestinationImage != value)
                {
                    _DestinationImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _allow_stretch;
        public bool allow_stretch
        {
            get { return _allow_stretch; }
            set
            {
                if (_allow_stretch != value)
                {
                    _allow_stretch = value;
                    structureParameters.allowStretch = value;
                    Refresh();
                    RaisePropertyChanged();
                }
            }
        }

        private bool _draw_borders;
        public bool draw_borders
        {
            get { return _draw_borders; }
            set
            {
                if (_draw_borders != value)
                {
                    _draw_borders = value;
                    Refresh();
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SinglePixel;
        public bool SinglePixel
        {
            get { return _SinglePixel; }
            set
            {
                if (_SinglePixel != value)
                {
                    _SinglePixel = value;
                    if (value)
                    {
                        structureParameters.average = AverageMode.Corner;
                        Refresh();
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public bool AverageArea
        {
            get { return !_SinglePixel; }
            set
            {
                if (_SinglePixel == value)
                {
                    _SinglePixel = !value;
                    if (value)
                    {
                        structureParameters.average = AverageMode.Average;
                        Refresh();
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private StructureViewModel _CurrentViewModel;
        public StructureViewModel CurrentViewModel
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

        private string _sColorApproxMode;
        public string sColorApproxMode
        {
            get { return _sColorApproxMode; }
            set
            {
                if (_sColorApproxMode != value)
                {
                    _sColorApproxMode = value;
                    Refresh();
                    RaisePropertyChanged();
                }
            }
        }

        private int _iColorApproxMode;
        public int iColorApproxMode
        {
            get { return _iColorApproxMode; }
            set
            {
                if (_iColorApproxMode != value)
                {
                    _iColorApproxMode = value;
                    switch (value)
                    {
                        case 0:
                            cdMode = ColorDetectionMode.Cie1976Comparison;
                            sColorApproxMode = "CIE-76 Comparison (ISO 12647)";
                            structureParameters.colorMode = ColorDetectionMode.Cie1976Comparison;
                            break;
                        case 1:
                            cdMode = ColorDetectionMode.CmcComparison;
                            structureParameters.colorMode = ColorDetectionMode.CmcComparison;
                            sColorApproxMode = "CMC (l:c) Comparison";
                            break;
                        case 2:
                            cdMode = ColorDetectionMode.Cie94Comparison;
                            structureParameters.colorMode = ColorDetectionMode.Cie94Comparison;
                            sColorApproxMode = "CIE-94 Comparison (DIN 99)";
                            break;
                        case 3:
                            cdMode = ColorDetectionMode.CieDe2000Comparison;
                            structureParameters.colorMode = ColorDetectionMode.CieDe2000Comparison;
                            sColorApproxMode = "CIE-E-2000 Comparison";
                            break;
                        default:
                            break;
                    }
                    RaisePropertyChanged();
                }
            }
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
        private void CreateStructureVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Refresh();
        }
        private void Refresh()
        {
            try
            {
                DominoTransfer t = structureParameters.Generate(progress);
                CurrentViewModel.StrucSize = t.dominoes.Count();
                DestinationImage = ImageConvert.ToWriteableBitmap(t.GenerateImage(2000, draw_borders).Bitmap);
                if (structureParameters.hasProcotolDefinition)
                    VisibleFieldplan = Visibility.Visible;
                else
                    VisibleFieldplan = Visibility.Hidden;
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("adf");
            }
        }
        public override bool Save()
        {
            throw new NotImplementedException();
        }
        private void FieldPlan()
        {
            ProtocolV protocolV = new ProtocolV();
            protocolV.DataContext = new ProtocolVM(structureParameters);
            protocolV.ShowDialog();
        }
        private void CurrentViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            bool changed = false;
            if (sender.GetType() == typeof(RectangularSizeVM))
            {
                if (e.PropertyName.Equals("sLength"))
                {
                    ((StructureParameters)structureParameters).length = ((RectangularSizeVM)CurrentViewModel).sLength;
                    changed = true;
                }
                else if (e.PropertyName.Equals("sHeight"))
                {
                    ((StructureParameters)structureParameters).height = ((RectangularSizeVM)CurrentViewModel).sHeight;
                    changed = true;
                }
                else if (e.PropertyName.Equals("structure_index"))
                {
                    ((StructureParameters)structureParameters).structureDefinitionXML = xElement.Elements().ElementAt(((RectangularSizeVM)CurrentViewModel).structure_index);
                    changed = true;
                }
            }
            else
            {
                if (e.PropertyName.Equals("dWidth"))
                {
                    ((SpiralParameters)structureParameters).normalWidth = ((RoundSizeVM)CurrentViewModel).dWidth;
                    changed = true;
                }
                else if (e.PropertyName.Equals("dHeight"))
                {
                    ((SpiralParameters)structureParameters).tangentialWidth = ((RoundSizeVM)CurrentViewModel).dHeight;
                    changed = true;
                }
                else if (e.PropertyName.Equals("beLines"))
                {
                    ((SpiralParameters)structureParameters).normalDistance = ((RoundSizeVM)CurrentViewModel).beLines;
                    changed = true;
                }
                else if (e.PropertyName.Equals("beDominoes"))
                {
                    ((SpiralParameters)structureParameters).tangentialDistance = ((RoundSizeVM)CurrentViewModel).beDominoes;
                    changed = true;
                }
            }
            if (changed)
                Refresh();
        }
        #endregion

        #region Commands

        private ICommand _ShowFieldPlan;
        public ICommand ShowFieldPlan { get { return _ShowFieldPlan; } set { if (value != _ShowFieldPlan) { _ShowFieldPlan = value; } } }

        #endregion
    }
}
