using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using Emgu.CV.CvEnum;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class CreateFieldVM : TabBaseVM
    {
        #region CTOR
        public CreateFieldVM(FieldNode dominoProvider) : base()
        {
            name = Path.GetFileNameWithoutExtension(dominoProvider.relativePath);
            CurrentProject = dominoProvider.obj;
            
            fsvm = new FieldSizeVM(true);
            OnlyOwnStonesVM = new OnlyOwnStonesVM(((UncoupledCalculation)fieldParameters.PrimaryCalculation).IterationInformation);

            iResizeMode = (int)((FieldReadout)fieldParameters.PrimaryImageTreatment).ResizeMode;
            iColorApproxMode = (int)((UncoupledCalculation)fieldParameters.PrimaryCalculation).ColorMode.colorComparisonMode;
            iDiffusionMode = (int)((UncoupledCalculation)fieldParameters.PrimaryCalculation).Dithering.Mode;
            TransparencyValue = ((UncoupledCalculation)fieldParameters.PrimaryCalculation).TransparencySetting;

            ReloadSizes();
            FillColorList();
            refresh();
            RefreshColorAmount();
            UnsavedChanges = false;
            BuildtoolsClick = new RelayCommand(o => { OpenBuildTools(); });
            EditClick = new RelayCommand(o => { fieldParameters.Editing = true; });
        }
        #endregion

        #region fields
        string name;
        int refrshCounter = 0;
        Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
        FieldParameters fieldParameters
        {
            get { return CurrentProject as FieldParameters; }
            set
            {
                if(CurrentProject != value)
                {
                    CurrentProject = value;
                }
            }
        }
        private DominoTransfer _dominoTransfer;

        public DominoTransfer dominoTransfer
        {
            get { return _dominoTransfer; }
            set
            {
                _dominoTransfer = value;
                refreshPlanPic();

                fsvm.PhysicalLength = dominoTransfer.physicalLength;
                fsvm.PhysicalHeight = dominoTransfer.physicalHeight;
            }
        }
        #endregion

        #region prope
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
        
        public override TabItemType tabType
        {
            get
            {
                return TabItemType.CreateField;
            }
        }

        private FieldSizeVM _fsvm;
        public FieldSizeVM fsvm
        {
            get { return _fsvm; }
            set
            {
                if (_fsvm != value)
                {
                    _fsvm = value;
                    RaisePropertyChanged();
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

        private void _onlyOwnStonesVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("OnlyUse"))
            {
                if (OnlyOwnStonesVM.OnlyUse)
                {
                    ((UncoupledCalculation)fieldParameters.PrimaryCalculation).IterationInformation = new IterativeColorRestriction(OnlyOwnStonesVM.Iterations, OnlyOwnStonesVM.Weight);
                    if (OnlyOwnStonesVM.Iterations == 0)
                    {
                        OnlyOwnStonesVM.Iterations = 2;
                        OnlyOwnStonesVM.Weight = 0.1;
                    }
                }
                else
                {
                    ((UncoupledCalculation)fieldParameters.PrimaryCalculation).IterationInformation = new NoColorRestriction();
                }
                refresh();
            }
            else if (e.PropertyName.Equals("Iterations"))
            {
                if (OnlyOwnStonesVM.OnlyUse)
                {
                    ((UncoupledCalculation)fieldParameters.PrimaryCalculation).IterationInformation.maxNumberOfIterations = OnlyOwnStonesVM.Iterations;
                    refresh();
                }
            }
            else if (e.PropertyName.Equals("Weight"))
            {
                if (OnlyOwnStonesVM.OnlyUse)
                {
                    ((IterativeColorRestriction)((UncoupledCalculation)fieldParameters.PrimaryCalculation).IterationInformation).iterationWeight = OnlyOwnStonesVM.Weight;
                    refresh();
                }
            }
        }

        private WriteableBitmap _CurrentPlan;
        public WriteableBitmap CurrentPlan
        {
            get { return _CurrentPlan; }
            set
            {
                if (_CurrentPlan != value)
                {
                    _CurrentPlan = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _sResizeMode;
        public string sResizeMode
        {
            get { return _sResizeMode; }
            set
            {
                if (_sResizeMode != value)
                {
                    _sResizeMode = value;
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

        private int _iResizeMode;
        public int iResizeMode
        {
            get { return _iResizeMode; }
            set
            {
                if (_iResizeMode != value || _sResizeMode == null)
                {
                    _iResizeMode = value;
                    ((FieldReadout)fieldParameters.PrimaryImageTreatment).ResizeMode = (Inter)value;
                    sResizeMode = ((FieldReadout)fieldParameters.PrimaryImageTreatment).ResizeMode.ToString();
                    RaisePropertyChanged();
                    refresh();
                }
            }
        }

        private int _iColorApproxMode;
        public int iColorApproxMode
        {
            get { return _iColorApproxMode; }
            set
            {
                if (_iColorApproxMode != value || sColorApproxMode == null)
                {
                    _iColorApproxMode = value;
                    switch (value)
                    {
                        case 0:
                            ((UncoupledCalculation)fieldParameters.PrimaryCalculation).ColorMode = ColorDetectionMode.Cie1976Comparison;
                            sColorApproxMode = "CIE-76 (ISO 12647)";
                            break;
                        case 1:
                            ((UncoupledCalculation)fieldParameters.PrimaryCalculation).ColorMode = ColorDetectionMode.CmcComparison;
                            sColorApproxMode = "CMC (l:c)";
                            break;
                        case 2:
                            ((UncoupledCalculation)fieldParameters.PrimaryCalculation).ColorMode = ColorDetectionMode.Cie94Comparison;
                            sColorApproxMode = "CIE-94 (DIN 99)";
                            break;
                        case 3:
                            ((UncoupledCalculation)fieldParameters.PrimaryCalculation).ColorMode = ColorDetectionMode.CieDe2000Comparison;
                            sColorApproxMode = "CIE-E-2000";
                            break;
                        default:
                            break;
                    }
                    RaisePropertyChanged();
                    refresh();
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
                    ((UncoupledCalculation)fieldParameters.PrimaryCalculation).TransparencySetting = _TransparencyValue;
                    refresh();
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
                    refresh();
                    RaisePropertyChanged();
                }
            }
        }
        private bool _collapsed;
        public bool Collapsed
        {
            get { return _collapsed; }
            set
            {
                if (_collapsed != value)
                {
                    _collapsed = value;
                    refresh();
                    RaisePropertyChanged();
                }
            }
        }
        private System.Windows.Media.Color _backgroundColor = System.Windows.Media.Color.FromArgb(0, 255, 255, 255);
        public System.Windows.Media.Color backgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    RaisePropertyChanged();
                    refresh();
                }
            }
        }

        private int _iDiffusionMode = 1;
        public int iDiffusionMode
        {
            get { return _iDiffusionMode; }
            set
            {
                if (_iDiffusionMode != value || _sDiffusionMode == null)
                {
                    _iDiffusionMode = value;
                    switch ((DitherMode)value)
                    {
                        case DitherMode.NoDithering:
                            sDiffusionMode = "NoDiffusion";
                            ((UncoupledCalculation)fieldParameters.PrimaryCalculation).Dithering = new Dithering();
                            break;
                        case DitherMode.FloydSteinberg:
                            sDiffusionMode = "Floyd/Steinberg Dithering";
                            ((UncoupledCalculation)fieldParameters.PrimaryCalculation).Dithering= new FloydSteinbergDithering();
                            break;
                        case DitherMode.JarvisJudiceNinke:
                            sDiffusionMode = "Jarvis/Judice/Ninke Dithering";
                            ((UncoupledCalculation)fieldParameters.PrimaryCalculation).Dithering = new JarvisJudiceNinkeDithering();
                            break;
                        case DitherMode.Stucki:
                            sDiffusionMode = "Stucki Dithering";
                            ((UncoupledCalculation)fieldParameters.PrimaryCalculation).Dithering= new StuckiDithering();
                            break;
                        default:
                            break;
                    }
                    RaisePropertyChanged();
                    refresh();
                }
            }
        }

        public System.Windows.Threading.Dispatcher dispatcher;
        private void refreshPlanPic()
        {
            System.Diagnostics.Debug.WriteLine(progress.ToString());
            if (dispatcher == null)
            {
                CurrentPlan = ImageConvert.ToWriteableBitmap(dominoTransfer.GenerateImage(backgroundColor, 2000, draw_borders, Collapsed).Bitmap);
                cursor = null;
            }
            else
            {
                dispatcher.BeginInvoke((Action)(() =>
                {
                    WriteableBitmap newBitmap = ImageConvert.ToWriteableBitmap(dominoTransfer.GenerateImage(backgroundColor, 2000, draw_borders, Collapsed).Bitmap);
                    CurrentPlan = newBitmap;
                    RefreshColorAmount();
                    cursor = null;
                }));
            }
        }

        CancellationToken ct;
        private async void refresh()
        {
            cursor = Cursors.Wait;
            refrshCounter++;
            Func<DominoTransfer> function = new Func<DominoTransfer>(() => fieldParameters.Generate(ct, progress));
            DominoTransfer dt = await Task.Factory.StartNew<DominoTransfer>(function);
            refrshCounter--;
            if(refrshCounter == 0)
            {   
                dominoTransfer = dt;
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

        public override bool Save()
        {
            try
            {
                fieldParameters.Save();
                UnsavedChanges = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void FillColorList()
        {
            OnlyOwnStonesVM.Colors = new System.Collections.ObjectModel.ObservableCollection<ColorListEntry>();
            
            int counter = 0;
            foreach (DominoColor domino in CurrentProject.colors.RepresentionForCalculation.OfType<DominoColor>())
            {
                OnlyOwnStonesVM.Colors.Add(new ColorListEntry() { DominoColor = domino, SortIndex = CurrentProject.colors.Anzeigeindizes[counter] });
                counter++;
            }

            if (CurrentProject.colors.RepresentionForCalculation.OfType<EmptyDomino>().Count() == 1)
            {
                OnlyOwnStonesVM.Colors.Add(new ColorListEntry() { DominoColor = CurrentProject.colors.RepresentionForCalculation.OfType<EmptyDomino>().First(), SortIndex = -1 });
            }
        }
        private void RefreshColorAmount()
        {
            bool fulfilled = true;
            for (int i = 0; i < OnlyOwnStonesVM.Colors.Count(); i++)
            {
                OnlyOwnStonesVM.Colors[i].ProjectCount.Clear();
                if (CurrentProject.Counts.Length > i + 1)
                {
                    OnlyOwnStonesVM.Colors[i].ProjectCount.Add(CurrentProject.Counts[i + 1]);
                    OnlyOwnStonesVM.Colors[i].Weight = (((UncoupledCalculation)CurrentProject.PrimaryCalculation).IterationInformation.weights[i + 1]);
                    if (OnlyOwnStonesVM.Colors[i].SumAll > OnlyOwnStonesVM.Colors[i].DominoColor.count)
                        fulfilled = false;
                }
                else
                {
                    OnlyOwnStonesVM.Colors[i].ProjectCount.Add(CurrentProject.Counts[0]);
                }
            }
            OnlyOwnStonesVM.ColorRestrictionFulfilled = fulfilled;
        }
        private void CreateFieldVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            bool important = false;
            if (e.PropertyName.Equals("FieldSize") || e.PropertyName.Equals("BindSize"))
            {
                fieldParameters.TargetCount = fsvm.FieldSize;
                important = true;
            }
            else if (e.PropertyName.Equals("Length"))
            {
                fieldParameters.Length = (int)fsvm.Length;
                if (fsvm.BindSize)
                {
                    double fieldWidth = fsvm.Length * (fieldParameters.HorizontalDistance + fieldParameters.HorizontalSize);
                    double stoneHeightWidhSpace = fieldParameters.VerticalDistance + fieldParameters.VerticalSize;
                    fieldParameters.Height = (int)(fieldWidth / (double)fieldParameters.PrimaryImageTreatment.Width * fieldParameters.PrimaryImageTreatment.Height / stoneHeightWidhSpace);
                }
                important = true;
            }
            else if (e.PropertyName.Equals("Height"))
            {
                fieldParameters.Height = (int)fsvm.Height;
                if (fsvm.BindSize)
                {
                    double fieldHeight = fsvm.Height * (fieldParameters.VerticalDistance + fieldParameters.VerticalSize);
                    double stoneWidthWidthSpace = fieldParameters.HorizontalDistance + fieldParameters.HorizontalSize;
                    fieldParameters.Length = (int)(fieldHeight / (double)fieldParameters.PrimaryImageTreatment.Height * fieldParameters.PrimaryImageTreatment.Width / stoneWidthWidthSpace);
                }
                important = true;
            }
            else if (e.PropertyName.Equals("SelectedItem"))
            {
                UpdateStoneSizes();
                important = true;
            }
            else if (e.PropertyName.Equals("Vertical"))
            {
                fieldParameters.FieldPlanDirection = Core.Orientation.Vertical;
                UpdateStoneSizes();
                important = true;
            }
            else if (e.PropertyName.Equals("Horizontal"))
            {
                fieldParameters.FieldPlanDirection = Core.Orientation.Horizontal;
                UpdateStoneSizes();
                important = true;
            }

            if (important)
            {
                refresh();
                ReloadSizes();
            }
        }

        private void UpdateStoneSizes()
        {
            if (fsvm.Vertical)
            {
                fieldParameters.HorizontalDistance = fsvm.SelectedItem.Sizes.d;
                fieldParameters.HorizontalSize = fsvm.SelectedItem.Sizes.c;
                fieldParameters.VerticalSize = fsvm.SelectedItem.Sizes.b;
                fieldParameters.VerticalDistance = fsvm.SelectedItem.Sizes.a;
            }
            else
            {
                fieldParameters.HorizontalDistance = fsvm.SelectedItem.Sizes.a;
                fieldParameters.HorizontalSize = fsvm.SelectedItem.Sizes.b;
                fieldParameters.VerticalSize = fsvm.SelectedItem.Sizes.c;
                fieldParameters.VerticalDistance = fsvm.SelectedItem.Sizes.d;
            }
        }

        private void Sizes_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateStoneSizes();
            refresh();
            ReloadSizes();
        }

        private void ReloadSizes()
        {
            fsvm.PropertyChanged -= CreateFieldVM_PropertyChanged;
            fsvm.FieldSize = fieldParameters.Height * fieldParameters.Length;
            fsvm.Length = fieldParameters.Length;
            fsvm.Height = fieldParameters.Height;
            fsvm.Horizontal = fieldParameters.FieldPlanDirection == Core.Orientation.Horizontal;
            Sizes currentSize = new Sizes(fieldParameters.HorizontalDistance, fieldParameters.HorizontalSize, fieldParameters.VerticalSize, fieldParameters.VerticalDistance);
            bool found = false;
            foreach (StandardSize sSize in fsvm.field_templates)
            {
                if (fsvm.Horizontal)
                {
                    if (sSize.Sizes.a == currentSize.a && sSize.Sizes.b == currentSize.b && sSize.Sizes.c == currentSize.c && sSize.Sizes.d == currentSize.d)
                    {
                        fsvm.SelectedItem = sSize;
                        found = true;
                        break;
                    }
                }
                else
                {
                    if (sSize.Sizes.a == currentSize.d && sSize.Sizes.b == currentSize.c && sSize.Sizes.c == currentSize.b && sSize.Sizes.d == currentSize.a)
                    {
                        fsvm.SelectedItem = sSize;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                fsvm.field_templates.Last<StandardSize>().Sizes = currentSize;
                fsvm.SelectedItem = fsvm.field_templates.Last<StandardSize>();
            }
            else
            {
                if (fsvm.SelectedItem.Name.Equals("User Size"))
                {
                    fsvm.SelectedItem.Sizes.PropertyChanged -= Sizes_PropertyChanged;
                    fsvm.SelectedItem.Sizes.PropertyChanged += Sizes_PropertyChanged;
                }
                else
                    fsvm.SelectedItem.Sizes.PropertyChanged -= Sizes_PropertyChanged;
            }

            this.fsvm.PropertyChanged += CreateFieldVM_PropertyChanged;
            UnsavedChanges = true;
        }

        private void OpenBuildTools()
        {
            ProtocolV protocolV = new ProtocolV();
            protocolV.DataContext = new ProtocolVM(fieldParameters, name);
            protocolV.ShowDialog();
        }
        #endregion

        #region Commands
		private ICommand _EditClick;
        public ICommand EditClick { get { return _EditClick; } set { if (value != _EditClick) { _EditClick = value; } } }

        private ICommand _BuildtoolsClick;
        public ICommand BuildtoolsClick { get { return _BuildtoolsClick; } set { if (value != _BuildtoolsClick) { _BuildtoolsClick = value; } } }
        #endregion
    }
}
