using DominoPlanner.Core;
using DominoPlanner.Core.ColorMine.Comparisons;
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

namespace DominoPlanner.Usage
{
    class ProtocolVM : ModelBase
    {
        #region CTOR
        public ProtocolVM(string filePath)
        {
            //hier muss dann mal die Datei geladen werden
            BitmapImage b = new BitmapImage(new Uri(@"D:\Pictures\HintergrundOrdner\TDT2016_Teamfoto.JPG", UriKind.RelativeOrAbsolute));
            WriteableBitmap wb = new WriteableBitmap(b);
            fParameters = new FieldParameters(wb, new List<DominoColor>(), 8, 8, 24, 8, 1500, BitmapScalingMode.HighQuality, DitherMode.NoDithering, ColorDetectionMode.CieDe2000Comparison);
            fParameters.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            fParameters.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            fParameters.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            fParameters.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            fParameters.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            fParameters.colors.Add(new DominoColor(Colors.White, 1000, "white"));
            dominoTransfer = fParameters.Generate(progress);

            Init();
        }

        public ProtocolVM(IDominoProvider fieldParameters)
        {
            fParameters = fieldParameters;
            dominoTransfer = fParameters.Generate(progress);
            Init();
        }
        #endregion

        #region fields
        Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
        private ObjectProtocolParameters currentOPP = new ObjectProtocolParameters();
        IDominoProvider fParameters;
        DominoTransfer dominoTransfer;
        #endregion

