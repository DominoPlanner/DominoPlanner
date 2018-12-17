using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class CreateStructureVM : TabBaseVM
    {
        #region CTOR
        public CreateStructureVM(bool rectangular) : base()
        {
            OnlyOwnStonesVM = new OnlyOwnStonesVM();
            structureIsRectangular = rectangular;
            if (structureIsRectangular)
            {
                CurrentViewModel = new RectangularSizeVM();

                structureParameters = new StructureParameters(@"C:\Users\johan\Pictures\Screenshots\Screenshot (5).png", CurrentViewModel.SelectedStructureElement, 1500,
                    @"C:\Users\johan\Desktop\colors.DColor", ColorDetectionMode.CieDe2000Comparison, AverageMode.Corner, new NoColorRestriction());
                ((RectangularSizeVM)CurrentViewModel).sLength = ((StructureParameters)structureParameters).length;
                ((RectangularSizeVM)CurrentViewModel).sHeight = ((StructureParameters)structureParameters).height;
            }
            else
            {
                CurrentViewModel = new RoundSizeVM();

                //structureParameters = new SpiralParameters(wb, 80, 24, 8, 8, 10, new List<DominoColor>(), ColorDetectionMode.CieDe2000Comparison, false, AverageMode.Corner);
                //structureParameters = new SpiralParameters("..\\..\\Icons\\colorPicker.ico", 80, 24, 8, 8, 10, @"C:\Users\johan\Desktop\colors.DColor", ColorDetectionMode.CieDe2000Comparison, AverageMode.Corner, new NoColorRestriction());
                ((RoundSizeVM)CurrentViewModel).dWidth = ((SpiralParameters)structureParameters).NormalWidth;
                ((RoundSizeVM)CurrentViewModel).dHeight = ((SpiralParameters)structureParameters).TangentialWidth;
                ((RoundSizeVM)CurrentViewModel).beLines = ((SpiralParameters)structureParameters).NormalDistance;
                ((RoundSizeVM)CurrentViewModel).beDominoes = ((SpiralParameters)structureParameters).TangentialDistance;
            }

            allow_stretch = structureParameters.allowStretch;
            SinglePixel = structureParameters.average == AverageMode.Corner;
            
            iColorApproxMode = (int)structureParameters.colorMode.colorComparisonMode;
            
            CurrentViewModel.PropertyChanged += CurrentViewModel_PropertyChanged;
            draw_borders = true;
            Refresh();
            UnsavedChanges = false;
            //PropertyChanged += CreateStructureVM_PropertyChanged;
            ShowFieldPlan = new RelayCommand(o => { FieldPlan(); });
        }
        #endregion

        #region fields
        private Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
        RectangleDominoProvider structureParameters;
        private bool structureIsRectangular;
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

        private string _sDiffusionMode;
        public string sDiffusionMode
        {
            get { return _sDiffusionMode; }
            set
            {
                if (_sDiffusionMode != value)
                {
                    _sDiffusionMode = value;
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
                            sColorApproxMode = "CIE-76 Comparison (ISO 12647)";
                            structureParameters.colorMode = ColorDetectionMode.Cie1976Comparison;
                            break;
                        case 1:
                            structureParameters.colorMode = ColorDetectionMode.CmcComparison;
                            sColorApproxMode = "CMC (l:c) Comparison";
                            break;
                        case 2:
                            structureParameters.colorMode = ColorDetectionMode.Cie94Comparison;
                            sColorApproxMode = "CIE-94 Comparison (DIN 99)";
                            break;
                        case 3:
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

        /*private int _iDiffusionMode = 1;
        public int iDiffusionMode
        {
            get { return _iDiffusionMode; }
            set
            {
                if (_iDiffusionMode != value)
                {
                    _iDiffusionMode = value;
                    switch ((DitherMode)value)
                    {
                        case DitherMode.NoDithering:
                            sDiffusionMode = "NoDiffusion";
                            structureParameters. = new Dithering();
                            break;
                        case DitherMode.FloydSteinberg:
                            sDiffusionMode = "Floyd/Steinberg Dithering";
                            fParameters.ditherMode = new FloydSteinbergDithering();
                            break;
                        case DitherMode.JarvisJudiceNinke:
                            sDiffusionMode = "Jarvis/Judice/Ninke Dithering";
                            fParameters.ditherMode = new JarvisJudiceNinkeDithering();
                            break;
                        case DitherMode.Stucki:
                            sDiffusionMode = "Stucki Dithering";
                            fParameters.ditherMode = new StuckiDithering();
                            break;
                        default:
                            break;
                    }
                    RaisePropertyChanged();
                    updateField();
                }
            }
        }*/

        private OnlyOwnStonesVM _onlyOwnStonesVM;

        public OnlyOwnStonesVM OnlyOwnStonesVM
        {
            get { return _onlyOwnStonesVM; }
            set
            {
                if (_onlyOwnStonesVM != value)
                {
                    if (_onlyOwnStonesVM != null)
                    {
                        _onlyOwnStonesVM.PropertyChanged -= _onlyOwnStonesVM_PropertyChanged;
                    }
                    _onlyOwnStonesVM = value;
                    RaisePropertyChanged();
                    _onlyOwnStonesVM.PropertyChanged += _onlyOwnStonesVM_PropertyChanged;
                }
            }
        }

        private void _onlyOwnStonesVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("OnlyUse"))
            {
                if (OnlyOwnStonesVM.OnlyUse)
                {
                    structureParameters.IterationInformation = new IterativeColorRestriction(OnlyOwnStonesVM.Iterations, OnlyOwnStonesVM.Weight);
                }
                else
                {
                    structureParameters.IterationInformation = new NoColorRestriction();
                }
                Refresh();
            }
            else if (e.PropertyName.Equals("Iterations"))
            {
                if (OnlyOwnStonesVM.OnlyUse)
                {
                    structureParameters.IterationInformation.maxNumberOfIterations = OnlyOwnStonesVM.Iterations;
                    Refresh();
                }
            }
            else if (e.PropertyName.Equals("Weight"))
            {
                if (OnlyOwnStonesVM.OnlyUse)
                {
                    ((IterativeColorRestriction)structureParameters.IterationInformation).iterationWeight = OnlyOwnStonesVM.Weight;
                    Refresh();
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
                System.Diagnostics.Debug.WriteLine("asdf");
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
                    ((StructureParameters)structureParameters).structureDefinitionXML = ((RectangularSizeVM)CurrentViewModel).SelectedStructureElement;
                    changed = true;
                }
            }
            else
            {
                if (e.PropertyName.Equals("dWidth"))
                {
                    ((SpiralParameters)structureParameters).NormalWidth = ((RoundSizeVM)CurrentViewModel).dWidth;
                    changed = true;
                }
                else if (e.PropertyName.Equals("dHeight"))
                {
                    ((SpiralParameters)structureParameters).TangentialWidth = ((RoundSizeVM)CurrentViewModel).dHeight;
                    changed = true;
                }
                else if (e.PropertyName.Equals("beLines"))
                {
                    ((SpiralParameters)structureParameters).NormalDistance = ((RoundSizeVM)CurrentViewModel).beLines;
                    changed = true;
                }
                else if (e.PropertyName.Equals("beDominoes"))
                {
                    ((SpiralParameters)structureParameters).TangentialDistance = ((RoundSizeVM)CurrentViewModel).beDominoes;
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
