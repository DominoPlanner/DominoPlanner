using DominoPlanner.Core;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class ColorListControlVM : TabBaseVM
    {
        #region CTOR
        public ColorListControlVM(string filepath) : base()
        {
            ColorList = new ObservableCollection<ColorListEntry>();

            Reload(filepath);
            
            BtnAddColor = new RelayCommand(o => { AddNewColor(); });
            BtnSaveColors = new RelayCommand(o => { Save(); });
            BtnRemove = new RelayCommand(o => { RemoveSelected(); });
            BtnMoveDown = new RelayCommand(o => { MoveDown(); });
            BtnMoveUp = new RelayCommand(o => { MoveUp(); });
            BtnExportXLSX = new RelayCommand(o => { ExportXLSX(); });
            base.UnsavedChanges = false;
            
            ShowProjects = false;
        }

        public ColorListControlVM(DominoAssembly dominoAssembly) : this(Workspace.AbsolutePathFromReference(dominoAssembly.colorPath, dominoAssembly))
        {
            this.dominoAssembly = dominoAssembly;
        }
        #endregion

        #region Methods
        public void ResetList()
        {
            FilePath = String.Empty;
            colorRepository.Anzeigeindizes.CollectionChanged -= Anzeigeindizes_CollectionChanged;
            colorRepository = new ColorRepository();
            colorRepository.Anzeigeindizes.CollectionChanged += Anzeigeindizes_CollectionChanged;
            _ColorList.Clear();
            refreshList();
        }

        internal void Reload(string fileName)
        {
            FilePath = fileName;
            _ColorList.Clear();
            if (colorRepository != null) colorRepository.Anzeigeindizes.CollectionChanged -= Anzeigeindizes_CollectionChanged;
            colorRepository = Workspace.Load<ColorRepository>(FilePath);
            colorRepository.Anzeigeindizes.CollectionChanged += Anzeigeindizes_CollectionChanged;
            refreshList();
        }

        private void refreshList()
        {
            int counter = 0;
            if (colorRepository.RepresentionForCalculation.OfType<EmptyDomino>().Count() == 1)
            {
                _ColorList.Add(new ColorListEntry() { DominoColor = colorRepository.RepresentionForCalculation.OfType<EmptyDomino>().First(), SortIndex = -1 });
            }
            foreach (DominoColor domino in colorRepository.RepresentionForCalculation.OfType<DominoColor>())
            {
                //colorRepository = Workspace.Load<ColorRepository>(FilePath);
                _ColorList.Add(new ColorListEntry() { DominoColor = domino, SortIndex = colorRepository.Anzeigeindizes[counter] });
                counter++;
            }
        }
        private void ExportXLSX()
        {
            using (var p = new ExcelPackage())
            {
                // content
                var ws = p.Workbook.Worksheets.Add("Overview");
                ws.Cells["A1"].Value = "Color Usage Overview";
                ws.Cells["A1"].Style.Font.Size = 15;
                ws.Cells["B3"].Value = "Color";
                ws.Cells["C3"].Value = "Available";
                
                // Write project titles
                for (int i = 0; i < DifColumns.Count; i++)
                { 
                    ws.Cells[3, 4 + i].Value = DifColumns[i].Header;
                    
                    ws.Cells[4 + ColorList.Count, 4+i].Formula
                        = "SUM(" + ws.Cells[4, 4+i].Address + ":" + ws.Cells[3 + ColorList.Count, 4+i].Address + ")";
                }
                ws.Cells[4 + ColorList.Count, 3].Formula
                        = "SUM(" + ws.Cells[4, 3].Address + ":" + ws.Cells[3 + ColorList.Count, 3].Address + ")";
                ws.Cells[ColorList.Count + 4, 4 + DifColumns.Count].Formula =
                     "SUM(" + ws.Cells[4, 4 + DifColumns.Count].Address + ":" + ws.Cells[3 + ColorList.Count, 4 + DifColumns.Count].Address + ")";
                if (DifColumns.Count != 0)  ws.Cells[3, 4 + DifColumns.Count].Value = "Sum";
                ws.Cells[4 + ColorList.Count, 2].Value = "Sum";
                // fill color counts
                for (int i = 0; i < ColorList.Count; i++)
                {

                    ws.Cells[i + 4, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[i + 4, 1].Style.Fill.BackgroundColor.SetColor(ColorList[i].DominoColor.mediaColor.ToSD());
                    ws.Cells[i + 4, 2].Value = ColorList[i].DominoColor.name;
                    ws.Cells[i + 4, 3].Value = ColorList[i].DominoColor.count;
                    for (int j = 0; j < ColorList[i].ProjectCount.Count; j++)
                    {
                        ws.Cells[4 + i, 4 + j].Value = ColorList[i].ProjectCount[j];
                    }
                    if (DifColumns.Count != 0)
                    {
                        ws.Cells[4 + i, 4 + DifColumns.Count].Formula
                            = "SUM(" + ws.Cells[4 + i, 4].Address + ":"
                            + ws.Cells[4 + i, 3 + DifColumns.Count].Address + ")";
                    }
                }
                
                ws.Cells["C4"].Value = ""; // Count of empty domino
                ws.Calculate();

                //styling 
                
                ws.Cells[3, 4 + DifColumns.Count, 4 + ColorList.Count, 4 + DifColumns.Count].Style.Font.Bold = true;
                ws.Cells[4 + ColorList.Count, 2, 4 + ColorList.Count, 4 + DifColumns.Count].Style.Font.Bold = true;

                ws.Cells[3, 1, 3, 4 + DifColumns.Count].Style.Font.Bold = true;
                ws.Cells[3, 1, 3, 4 + DifColumns.Count].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                ws.Cells[3, 3, 4 + ColorList.Count, 3].Style.Font.Bold = true;
                ws.Cells[3, 3, 4 + ColorList.Count, 3].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                if (DifColumns.Count != 0)
                {
                    ws.Cells[3, 4, 4 + ColorList.Count, 3 + DifColumns.Count].Style.Border.Right.Style
                        = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    ws.Cells[4, 2, 3 + ColorList.Count, 4 + DifColumns.Count].Style.Border.Bottom.Style
                        = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                ws.Cells[4 + ColorList.Count, 2, 4 + ColorList.Count, 4 + DifColumns.Count].Style.Border.Top.Style 
                    = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                ws.Cells[3, 4 + DifColumns.Count, 4 + ColorList.Count, 4 + DifColumns.Count].Style.Border.Left.Style
                    = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = "ColorList";
                dlg.DefaultExt = ".xlsx";
                dlg.Filter = "Excel files (.xlsx)|*.xlsx|All Files (*.*)|*";
                if (dlg.ShowDialog() == true)
                {
                    try
                    {
                        p.SaveAs(new FileInfo(dlg.FileName));
                        Process.Start(dlg.FileName);
                    }
                    catch
                    {
                        MessageBox.Show("Save failed");
                    }
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
        private void RemoveSelected()
        {
            throw new NotImplementedException();
            UnsavedChanges = true;
        }

        private void MoveUp()
        {
            try
            {
                if (SelectedStone.DominoColor is DominoColor dominoColor)
                {
                    colorRepository.MoveUp(dominoColor);
                }
                RaisePropertyChanged("ColorList");
            }
            catch (Exception) { }
        }

        private void MoveDown()
        {
            try
            {
                if (SelectedStone.DominoColor is DominoColor dominoColor)
                {
                    colorRepository.MoveDown(dominoColor);
                }
                RaisePropertyChanged("ColorList");
            }
            catch (Exception) { }
        }

        private void AddNewColor()
        {
            colorRepository.Add(new DominoColor(System.Windows.Media.Colors.IndianRed, 0, "New Color"));
            _ColorList.Add(new ColorListEntry() { DominoColor = colorRepository.RepresentionForCalculation.Last(), SortIndex = colorRepository.Anzeigeindizes.Last(),
             ProjectCount = new ObservableCollection<int>(Enumerable.Repeat(0, _ColorList[0].ProjectCount.Count))});

            UnsavedChanges = true;
        }

        public override bool Save()
        {
            if(FilePath == string.Empty)
            {
                SaveFileDialog ofd = new SaveFileDialog();
                ofd.Filter = "domino color files (*.DColor)|*.DColor|All files (*.*)|*.*";
                if (ofd.ShowDialog() == true)
                {
                    if (ofd.FileName != string.Empty)
                    {
                        FilePath = ofd.FileName;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            colorRepository.Save(FilePath);
            UnsavedChanges = false;
            return true;
        }

        private void DominoColor_PropertyChanged(object sender, string e)
        {
            UnsavedChanges = true;
        }

        private void Anzeigeindizes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    _ColorList.Where(x => x.DominoColor is DominoColor).ElementAt(e.NewStartingIndex).SortIndex = (int)e.NewItems[0];
                    break;
            }
            UnsavedChanges = false;
            RaisePropertyChanged("ColorList");
        }
        #endregion

        #region COMMANDS
        private ICommand _BtnExportXLSX;
        public ICommand BtnExportXLSX { get { return _BtnExportXLSX; } set { if (value != _BtnExportXLSX) { _BtnExportXLSX = value; } } }

        private ICommand _BtnSendMail;
        public ICommand BtnSendMail { get { return _BtnSendMail; } set { if (value != _BtnSendMail) { _BtnSendMail = value; } } }

        private ICommand _BtnAddColor;
        public ICommand BtnAddColor { get { return _BtnAddColor; } set { if (value != _BtnAddColor) { _BtnAddColor = value; } } }

        private ICommand _BtnSaveColors;
        public ICommand BtnSaveColors { get { return _BtnSaveColors; } set { if (value != _BtnSaveColors) { _BtnSaveColors = value; } } }

        private ICommand _BtnRemove;
        public ICommand BtnRemove { get { return _BtnRemove; } set { if (value != _BtnRemove) { _BtnRemove = value; } } }

        private ICommand _BtnMoveDown;
        public ICommand BtnMoveDown { get { return _BtnMoveDown; } set { if (value != _BtnMoveDown) { _BtnMoveDown = value; } } }

        private ICommand _BtnMoveUp;
        public ICommand BtnMoveUp { get { return _BtnMoveUp; } set { if (value != _BtnMoveUp) { _BtnMoveUp = value; } } }
        #endregion

        #region prop
        private DominoAssembly dominoAssembly;
        public DominoAssembly DominoAssembly
        {
            get { return dominoAssembly; }
        }
        private ColorListEntry _SelectedStone;
        public ColorListEntry SelectedStone
        {
            get { return _SelectedStone; }
            set
            {
                if (value != null && !(value.DominoColor is EmptyDomino))
                {
                    if (_SelectedStone != value)
                    {
                        if (_SelectedStone != null)
                            _SelectedStone.DominoColor.PropertyChanged -= DominoColor_PropertyChanged;
                        _SelectedStone = value;
                        if (_SelectedStone != null)
                            _SelectedStone.DominoColor.PropertyChanged += DominoColor_PropertyChanged;
                    }
                }
                RaisePropertyChanged();
            }
        }
        public override TabItemType tabType
        {
            get
            {
                return TabItemType.ColorList;
            }
        }

        private ColorRepository _ColorRepository;
        public ColorRepository colorRepository
        {
            get { return _ColorRepository; }
            set
            {
                if (_ColorRepository != value)
                {
                    _ColorRepository = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ColorListEntry> _ColorList;
        public ObservableCollection<ColorListEntry> ColorList
        {
            get { return new ObservableCollection<ColorListEntry>(_ColorList.OrderBy(x => x.SortIndex)); }
            set
            {
                if (_ColorList != value)
                {
                    if (_ColorList != null) _ColorList.CollectionChanged -= _ColorList_CollectionChanged;
                    _ColorList = value;
                    _ColorList.CollectionChanged += _ColorList_CollectionChanged;
                    RaisePropertyChanged();
                }
            }
        }

        private void _ColorList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("ColorList");
        }

        private ObservableCollection<DataGridColumn> _DifColumns;
        public ObservableCollection<DataGridColumn> DifColumns
        {
            get { return _DifColumns; }
            set
            {
                if (_DifColumns != value)
                {
                    _DifColumns = value;
                }
            }
        }

        internal event EventHandler ShowProjectsChanged;

        private bool _ShowProjects;
        public bool ShowProjects
        {
            get { return _ShowProjects; }
            set
            {
                if (_ShowProjects != value)
                {
                    _ShowProjects = value;
                    ShowProjectsChanged?.Invoke(this, EventArgs.Empty);
                    RaisePropertyChanged();
                }
            }
        }

        internal override void ResetContent()
        {
            ShowProjects = true;
            DifColumns = new ObservableCollection<DataGridColumn>();
            foreach(ColorListEntry cle in _ColorList)
            {
                cle.ProjectCount.Clear();
            }
            string thispath = Workspace.AbsolutePathFromReference(dominoAssembly.colorPath, dominoAssembly);
            foreach (DocumentNode project in dominoAssembly.children)
            {
                var counts2 = Workspace.LoadColorList<IDominoProvider>(Workspace.AbsolutePathFromReference(project.relativePath, dominoAssembly));
                if (counts2.Item1 != thispath) continue;
                // Jojo: Warnmeldung anzeigen, dass diese Datei nicht in der Liste enthalten ist
                for (int i = 0; i < counts2.Item2.Length; i++)
                {
                    _ColorList[i].ProjectCount.Add(counts2.Item2[i]);
                }
                for (int i = counts2.Item2.Length; i < _ColorList.Count; i++)
                {
                    _ColorList[i].ProjectCount.Add(0);
                }
                AddProjectCountsColumn(Path.GetFileNameWithoutExtension(project.relativePath));
            }
        }

        private void AddProjectCountsColumn(string projectName)
        {
            Binding amountBinding = new Binding(string.Format("ProjectCount[{0}]", DifColumns.Count.ToString()));
            amountBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            DataTemplate amountLabel = new DataTemplate();
            FrameworkElementFactory _textboxFactory = new FrameworkElementFactory(typeof(TextBlock));

            DataGridTemplateColumn c = new DataGridTemplateColumn();
            c.Header = projectName;
            FrameworkElementFactory textFactory = new FrameworkElementFactory(typeof(Label));
            textFactory.SetValue(Label.ContentProperty, amountBinding);

            DataTemplate columnTemplate = new DataTemplate();
            columnTemplate.VisualTree = textFactory;
            c.CellTemplate = columnTemplate;
            DifColumns.Add(c);
        }
        #endregion
    }
}