        #region prope
        private bool _DefaultBackColor;
        public bool DefaultBackColor
        {
            get { return _DefaultBackColor; }
            set
            {
                if (_DefaultBackColor != value)
                {
                    _DefaultBackColor = value;
                    if (value)
                        currentOPP.backColorMode = ColorMode.Normal;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _InvertedBackColor;
        public bool InvertedBackColor
        {
            get { return _InvertedBackColor; }
            set
            {
                if (_InvertedBackColor != value)
                {
                    _InvertedBackColor = value;
                    if (value)
                        currentOPP.backColorMode = ColorMode.Inverted;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IntelligentBackColor;
        public bool IntelligentBackColor
        {
            get { return _IntelligentBackColor; }
            set
            {
                if (_IntelligentBackColor != value)
                {
                    _IntelligentBackColor = value;
                    if (value)
                        currentOPP.backColorMode = ColorMode.Intelligent;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DefaultTextColor;
        public bool DefaultTextColor
        {
            get { return _DefaultTextColor; }
            set
            {
                if (_DefaultTextColor != value)
                {
                    _DefaultTextColor = value;
                    if (value)
                        currentOPP.foreColorMode = ColorMode.Normal;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _InvertedTextColor;
        public bool InvertedTextColor
        {
            get { return _InvertedTextColor; }
            set
            {
                if (_InvertedTextColor != value)
                {
                    _InvertedTextColor = value;
                    if (value)
                        currentOPP.foreColorMode = ColorMode.Inverted;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IntelligentTextColor;
        public bool IntelligentTextColor
        {
            get { return _IntelligentTextColor; }
            set
            {
                if (_IntelligentTextColor != value)
                {
                    _IntelligentTextColor = value;
                    if (value)
                        currentOPP.foreColorMode = ColorMode.Intelligent;
                    RaisePropertyChanged();
                }
            }
        }


        private string _TextFormat;
        public string TextFormat
        {
            get { return _TextFormat; }
            set
            {
                if (_TextFormat != value)
                {
                    _TextFormat = value;
                    currentOPP.textFormat = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TextRegex;
        public string TextRegex
        {
            get { return _TextRegex; }
            set
            {
                if (_TextRegex != value)
                {
                    _TextRegex = value;
                    currentOPP.textRegex = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _HasNoProperties;
        public bool HasNoProperties
        {
            get { return _HasNoProperties; }
            set
            {
                if (_HasNoProperties != value)
                {
                    _HasNoProperties = value;
                    if (value)
                        currentOPP.summaryMode = SummaryMode.None;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _HasShortProperties;
        public bool HasShortProperties
        {
            get { return _HasShortProperties; }
            set
            {
                if (_HasShortProperties != value)
                {
                    _HasShortProperties = value;
                    if (value)
                        currentOPP.summaryMode = SummaryMode.Small;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _HasExtendedProperties;
        public bool HasExtendedProperties
        {
            get { return _HasExtendedProperties; }
            set
            {
                if (_HasExtendedProperties != value)
                {
                    _HasExtendedProperties = value;
                    if (value)
                        currentOPP.summaryMode = SummaryMode.Large;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _HideText;
        public bool HideText
        {
            get { return _HideText; }
            set
            {
                if (_HideText != value)
                {
                    _HideText = value;
                    if (value)
                        TextRegex = "%count%";
                    else
                        TextRegex = "%count% %color%";

                    RaisePropertyChanged();
                }
            }
        }

        private string _Titel;
        public string Titel
        {
            get { return _Titel; }
            set
            {
                if (_Titel != value)
                {
                    _Titel = value;
                    currentOPP.title = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _UseBlocks;
        public bool UseBlocks
        {
            get { return _UseBlocks; }
            set
            {
                if (_UseBlocks != value)
                {
                    _UseBlocks = value;

                    RaisePropertyChanged();
                }
            }
        }

        private int _StonesPerBlock;
        public int StonesPerBlock
        {
            get { return _StonesPerBlock; }
            set
            {
                if (_StonesPerBlock != value)
                {
                    _StonesPerBlock = value;
                    currentOPP.templateLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _BuildReverse;
        public bool BuildReverse
        {
            get { return _BuildReverse; }
            set
            {
                if (_BuildReverse != value)
                {
                    _BuildReverse = value;
                    currentOPP.reverse = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CurrentProtocol;
        public string CurrentProtocol
        {
            get { return _CurrentProtocol; }
            set
            {
                if (_CurrentProtocol != value)
                {
                    _CurrentProtocol = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Methods
        private void Init()
        {
            Titel = "Field";
            StonesPerBlock = 20;
            BuildReverse = false;
            currentOPP.reverse = false;
            HasShortProperties = true;
            TextFormat = "<font face=\"Verdana\">";
            DefaultBackColor = true;
            IntelligentTextColor = true;
            HideText = true;
            currentOPP.orientation = Orientation.Horizontal;

            CurrentProtocol = fParameters.GetHTMLProcotol(currentOPP);

            ShowLiveBuildHelper = new RelayCommand(o => { ShowLiveHelper(); });
            SaveHTML = new RelayCommand(o => { SaveHTMLFile(); });
            SaveExcel = new RelayCommand(o => { SaveExcelFile(); });

            this.PropertyChanged += ProtocolVM_PropertyChanged;
        }
        private void ProtocolVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CurrentProtocol = fParameters.GetHTMLProcotol(currentOPP);
        }
        private void ShowLiveHelper()
        {
            LiveBuildHelperV lbhv = new LiveBuildHelperV();
            lbhv.DataContext = new LiveBuildHelperVM(fParameters, StonesPerBlock);
            lbhv.ShowDialog();
        }

        public void SaveExcelFile()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Excel Document (.xlsx)|*.xlsx";
            dlg.FileName = Titel;

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    fParameters.SaveXLSFieldPlan(dlg.FileName, currentOPP);
                    MessageBox.Show("Save protocol in excel file.", "Save", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                catch (Exception ex) { MessageBox.Show("Fehler: " + ex.Message); }
            }
        }

        public void SaveHTMLFile()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".html";
            dlg.Filter = "Hypertext Markup Language (.html)|*.html";
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;

                try
                {
                    FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(CurrentProtocol);
                    fs.Close();
                    MessageBox.Show("Save protocol in .html file!", "Save", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                catch (Exception ex) { MessageBox.Show("Fehler: " + ex.Message); }
            }
        }
        #endregion

        #region commands
        private ICommand _ShowliveBuildHelper;
        public ICommand ShowLiveBuildHelper { get { return _ShowliveBuildHelper; } set { if (value != _ShowliveBuildHelper) { _ShowliveBuildHelper = value; } } }

        private ICommand _SaveHTML;
        public ICommand SaveHTML { get { return _SaveHTML; } set { if (value != _SaveHTML) { _SaveHTML = value; } } }

        private ICommand _SaveExcel;
        public ICommand SaveExcel { get { return _SaveExcel; } set { if (value != _SaveExcel) { _SaveExcel = value; } } }
        #endregion
    }
}
