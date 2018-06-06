using ColorMine.ColorSpaces.Comparisons;
using DominoPlanner.Core;
using DominoPlanner.Core.ColorMine.Comparisons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            //filePath einfach mal öffnen :D
            fsvm = new FieldSizeVM(true);
            BitmapImage b = new BitmapImage(new Uri(@"D:\Pictures\HintergrundOrdner\TDT2016_Teamfoto.JPG", UriKind.RelativeOrAbsolute));
            WriteableBitmap wb = new WriteableBitmap(b);
            fParameters = new FieldParameters(wb, new List<DominoColor>(), 8, 8, 24, 8, 1000, BitmapScalingMode.NearestNeighbor, DitherMode.NoDithering, ColorDetectionMode.Cie94Comparison);
            fParameters.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            fParameters.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            fParameters.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            fParameters.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            fParameters.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            fParameters.colors.Add(new DominoColor(Colors.White, 1000, "white"));
            dominoTransfer = fParameters.Generate(progress);

            iResizeMode = (int)fParameters.resizeMode;

            if (fParameters.colorMode.GetType() == typeof(Cie1976Comparison))
                iColorApproxMode = 0;
            else if (fParameters.colorMode.GetType() == typeof(CmcComparison))
                iColorApproxMode = 1;
            else if (fParameters.colorMode.GetType() == typeof(Cie94Comparison))
                iColorApproxMode = 2;
            else
                iColorApproxMode = 3;

            iDiffusionMode = (int)fParameters.ditherMode;

            ReloadSizes();

            CurrentPlan = dominoTransfer.GenerateImage(2000);
            UnsavedChanges = false;
            BuildtoolsClick = new RelayCommand(o => { OpenBuildTools(); });
        }
        #endregion

        #region fields
        private DitherMode actDitherMode;
        private IColorSpaceComparison cdMode;
        private BitmapScalingMode bsMode;

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
                    switch (value)
                    {
                        case 0:
                            bsMode = BitmapScalingMode.Unspecified;
                            fParameters.resizeMode = BitmapScalingMode.Unspecified;
                            sResizeMode = "ähm.. wie wollen wir das nenen?";
                            break;
                        case 1:
                            bsMode = BitmapScalingMode.Linear;
                            fParameters.resizeMode = BitmapScalingMode.Linear;
                            sResizeMode = "Linear";
                            break;
                        case 2:
                            bsMode = BitmapScalingMode.Fant;
                            fParameters.resizeMode = BitmapScalingMode.Fant;
                            sResizeMode = "Bicubic";
                            break;
                        case 3:
                            bsMode = BitmapScalingMode.NearestNeighbor;
                            fParameters.resizeMode = BitmapScalingMode.NearestNeighbor;
                            sResizeMode = "Nearest Neighbor";
                            break;
                        default:
                            break;
                    }
                    RaisePropertyChanged();
                    dominoTransfer = fParameters.Generate(progress);
                    CurrentPlan = dominoTransfer.GenerateImage(2000);
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
                            fParameters.colorMode = ColorDetectionMode.Cie1976Comparison;
                            sColorApproxMode = "CIE-76 Comparison (ISO 12647)";
                            break;
                        case 1:
                            cdMode = ColorDetectionMode.CmcComparison;
                            fParameters.colorMode = ColorDetectionMode.CmcComparison;
                            sColorApproxMode = "CMC (l:c) Comparison";
                            break;
                        case 2:
                            cdMode = ColorDetectionMode.Cie94Comparison;
                            fParameters.colorMode = ColorDetectionMode.Cie94Comparison;
                            sColorApproxMode = "CIE-94 Comparison (DIN 99)";
                            break;
                        case 3:
                            cdMode = ColorDetectionMode.CieDe2000Comparison;
                            fParameters.colorMode = ColorDetectionMode.CieDe2000Comparison;
                            sColorApproxMode = "CIE-E-2000 Comparison";
                            break;
                        default:
                            break;
                    }
                    RaisePropertyChanged();
                    dominoTransfer = fParameters.Generate(progress);
                    CurrentPlan = dominoTransfer.GenerateImage(2000);
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
                    actDitherMode = (DitherMode)value;
                    fParameters.ditherMode = actDitherMode;
                    switch (actDitherMode)
                    {
                        case DitherMode.NoDithering:
                            sDiffusionMode = "NoDiffusion";
                            break;
                        case DitherMode.FloydSteinberg:
                            sDiffusionMode = "Floyd/Steinberg Dithering";
                            break;
                        case DitherMode.JarvisJudiceNinke:
                            sDiffusionMode = "Jarvis/Judice/Ninke Dithering";
                            break;
                        case DitherMode.Stucki:
                            sDiffusionMode = "Stucki Dithering";
                            break;
                        default:
                            break;
                    }
                    RaisePropertyChanged();
                    dominoTransfer = fParameters.Generate(progress);
                    CurrentPlan = dominoTransfer.GenerateImage(2000);
                    fParameters.GenerateShapes();
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
        public override bool Save()
        {
            throw new NotImplementedException();
        }

        private void CreateFieldVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            bool important = false;
            if (e.PropertyName.Equals("FieldSize"))
            {
                fParameters.targetCount = fsvm.FieldSize;
                important = true;
            }
            else if (e.PropertyName.Equals("Length"))
            {
                fParameters.length = (int)fsvm.Length;
                important = true;
            }
            else if (e.PropertyName.Equals("Height"))
            {
                fParameters.height = (int)fsvm.Height;
                important = true;
            }
            else if (e.PropertyName.Equals("SelectedItem"))
            {
                fParameters.a = fsvm.SelectedItem.Sizes.a;
                fParameters.b = fsvm.SelectedItem.Sizes.b;
                fParameters.c = fsvm.SelectedItem.Sizes.c;
                fParameters.d = fsvm.SelectedItem.Sizes.d;
                important = true;
            }
            else if (e.PropertyName.Equals("Vertical"))
            {
                if (fsvm.Vertical)
                {
                    fParameters.a = fsvm.SelectedItem.Sizes.d;
                    fParameters.b = fsvm.SelectedItem.Sizes.c;
                    fParameters.c = fsvm.SelectedItem.Sizes.b;
                    fParameters.d = fsvm.SelectedItem.Sizes.a;
                    important = true;
                }
            }
            else if (e.PropertyName.Equals("Horizontal"))
            {
                if (fsvm.Horizontal)
                {
                    fParameters.a = fsvm.SelectedItem.Sizes.a;
                    fParameters.b = fsvm.SelectedItem.Sizes.b;
                    fParameters.c = fsvm.SelectedItem.Sizes.c;
                    fParameters.d = fsvm.SelectedItem.Sizes.d;
                    important = true;
                }
            }

            if (important)
            {
                dominoTransfer = fParameters.Generate(progress);
                CurrentPlan = dominoTransfer.GenerateImage(2000);
                ReloadSizes();
            }
        }

        private void Sizes_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            fParameters.a = ((Sizes)sender).a;
            fParameters.b = ((Sizes)sender).b;
            fParameters.c = ((Sizes)sender).c;
            fParameters.d = ((Sizes)sender).d;
            dominoTransfer = fParameters.Generate(progress);
            CurrentPlan = dominoTransfer.GenerateImage(2000);
            ReloadSizes();
        }

        private void ReloadSizes()
        {
            this.fsvm.PropertyChanged -= CreateFieldVM_PropertyChanged;
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
