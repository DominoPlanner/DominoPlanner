using DominoPlanner.Core;
using DominoPlanner.Core.Dithering;
using DominoPlanner.Usage.HelperClass;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class CreateFieldVM : TabBaseVM
    {
        #region CTOR
        public CreateFieldVM(string filePath = "") : base()
        {
            fsvm = new FieldSizeVM(true);
            OnlyOwnStonesVM = new OnlyOwnStonesVM();
            
            fParameters = new FieldParameters(filePath, @"C:\Users\johan\Desktop\colors.DColor", 8, 8, 24, 8, 1500, Inter.Lanczos4, new Dithering(), ColorDetectionMode.CieDe2000Comparison, new NoColorRestriction());
            
            iResizeMode = (int)fParameters.resizeMode;
            iColorApproxMode = (int)fParameters.colorMode.colorComparisonMode;
            iDiffusionMode = (int)fParameters.ditherMode.Mode;

            ReloadSizes();

            updateField();
            UnsavedChanges = false;
            BuildtoolsClick = new RelayCommand(o => { OpenBuildTools(); });
        }
        #endregion

        #region fields
        Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
        FieldParameters fParameters;
        DominoTransfer dominoTransfer;
        #endregion

        #region prope
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
                    fParameters.IterationInformation = new IterativeColorRestriction(OnlyOwnStonesVM.Iterations, OnlyOwnStonesVM.Weight);
                }
                else
                {
                    fParameters.IterationInformation = new NoColorRestriction();
                }
                updateField();
            }
            else if (e.PropertyName.Equals("Iterations"))
            {
                if (OnlyOwnStonesVM.OnlyUse)
                {
                    fParameters.IterationInformation.maxNumberOfIterations = OnlyOwnStonesVM.Iterations;
                    updateField();
                }
            }
            else if (e.PropertyName.Equals("Weight"))
            {
                if (OnlyOwnStonesVM.OnlyUse)
                {
                    ((IterativeColorRestriction)fParameters.IterationInformation).iterationWeight = OnlyOwnStonesVM.Weight;
                    updateField();
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
                    fParameters.resizeMode = (Inter)value;
                    sResizeMode = fParameters.resizeMode.ToString();
                    RaisePropertyChanged();
                    updateField();
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
                            fParameters.colorMode = ColorDetectionMode.Cie1976Comparison;
                            sColorApproxMode = "CIE-76 Comparison (ISO 12647)";
                            break;
                        case 1:
                            fParameters.colorMode = ColorDetectionMode.CmcComparison;
                            sColorApproxMode = "CMC (l:c) Comparison";
                            break;
                        case 2:
                            fParameters.colorMode = ColorDetectionMode.Cie94Comparison;
                            sColorApproxMode = "CIE-94 Comparison (DIN 99)";
                            break;
                        case 3:
                            fParameters.colorMode = ColorDetectionMode.CieDe2000Comparison;
                            sColorApproxMode = "CIE-E-2000 Comparison";
                            break;
                        default:
                            break;
                    }
                    RaisePropertyChanged();
                    updateField();
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
                            fParameters.ditherMode = new Dithering();
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
        }

        private void updateField()
        {
            dominoTransfer = fParameters.Generate(progress);
            CurrentPlan = ImageConvert.ToWriteableBitmap(dominoTransfer.GenerateImage(2000).Bitmap);
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

            throw new NotImplementedException();
        }

        private void CreateFieldVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            bool important = false;
            if (e.PropertyName.Equals("FieldSize"))
            {
                fParameters.TargetCount = fsvm.FieldSize;
                important = true;
            }
            else if (e.PropertyName.Equals("Length"))
            {
                fParameters.length = (int)fsvm.Length;
                if (fsvm.BindSize)
                {
                    double fieldWidth = fsvm.Length * (fParameters.a + fParameters.b);
                    double stoneHeightWidhSpace = fParameters.c + fParameters.d;
                    fParameters.height = (int)(fieldWidth / (double)fParameters.image_filtered.Size.Width * fParameters.image_filtered.Size.Height / stoneHeightWidhSpace);
                }
                important = true;
            }
            else if (e.PropertyName.Equals("Height"))
            {
                fParameters.height = (int)fsvm.Height;
                if (fsvm.BindSize)
                {
                    double fieldHeight = fsvm.Height * (fParameters.c + fParameters.d);
                    double stoneWidthWidthSpace = fParameters.a + fParameters.b;
                    fParameters.length = (int)(fieldHeight / (double)fParameters.image_filtered.Size.Height * fParameters.image_filtered.Size.Width / stoneWidthWidthSpace);
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
                updateField();
                ReloadSizes();
            }
        }

        private void UpdateStoneSizes()
        {
            if (fsvm.Vertical)
            {
                fParameters.a = fsvm.SelectedItem.Sizes.d;
                fParameters.b = fsvm.SelectedItem.Sizes.c;
                fParameters.c = fsvm.SelectedItem.Sizes.b;
                fParameters.d = fsvm.SelectedItem.Sizes.a;
            }
            else
            {
                fParameters.a = fsvm.SelectedItem.Sizes.a;
                fParameters.b = fsvm.SelectedItem.Sizes.b;
                fParameters.c = fsvm.SelectedItem.Sizes.c;
                fParameters.d = fsvm.SelectedItem.Sizes.d;
            }
        }

        private void Sizes_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateStoneSizes();
            updateField();
            ReloadSizes();
        }

        private void ReloadSizes()
        {
            fsvm.PropertyChanged -= CreateFieldVM_PropertyChanged;
            fsvm.FieldSize = fParameters.height * fParameters.length;
            fsvm.Length = fParameters.length;
            fsvm.Height = fParameters.height;

            Sizes currentSize = new Sizes(fParameters.a, fParameters.b, fParameters.c, fParameters.d);
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
            protocolV.DataContext = new ProtocolVM(fParameters);
            protocolV.ShowDialog();
        }
        #endregion

        #region Commands

        private ICommand _BuildtoolsClick;
        public ICommand BuildtoolsClick { get { return _BuildtoolsClick; } set { if (value != _BuildtoolsClick) { _BuildtoolsClick = value; } } }

        #endregion
    }
}
