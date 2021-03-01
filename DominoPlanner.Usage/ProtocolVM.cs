﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;

namespace DominoPlanner.Usage
{
    using static Localizer;
    public enum Corner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    public struct NaturalFieldPlanOrientation
    {
        public bool orientation;
        public bool x;
        public bool y;
        public NaturalFieldPlanOrientation(bool mirrorX, bool mirrorY, bool orientation ) // true = vertical
        {
            x = mirrorX;
            y = mirrorY;
            this.orientation = orientation;
        }
        public (bool, bool) GetCorner(bool left, bool top, bool currentOrientation)
        {
            var (templeft, temptop) = (left ^ x, top ^ y);
            if (currentOrientation && templeft != temptop)
            {
                return (temptop, templeft);
            }
            else
            {
                return (templeft, temptop);
            }
        }
    }
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

        private NaturalFieldPlanOrientation naturalOrientation;

        public NaturalFieldPlanOrientation NaturalOrientation
        {
            get { return naturalOrientation; }
            set { naturalOrientation = value; RaisePropertyChanged();  }
        }


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

        private double rotateAngle;

        public double RotateAngle
        {
            get { return rotateAngle; }
            set { rotateAngle = value; RaisePropertyChanged(); }
        }

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
        [SettingsAttribute("ProtocolVM", false)]
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
        [SettingsAttribute("ProtocolVM", true)]
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
        [SettingsAttribute("ProtocolVM", false)]
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
        [SettingsAttribute("ProtocolVM", true)]
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
        [SettingsAttribute("ProtocolVM", 50)]
        public int StonesPerBlock
        {
            get 
            { 
                return _StonesPerBlock > 0 ? _StonesPerBlock : 1; 
            }
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
            TextFormat = "<font face=\"Verdana\">";
            DefaultBackColor = true;
            IntelligentTextColor = true;
            HideText = true;
            currentOPP.orientation = DominoProvider.FieldPlanDirection;
            currentOPP.mirrorHorizontal = DominoProvider.FieldPlanDirection == Core.Orientation.Vertical;
            CurrentProtocol = DominoProvider.GetHTMLProcotol(currentOPP);

            NaturalOrientation = GetNaturalOrientation();

            SkiaSharp.SKImage new_img = DominoProvider.Last.GenerateImage(1000, false).Snapshot();
            CurrentPlan = Bitmap.DecodeToWidth(new_img.Encode().AsStream(), new_img.Width);

            ShowLiveBuildHelper = new RelayCommand(o => { ShowLiveHelper(); });
            SaveHTML = new RelayCommand(o => { SaveHTMLFile(); });
            SaveExcel = new RelayCommand(o => { SaveExcelFile(); });

            this.PropertyChanged += ProtocolVM_PropertyChanged;

            ClickTopLeft = new RelayCommand(o => SetOrientation(false, false));
            ClipTopRight = new RelayCommand(o => SetOrientation(true, false));
            ClickBottomLeft = new RelayCommand(o => SetOrientation(false, true));
            ClickBottomRight = new RelayCommand(o => SetOrientation(true, true));

            // Special case: diagonal fields. We'll rotate the image so the arrows are correct again.
            if (DominoProvider is StructureParameters structure && XElement.Parse(structure._structureDefinitionXML).Attribute("Name").Value == "Diagonal Field")
            {
                RotateAngle = 45;
            }

        }
        private void SetOrientation(bool left, bool top)
        {
            var (target_left, target_top) = NaturalOrientation.GetCorner(left, top, Orientation);
            if (MirrorX == target_left && MirrorY == target_top)
                Orientation = !Orientation;
            (target_left, target_top) = NaturalOrientation.GetCorner(left, top, Orientation);
            MirrorX = target_left;
            MirrorY = target_top;
        }
        private NaturalFieldPlanOrientation GetNaturalOrientation()
        {
            var field = DominoProvider.GetBaseField();
            var topLeft = DominoProvider.Last.shapes.OrderBy(x => x.position.x).ThenBy(x => x.position.y).First();
            var topRight = DominoProvider.Last.shapes.OrderByDescending(x => x.position.x).ThenBy(x => x.position.y).First();
            var bottomLeft = DominoProvider.Last.shapes.OrderByDescending(x => x.position.y).ThenBy(x => x.position.x).First();

            var horizontalDx = topRight.GetBoundingRectangle().xc - topLeft.GetBoundingRectangle().xc;
            var horizontalDy = topRight.GetBoundingRectangle().yc - topLeft.GetBoundingRectangle().yc;
            var verticalDx = bottomLeft.GetBoundingRectangle().xc - topLeft.GetBoundingRectangle().xc;
            var verticalDy = bottomLeft.GetBoundingRectangle().yc - topLeft.GetBoundingRectangle().yc;
            NaturalFieldPlanOrientation orientation = new NaturalFieldPlanOrientation();
            
            if (horizontalDx < 0)
                orientation.x = true;
            if (verticalDy < 0)
                orientation.y = true;
            orientation.orientation = !(Math.Abs(horizontalDx) - Math.Abs(horizontalDy) > 0);
            //if (orientation.x ^ orientation.y)
            //    orientation.orientation = !orientation.orientation;
            return orientation;
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
            dlg.Filters.Add(new FileDialogFilter() { Extensions = new List<string> { "xlsx" }, Name = _("Excel Document") });

            string result = string.Empty;
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                ProtocolV protView = desktopLifetime.Windows.OfType<ProtocolV>().FirstOrDefault();
                if(protView != null)
                {
                    result = await dlg.ShowAsync(protView);
                }
            }

            if (result != null && result != "")
            {
                try
                {
                    DominoProvider.SaveXLSFieldPlan(result, currentOPP);
                    var process = new Process();
                    process.StartInfo = new ProcessStartInfo(result) { UseShellExecute = true };
                    process.Start();
                }
                catch (Exception ex) { await Errorhandler.RaiseMessage(_("Error: ") + ex.Message, _("Error"), Errorhandler.MessageType.Error); }
            }
        }

        public async void SaveHTMLFile()
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                DefaultExtension = ".html",
                InitialFileName = Titel
            };
            dlg.Filters.Add(new FileDialogFilter() { Extensions = new List<string> { "html" }, Name = _("Hypertext Markup Language") });
            string filename = string.Empty;
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                ProtocolV protView = desktopLifetime.Windows.OfType<ProtocolV>().FirstOrDefault();
                if (protView != null)
                {
                    filename = await dlg.ShowAsync(protView);
                }
            }
            if (filename != null && filename != "")
            {

                try
                {
                    FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(CurrentProtocol);
                    sw.Close();
                    var process = new Process();
                    process.StartInfo = new ProcessStartInfo(filename) { UseShellExecute = true };
                    process.Start();
                }
                catch (Exception ex) { await Errorhandler.RaiseMessage(_("Error: ") + ex.Message, _("Error"), Errorhandler.MessageType.Error); }
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
