using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class CreateStructureVM : TabBaseVM
    {
        #region CTOR
        public CreateStructureVM(IDominoProvider dominoProvider, bool rectangular) : base()
        {
            CurrentProject = dominoProvider;
            OnlyOwnStonesVM = new OnlyOwnStonesVM();
            structureIsRectangular = rectangular;

            if (structureIsRectangular)
            {
                CurrentViewModel = new RectangularSizeVM();

                ((RectangularSizeVM)CurrentViewModel).sLength = ((StructureParameters)structureParameters).length;
                ((RectangularSizeVM)CurrentViewModel).sHeight = ((StructureParameters)structureParameters).height;
            }
            else
            {
                CurrentViewModel = new RoundSizeVM() { PossibleTypeChange = false };
                if (structureParameters is SpiralParameters sp)
                {
                    ((RoundSizeVM)CurrentViewModel).Amount = (int)((SpiralParameters)structureParameters).QuarterRotations;
                }
                else if (structureParameters is CircleParameters cp)
                {
                    ((RoundSizeVM)CurrentViewModel).Amount = cp.Circles;
                }
                ((RoundSizeVM)CurrentViewModel).dWidth = ((CircularStructure)structureParameters).DominoWidth;
                ((RoundSizeVM)CurrentViewModel).dHeight = ((CircularStructure)structureParameters).DominoLength;
                ((RoundSizeVM)CurrentViewModel).beLines = ((CircularStructure)structureParameters).NormalDistance;
                ((RoundSizeVM)CurrentViewModel).beDominoes = ((CircularStructure)structureParameters).TangentialDistance;
            }

            allow_stretch = structureParameters.allowStretch;
            SinglePixel = structureParameters.average == AverageMode.Corner;
            AverageArea = !SinglePixel;
            iDiffusionMode = (int)structureParameters.ditherMode.Mode;
            iColorApproxMode = (int)structureParameters.colorMode.colorComparisonMode;
            TransparencyValue = structureParameters.TransparencySetting;

            CurrentViewModel.PropertyChanged += CurrentViewModel_PropertyChanged;
            draw_borders = true;
            Refresh();
            UnsavedChanges = false;
            ShowFieldPlan = new RelayCommand(o => { FieldPlan(); });
            EditClick = new RelayCommand(o => { CurrentProject.Editing = true; });
        }
        #endregion

        #region fields
        public System.Windows.Threading.Dispatcher dispatcher;
        private Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
        private GeneralShapesProvider structureParameters
        {
            get { return CurrentProject as GeneralShapesProvider; }
            set
            {
                if (CurrentProject != value)
                {
                    CurrentProject = value;
                    if (structureParameters != null)
                    {
                        if (structureParameters.hasProcotolDefinition)
                            VisibleFieldplan = Visibility.Visible;
                        else
                            VisibleFieldplan = Visibility.Hidden;
                    }
                }
            }
        }
        private bool structureIsRectangular;
        private DominoTransfer _dominoTransfer;

        private DominoTransfer dominoTransfer
        {
            get { return _dominoTransfer; }
            set
            {
                _dominoTransfer = value;
                refreshPlanPic();
            }
        }
        #endregion

        #region prop
        private Cursor _cursorState;
        public Cursor cursor
        {
            get { return _cursorState; }
            set
            {
                if (_cursorState != value)
                {
                    _cursorState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private byte _TransparencyValue;
        public byte TransparencyValue
        {
            get { return _TransparencyValue; }
            set
            {
                if (_TransparencyValue != value)
                {
                    _TransparencyValue = value;
                    structureParameters.TransparencySetting = _TransparencyValue;
                    Refresh();
                    RaisePropertyChanged();
                }
            }
        }

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

        private int _iDiffusionMode = 1;
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
                            structureParameters.ditherMode = new Dithering();
                            break;
                        case DitherMode.FloydSteinberg:
                            sDiffusionMode = "Floyd/Steinberg Dithering";
                            structureParameters.ditherMode = new FloydSteinbergDithering();
                            break;
                        case DitherMode.JarvisJudiceNinke:
                            sDiffusionMode = "Jarvis/Judice/Ninke Dithering";
                            structureParameters.ditherMode = new JarvisJudiceNinkeDithering();
                            break;
                        case DitherMode.Stucki:
                            sDiffusionMode = "Stucki Dithering";
                            structureParameters.ditherMode = new StuckiDithering();
                            break;
                        default:
                            break;
                    }
                    RaisePropertyChanged();
                    Refresh();
                }
            }
        }

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

        private System.Windows.Media.Color _backgroundColor;
        public System.Windows.Media.Color backgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    RaisePropertyChanged();
                    Refresh();
                }
            }
        }
        #endregion

        #region Methods
        private void refreshPlanPic()
        {
            if (dispatcher == null)
            {
                CurrentViewModel.StrucSize = dominoTransfer.shapes.Count();
                DestinationImage = ImageConvert.ToWriteableBitmap(dominoTransfer.GenerateImage(backgroundColor, 2000, draw_borders).Bitmap);
                cursor = null;
            }
            else
            {
                dispatcher.BeginInvoke((Action)(() =>
                {
                    CurrentViewModel.StrucSize = dominoTransfer.shapes.Count();
                    DestinationImage = ImageConvert.ToWriteableBitmap(dominoTransfer.GenerateImage(backgroundColor, 2000, draw_borders).Bitmap);
                    cursor = null;
                }));
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
        public override void Undo()
        {
            throw new NotImplementedException();
        }

        public override void Redo()
        {
            throw new NotImplementedException();
        }

        private async void Refresh()
        {
            CurrentViewModel.PropertyChanged -= CurrentViewModel_PropertyChanged;
            try
            {
                cursor = Cursors.Wait;
                Func<DominoTransfer> function = new Func<DominoTransfer>(() => structureParameters.Generate(progress));
                dominoTransfer = await Task.Factory.StartNew<DominoTransfer>(function);
                if (structureParameters is StructureParameters sp)
                {
                    if (CurrentViewModel is RectangularSizeVM rs)
                    {
                        rs.sHeight = sp.height;
                        rs.sLength = sp.length;
                    }
                }
            }
            catch (Exception ex)
            {
                cursor = null;
            }
            CurrentViewModel.PropertyChanged += CurrentViewModel_PropertyChanged;
        }
        public override bool Save()
        {
            try
            {
                structureParameters.Save();
                UnsavedChanges = false;
                return true;
            }
            catch (Exception rd)
            {
                return false;
            }
        }
        private void FieldPlan()
        {
            ProtocolV protocolV = new ProtocolV
            {
                DataContext = new ProtocolVM(structureParameters, Path.GetFileNameWithoutExtension(this.FilePath))
            };
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

                    if (structureParameters != null)
                    {
                        if (structureParameters.hasProcotolDefinition)
                            VisibleFieldplan = Visibility.Visible;
                        else
                            VisibleFieldplan = Visibility.Hidden;
                    }

                    changed = true;
                }
                else if (e.PropertyName.Equals("StrucSize"))
                {
                    ((StructureParameters)structureParameters).TargetCount = ((RectangularSizeVM)CurrentViewModel).StrucSize;
                    changed = true;
                }
            }
            else
            {
                if (e.PropertyName.Equals("dWidth"))
                {
                    ((CircularStructure)structureParameters).DominoWidth = ((RoundSizeVM)CurrentViewModel).dWidth;
                    changed = true;
                }
                else if (e.PropertyName.Equals("dHeight"))
                {
                    ((CircularStructure)structureParameters).DominoLength = ((RoundSizeVM)CurrentViewModel).dHeight;
                    changed = true;
                }
                else if (e.PropertyName.Equals("beLines"))
                {
                    ((CircularStructure)structureParameters).NormalDistance = ((RoundSizeVM)CurrentViewModel).beLines;
                    changed = true;
                }
                else if (e.PropertyName.Equals("beDominoes"))
                {
                    ((CircularStructure)structureParameters).TangentialDistance = ((RoundSizeVM)CurrentViewModel).beDominoes;
                    changed = true;
                }
                else if (e.PropertyName.Equals("Amount"))
                {
                    if (structureParameters is SpiralParameters sp)
                        sp.QuarterRotations = ((RoundSizeVM)CurrentViewModel).Amount;
                    else if (structureParameters is CircleParameters cp)
                        cp.Circles = ((RoundSizeVM)CurrentViewModel).Amount;
                    changed = true;
                }
                else if (e.PropertyName.Equals("TypeSelected"))
                {
                    if (!structureParameters.ImageFilters.Any(x => x is BlendFileFilter)) { return; }
                    string filepath = structureParameters.ImageFilters.OfType<BlendFileFilter>().First().FilePath;
                    /*jojo
                    if (((RoundSizeVM)CurrentViewModel).TypeSelected.Equals("Spiral"))
                    {
                        structureParameters = new SpiralParameters(filepath, ((RoundSizeVM)CurrentViewModel).Amount / 4, structureParameters.ColorPath,
                    structureParameters.colorMode, structureParameters.ditherMode, structureParameters.average, structureParameters.IterationInformation);

                        ((RoundSizeVM)CurrentViewModel).Amount = (int)((SpiralParameters)structureParameters).QuarterRotations;
                    }
                    else
                    {
                        structureParameters = new CircleParameters(filepath, ((RoundSizeVM)CurrentViewModel).Amount * 4, structureParameters.ColorPath,
                    structureParameters.colorMode, structureParameters.ditherMode, structureParameters.average, structureParameters.IterationInformation);
                        ((RoundSizeVM)CurrentViewModel).Amount = (int)((CircleParameters)structureParameters).Circles;
                    }
                    */
                    ((RoundSizeVM)CurrentViewModel).dWidth = ((CircularStructure)structureParameters).DominoWidth;
                    ((RoundSizeVM)CurrentViewModel).dHeight = ((CircularStructure)structureParameters).DominoLength;
                    ((RoundSizeVM)CurrentViewModel).beLines = ((CircularStructure)structureParameters).NormalDistance;
                    ((RoundSizeVM)CurrentViewModel).beDominoes = ((CircularStructure)structureParameters).TangentialDistance;
                }
            }
            if (changed)
            {
                Refresh();
                UnsavedChanges = true;
            }
        }
        #endregion

        #region Commands
        private ICommand _EditClick;
        public ICommand EditClick { get { return _EditClick; } set { if (value != _EditClick) { _EditClick = value; } } }

        private ICommand _ShowFieldPlan;
        public ICommand ShowFieldPlan { get { return _ShowFieldPlan; } set { if (value != _ShowFieldPlan) { _ShowFieldPlan = value; } } }
        #endregion
    }
}
