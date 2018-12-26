using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using Emgu.CV.CvEnum;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class CreateFieldVM : TabBaseVM
    {
        #region CTOR
        public CreateFieldVM(string filePath) : base()
        {
            this.FilePath = filePath;
            fsvm = new FieldSizeVM(true);
            OnlyOwnStonesVM = new OnlyOwnStonesVM();
            
            fieldParameters = new FieldParameters(@"C:\Users\johan\Pictures\Screenshots\Screenshot (5).png", @"C:\Users\johan\Desktop\colors.DColor", 8, 8, 24, 8, 1500, Inter.Lanczos4, new CieDe2000Comparison(), new Dithering(), new NoColorRestriction());
            
            //fieldParameters =  Workspace.Load<FieldParameters>(FilePath);

            iResizeMode = (int)fieldParameters.resizeMode;
            iColorApproxMode = (int)fieldParameters.colorMode.colorComparisonMode;
            iDiffusionMode = (int)fieldParameters.ditherMode.Mode;
            TransparencyValue = fieldParameters.TransparencySetting;

            ReloadSizes();

            refresh();
            UnsavedChanges = false;
            BuildtoolsClick = new RelayCommand(o => { OpenBuildTools(); });
        }
        #endregion

        #region fields
        int refrshCounter = 0;
        Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
        FieldParameters fieldParameters;
        private DominoTransfer _dominoTransfer;

        public DominoTransfer dominoTransfer
        {
            get { return _dominoTransfer; }
            set
            {
                _dominoTransfer = value;
                refreshPlanPic();
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
                    fieldParameters.IterationInformation = new IterativeColorRestriction(OnlyOwnStonesVM.Iterations, OnlyOwnStonesVM.Weight);
                }
                else
                {
                    fieldParameters.IterationInformation = new NoColorRestriction();
                }
                refresh();
            }
            else if (e.PropertyName.Equals("Iterations"))
            {
                if (OnlyOwnStonesVM.OnlyUse)
                {
                    fieldParameters.IterationInformation.maxNumberOfIterations = OnlyOwnStonesVM.Iterations;
                    refresh();
                }
            }
            else if (e.PropertyName.Equals("Weight"))
            {
                if (OnlyOwnStonesVM.OnlyUse)
                {
                    ((IterativeColorRestriction)fieldParameters.IterationInformation).iterationWeight = OnlyOwnStonesVM.Weight;
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
                if (_iResizeMode != value)
                {
                    _iResizeMode = value;
                    fieldParameters.resizeMode = (Inter)value;
                    sResizeMode = fieldParameters.resizeMode.ToString();
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
                if (_iColorApproxMode != value)
                {
                    _iColorApproxMode = value;
                    switch (value)
                    {
                        case 0:
                            fieldParameters.colorMode = ColorDetectionMode.Cie1976Comparison;
                            sColorApproxMode = "CIE-76 Comparison (ISO 12647)";
                            break;
                        case 1:
                            fieldParameters.colorMode = ColorDetectionMode.CmcComparison;
                            sColorApproxMode = "CMC (l:c) Comparison";
                            break;
                        case 2:
                            fieldParameters.colorMode = ColorDetectionMode.Cie94Comparison;
                            sColorApproxMode = "CIE-94 Comparison (DIN 99)";
                            break;
                        case 3:
                            fieldParameters.colorMode = ColorDetectionMode.CieDe2000Comparison;
                            sColorApproxMode = "CIE-E-2000 Comparison";
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
                    fieldParameters.TransparencySetting = _TransparencyValue;
                    refresh();
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
                            fieldParameters.ditherMode = new Dithering();
                            break;
                        case DitherMode.FloydSteinberg:
                            sDiffusionMode = "Floyd/Steinberg Dithering";
                            fieldParameters.ditherMode = new FloydSteinbergDithering();
                            break;
                        case DitherMode.JarvisJudiceNinke:
                            sDiffusionMode = "Jarvis/Judice/Ninke Dithering";
                            fieldParameters.ditherMode = new JarvisJudiceNinkeDithering();
                            break;
                        case DitherMode.Stucki:
                            sDiffusionMode = "Stucki Dithering";
                            fieldParameters.ditherMode = new StuckiDithering();
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
                CurrentPlan = ImageConvert.ToWriteableBitmap(dominoTransfer.GenerateImage(2000).Bitmap);
                cursor = null;
            }
            else
            {
                dispatcher.BeginInvoke((Action)(() =>
                {
                    WriteableBitmap newBitmap = ImageConvert.ToWriteableBitmap(dominoTransfer.GenerateImage(2000).Bitmap);
                    CurrentPlan = newBitmap;
                    cursor = null;
                }));
            }
        }
        
        private async void refresh()
        {
            cursor = Cursors.Wait;
            refrshCounter++;
            Func<DominoTransfer> function = new Func<DominoTransfer>(() => fieldParameters.Generate(progress));
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
                fieldParameters.Save(FilePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
                fieldParameters.length = (int)fsvm.Length;
                if (fsvm.BindSize)
                {
                    double fieldWidth = fsvm.Length * (fieldParameters.a + fieldParameters.b);
                    double stoneHeightWidhSpace = fieldParameters.c + fieldParameters.d;
                    fieldParameters.height = (int)(fieldWidth / (double)fieldParameters.image_filtered.Size.Width * fieldParameters.image_filtered.Size.Height / stoneHeightWidhSpace);
                }
                important = true;
            }
            else if (e.PropertyName.Equals("Height"))
            {
                fieldParameters.height = (int)fsvm.Height;
                if (fsvm.BindSize)
                {
                    double fieldHeight = fsvm.Height * (fieldParameters.c + fieldParameters.d);
                    double stoneWidthWidthSpace = fieldParameters.a + fieldParameters.b;
                    fieldParameters.length = (int)(fieldHeight / (double)fieldParameters.image_filtered.Size.Height * fieldParameters.image_filtered.Size.Width / stoneWidthWidthSpace);
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
                UpdateStoneSizes();
                important = true;
            }
            else if (e.PropertyName.Equals("Horizontal"))
            {
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
                fieldParameters.a = fsvm.SelectedItem.Sizes.d;
                fieldParameters.b = fsvm.SelectedItem.Sizes.c;
                fieldParameters.c = fsvm.SelectedItem.Sizes.b;
                fieldParameters.d = fsvm.SelectedItem.Sizes.a;
            }
            else
            {
                fieldParameters.a = fsvm.SelectedItem.Sizes.a;
                fieldParameters.b = fsvm.SelectedItem.Sizes.b;
                fieldParameters.c = fsvm.SelectedItem.Sizes.c;
                fieldParameters.d = fsvm.SelectedItem.Sizes.d;
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
            fsvm.FieldSize = fieldParameters.height * fieldParameters.length;
            fsvm.Length = fieldParameters.length;
            fsvm.Height = fieldParameters.height;

            Sizes currentSize = new Sizes(fieldParameters.a, fieldParameters.b, fieldParameters.c, fieldParameters.d);
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
        }

        private void OpenBuildTools()
        {
            ProtocolV protocolV = new ProtocolV();
            protocolV.DataContext = new ProtocolVM(fieldParameters);
            protocolV.ShowDialog();
        }
        #endregion

        #region Commands
        private ICommand _BuildtoolsClick;
        public ICommand BuildtoolsClick { get { return _BuildtoolsClick; } set { if (value != _BuildtoolsClick) { _BuildtoolsClick = value; } } }
        #endregion
    }
}
