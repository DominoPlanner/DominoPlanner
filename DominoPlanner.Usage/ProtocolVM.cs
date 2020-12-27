using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Input;

namespace DominoPlanner.Usage
{
    class ProtocolVM : ModelBase
    {
        private ICommand _ClipTopRight;
        public ICommand ClipTopRight { get { return _ClipTopRight; } set { if (value != _ClipTopRight) { _ClipTopRight = value; } } }

			private ICommand _ClickTopLeft;
        public ICommand ClickTopLeft { get { return _ClickTopLeft; } set { if (value != _ClickTopLeft) { _ClickTopLeft = value; } } }


			private ICommand _ClickBottomLeft;
        public ICommand ClickBottomLeft { get { return _ClickBottomLeft; } set { if (value != _ClickBottomLeft) { _ClickBottomLeft = value; } } }

			private ICommand _ClickBottomRight;
        public ICommand ClickBottomRight { get { return _ClickBottomRight; } set { if (value != _ClickBottomRight) { _ClickBottomRight = value; } } }



        #region CTOR
        public ProtocolVM(string filePath)
        {
            Titel = Path.GetFileNameWithoutExtension(filePath);
            Init();
        }

        public ProtocolVM(IDominoProvider dominoProvider, string fieldName, string assemblyname = "")
        {
            DominoProvider = dominoProvider;
            dominoTransfer = DominoProvider.Generate();
            Titel = fieldName;
            currentOPP.project = assemblyname;
            Init();
        }
        #endregion

        #region fields
        Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
        private readonly ObjectProtocolParameters currentOPP = new ObjectProtocolParameters();
        IDominoProvider DominoProvider;
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
                    currentOPP.templateLength = _UseBlocks ? StonesPerBlock : int.MaxValue;
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
        
        public bool MirrorX
        {
            get { return currentOPP.mirrorHorizontal; }
            set
            {
                if (currentOPP.mirrorHorizontal != value)
                {
                    currentOPP.mirrorHorizontal = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool MirrorY
        {
            get { return currentOPP.mirrorVertical; }
            set
            {
                if (currentOPP.mirrorVertical != value)
                {
                    currentOPP.mirrorVertical = value;
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
        public bool Orientation
        {
            get
            {
                return currentOPP.orientation == Core.Orientation.Vertical;
            }
            set
            {
                if (value != (currentOPP.orientation == Core.Orientation.Vertical))
                {
                    currentOPP.orientation = value ? Core.Orientation.Vertical : Core.Orientation.Horizontal;
                    RaisePropertyChanged();
                }
            }
        }

        private Bitmap _CurrentPlan;
        public Bitmap CurrentPlan
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

        #endregion

        #region Methods
        private void Init()
        {
            StonesPerBlock = 50;
            UseBlocks = true;
            HasShortProperties = true;
            TextFormat = "<font face=\"Verdana\">";
            DefaultBackColor = true;
            IntelligentTextColor = true;
            HideText = true;
            currentOPP.orientation = DominoProvider.FieldPlanDirection;
            currentOPP.mirrorHorizontal = DominoProvider.FieldPlanDirection == Core.Orientation.Vertical;
            CurrentProtocol = DominoProvider.GetHTMLProcotol(currentOPP);

            SkiaSharp.SKImage new_img = DominoProvider.Last.GenerateImage(1000, false).Snapshot();
            CurrentPlan = Bitmap.DecodeToWidth(new_img.Encode().AsStream(), new_img.Width);

            ShowLiveBuildHelper = new RelayCommand(o => { ShowLiveHelper(); });
            SaveHTML = new RelayCommand(o => { SaveHTMLFile(); });
            SaveExcel = new RelayCommand(o => { SaveExcelFile(); });

            this.PropertyChanged += ProtocolVM_PropertyChanged;

            ClickTopLeft = new RelayCommand(o => { MirrorX = false; MirrorY = false; });
            ClipTopRight = new RelayCommand(o => { MirrorX = !Orientation; MirrorY = Orientation; });
            ClickBottomLeft = new RelayCommand(o => { MirrorX = Orientation; MirrorY = !Orientation; });
            ClickBottomRight = new RelayCommand(o => { MirrorX = true; MirrorY = true; });
        }
        private void ProtocolVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CurrentProtocol = DominoProvider.GetHTMLProcotol(currentOPP);
        }
        private void ShowLiveHelper()
        {
            LiveBuildHelperV lbhv = new LiveBuildHelperV
            {
                DataContext = new LiveBuildHelperVM(DominoProvider, StonesPerBlock, currentOPP.orientation, MirrorX, MirrorY)
            };
            lbhv.Show();
        }

        public async void SaveExcelFile()
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                DefaultExtension = ".xlsx",
                InitialFileName = Titel
            };
            dlg.Filters.Add(new FileDialogFilter() { Extensions = new List<string> { "xlsx" }, Name = "Excel Document" });
            var result = dlg.ShowDialog();
            if (result != null && result != "")
            {
                try
                {
                    DominoProvider.SaveXLSFieldPlan(result, currentOPP);
                    Process.Start(result);
                }
                catch (Exception ex) { await Errorhandler.RaiseMessage("Error: " + ex.Message, "Error", Errorhandler.MessageType.Error); }
            }
        }

        public async void SaveHTMLFile()
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                DefaultExtension = ".html",
                InitialFileName = Titel
            };
            dlg.Filters.Add(new FileDialogFilter() { Extensions = new List<string> { "html" }, Name = "Hypertext Markup Language" });
            var filename = dlg.ShowDialog();
            if (filename != null && filename != "")
            {

                try
                {
                    FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(CurrentProtocol);
                    fs.Close();
                    Process.Start(filename);
                }
                catch (Exception ex) { await Errorhandler.RaiseMessage("Error: " + ex.Message, "Error", Errorhandler.MessageType.Error); }
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
