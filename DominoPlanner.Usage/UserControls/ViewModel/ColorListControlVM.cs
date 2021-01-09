using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using DominoPlanner.Core;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static DominoPlanner.Usage.ColorControl;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class ProjectColorList : ColorControl
    {
        public ProjectColorList()
        {
            ProjectProperty.Changed.AddClassHandler<ColorControl>(ProjectChanged);
        }

        private void ProjectChanged(ColorControl sender, AvaloniaPropertyChangedEventArgs args)
        {
            var header = this.Find<Grid>("HeaderGrid");
            header.ColumnDefinitions.Clear();

            UpdateLayout();
        }
        internal override void UpdateLayout()
        {
            var header = this.Find<Grid>("HeaderGrid");
            header.ColumnDefinitions.Clear();
            if (Project != null)
                HeaderRow = GetDepth(Project);
            header.Children.Clear();
            FillHeader(header);
            if (Project != null)
                PopulateHeaderColumns(header, Project, 0, ColumnConfig.Count);

            var itemscontrol = this.Find<ItemsControl>("ItemsControl");
            // for content, we have to define it inside a lambda function
            var template = new FuncDataTemplate<ColorListEntry>((x, _) =>
            {
                Grid g = new Grid();
                g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                FillTemplate(g);
                if (Project != null)
                    PopulateColumns(g, Project, ColumnConfig.Count, 0);
                return g;
            });
            itemscontrol.ItemTemplate = template;
        }
        public static int GetDepth(AssemblyNode assy)
        {
            if (assy.Obj.children.OfType<AssemblyNode>().Count() == 0)
            {
                return 1;
            }
            else
            {
                return assy.Obj.children.OfType<AssemblyNode>().Max(o => GetDepth(o)) + 1;
            }
        }
        int PopulateHeaderColumns(Grid g, AssemblyNode assy, int level, int start_column)
        {
            if (g.RowDefinitions == null)
                g.RowDefinitions = new RowDefinitions();
            while (g.RowDefinitions.Count <= level + 1)
                g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            int index = start_column;
            foreach (var p in assy.Obj.children)
            {
                if (p is AssemblyNode ass_child)
                {
                    if (ass_child.ColorPathMatches(assy))
                    {
                        index += PopulateHeaderColumns(g, ass_child, level + 1, index);
                    }
                    else
                    {
                        g.ColumnDefinitions.Add(new ColumnDefinition { SharedSizeGroup = "col_" + (index), Width = GridLength.Auto });
                        var errorheader = new ContentControl()
                        {
                            Content = Path.GetFileNameWithoutExtension(ass_child.Path)
                        };
                        errorheader.Classes.Add("Header");
                        errorheader.Classes.Add("Different");
                        errorheader.Classes.Add("Assembly");
                        g.Children.Add(errorheader);
                        Grid.SetColumn(errorheader, index);
                        Grid.SetRow(errorheader, level + 1);
                        Grid.SetRowSpan(errorheader, HeaderRow - level + 1);
                        index++;
                    }

                }
                if (p is DocumentNode dn)
                {
                    var contentblock = new ContentControl();
                    contentblock.Classes.Add("Header");
                    contentblock.Classes.Add("Project");
                    g.ColumnDefinitions.Add(new ColumnDefinition { SharedSizeGroup = "col_" + (index), Width = GridLength.Auto });
                    Grid.SetColumn(contentblock, index);
                    Grid.SetRow(contentblock, level + 1);
                    Grid.SetRowSpan(contentblock, HeaderRow - (level));
                    g.Children.Add(contentblock);
                    contentblock.Content = Path.GetFileNameWithoutExtension(dn.RelativePath);

                    if (!dn.ColorPathMatches(assy))
                    {
                        contentblock.Classes.Add("Different");
                    }
                    index++;
                }
            }

            // this is the title
            var cp = new ContentControl()
            {
                Content = Path.GetFileNameWithoutExtension(assy.Path)

            };
            cp.Classes.Add("Header");
            cp.Classes.Add("Assembly");
            g.Children.Add(cp);
            Grid.SetColumn(cp, start_column);
            Grid.SetRow(cp, level);
            // this is the sum column
            g.ColumnDefinitions.Add(new ColumnDefinition { SharedSizeGroup = "col_" + (index), Width = GridLength.Auto });
            var tb = new ContentControl()
            {
                Content = "Sum"
            };
            tb.Classes.Add("Header");
            tb.Classes.Add("Sum");
            Grid.SetColumn(tb, index);
            Grid.SetRow(tb, level + 1);
            Grid.SetRowSpan(tb, HeaderRow - (level));
            g.Children.Add(tb);
            index++;
            //finish positioning
            var colspan = index - start_column; // +1 for sum column
            Grid.SetColumnSpan(cp, colspan);


            return colspan;
        }
        Tuple<int, int> PopulateColumns(Grid g, AssemblyNode assy, int start_column, int projectcount_startindex)
        {
            int index = start_column;
            int projectindex = projectcount_startindex;
            foreach (var p in assy.Obj.children)
            {
                if (p is AssemblyNode ass_child)
                {
                    if (ass_child.ColorPathMatches(assy))
                    {
                        var columns = PopulateColumns(g, ass_child, index, projectindex);
                        index += columns.Item1;
                        projectindex += columns.Item2;
                    }
                    else
                    {
                        g.ColumnDefinitions.Add(new ColumnDefinition { SharedSizeGroup = "col_" + (index), Width = GridLength.Auto });
                        index++;
                    }
                }
                if (p is DocumentNode dn)
                {
                    g.ColumnDefinitions.Add(new ColumnDefinition { SharedSizeGroup = "col_" + (index), Width = GridLength.Auto });
                    if (dn.ColorPathMatches(assy))
                    {
                        var contentblock = new ContentControl()
                        {
                            [!ContentProperty] = new Binding("ProjectCount[" + (projectindex) + "]"),
                        };
                        contentblock.Classes.Add("Content");
                        contentblock.Classes.Add("Project");
                        Grid.SetColumn(contentblock, index);
                        g.Children.Add(contentblock);
                        projectindex++;
                    }
                    index++;
                }
            }

            // this is the sum column
            g.ColumnDefinitions.Add(new ColumnDefinition { SharedSizeGroup = "col_" + (index), Width = GridLength.Auto });
            var tb = new ContentControl()
            {
                [!ContentControl.ContentProperty] = new Binding("ProjectCount[" + (projectindex) + "]")
            };
            tb.Classes.Add("Content");
            tb.Classes.Add("Sum");
            Grid.SetColumn(tb, index);
            g.Children.Add(tb);
            index++;
            projectindex++;

            return new Tuple<int, int>(index - start_column, projectindex - projectcount_startindex);
        }


        public AssemblyNode Project
        {
            get { return (AssemblyNode)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Project.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<AssemblyNode> ProjectProperty =
            AvaloniaProperty.Register<ColorControl, AssemblyNode>("Project");


    }
    public class ColorListControlVM : TabBaseVM
    {
        #region CTOR
        public ColorListControlVM(string filepath) : base()
        {
            ColorList = new ObservableCollection<ColorListEntry>();

            Reload(filepath);

            BtnAddColor = new RelayCommand(o => AddNewColor(false));
            BtnAddColorMix = new RelayCommand(o => AddNewColor(true));
            BtnSaveColors = new RelayCommand(o => { Save(); });
            BtnRemove = new RelayCommand(o => { RemoveSelected(); });
            BtnMoveDown = new RelayCommand(o => { Move(false); });
            BtnMoveUp = new RelayCommand(o => { Move(true); });
            BtnExportXLSX = new RelayCommand(o => { ExportXLSX(); });
            base.UnsavedChanges = false;

            ColumnConfig = new AvaloniaList<Column>
            {
                new Column() { DataField = ".", Header = "", Class = "Color", CanResize = false },
                new Column() { DataField = "Name", Header = "Name", Class = "Name",  CanResize = true, Width = new GridLength(100) },
                new Column() { DataField = ".", Header = "RGB", Class = "RGB",   CanResize = true, Width= new GridLength(70)   },
                new Column() { DataField = "Count", Header = "Count", Class="Count", CanResize = true, Width = new GridLength(70) }
            };

            ShowProjects = false;
        }

        public ColorListControlVM(AssemblyNode dominoAssembly) : this(Workspace.AbsolutePathFromReferenceLoseUpdate(dominoAssembly.Obj.ColorPath, dominoAssembly.Obj))
        {
            this.DominoAssembly = dominoAssembly;
        }
        #endregion

        #region Methods
        public void ResetList()
        {
            FilePath = String.Empty;
            ColorRepository.Anzeigeindizes.CollectionChanged -= Anzeigeindizes_CollectionChanged;
            ColorRepository = new ColorRepository();
            ColorRepository.Anzeigeindizes.CollectionChanged += Anzeigeindizes_CollectionChanged;
            _ColorList.Clear();
            RefreshList();
            UnsavedChanges = false;
        }

        internal void Reload(string fileName)
        {
            FilePath = fileName;
            _ColorList.Clear();
            if (ColorRepository != null) ColorRepository.Anzeigeindizes.CollectionChanged -= Anzeigeindizes_CollectionChanged;
            ColorRepository = Workspace.Load<ColorRepository>(FilePath);
            ColorRepository.Anzeigeindizes.CollectionChanged += Anzeigeindizes_CollectionChanged;
            RefreshList();
            UnsavedChanges = false;
        }

        private void RefreshList()
        {
            int counter = 0;
            
            if (ColorRepository.RepresentionForCalculation.OfType<EmptyDomino>().Count() == 1)
            {
                _ColorList.Add(new ColorListEntry() { DominoColor = ColorRepository.RepresentionForCalculation.OfType<EmptyDomino>().First(), SortIndex = -1, ValueChanged = PropertyValueChanged });
            }
            foreach (IDominoColor domino in ColorRepository.RepresentionForCalculation.OfType<IDominoColor>())
            {
                if (domino is EmptyDomino)
                    continue;
                var entry = ColorListEntry.ColorListEntryFactory(domino, ColorRepository, 0);
                entry.ValueChanged = PropertyValueChanged;
                entry.SortIndex = ColorRepository.Anzeigeindizes[counter];
                _ColorList.Add(entry);
                counter++;
            }
        }
        private async void ExportXLSX()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var p = new ExcelPackage();
            // content
            var ws = p.Workbook.Worksheets.Add("Overview");
            ws.Cells["A1"].Value = "Color Usage Overview";
            ws.Cells["A1"].Style.Font.Size = 15;
            var HeaderRow = ProjectColorList.GetDepth(DominoAssembly);
            // 
            var offset = HeaderRow + 4;
            ws.Cells[offset - 1, 2].Value = "Color";
            ws.Cells[offset - 1, 3].Value = "Available";
            for (int i = 0; i < ColorList.Count; i++)
            {

                ws.Cells[offset + i, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[offset + i, 1].Style.Fill.BackgroundColor.SetColor(ColorList[i].DominoColor.mediaColor.ToSD());
                ws.Cells[offset + i, 2].Value = ColorList[i].DominoColor.name;
                ws.Cells[offset + i, 3].Value = ColorList[i].DominoColor.count;
            }
            ws.Cells[offset, 3].Value = ""; // Count of empty domino

            var length = ExportAssemblyToExcel(ws, DominoAssembly, 0, 0, 0, HeaderRow);
            int index = length.Item1;


            // add lines 
            ws.Cells[3, 3, 4 + ColorList.Count + HeaderRow, 3 + index].Style.Border.Right.Style
                = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            ws.Cells[2, 1, 3 + ColorList.Count + HeaderRow, 3 + index].Style.Border.Bottom.Style
                = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            ws.Cells[3 + HeaderRow, 1, 3 + HeaderRow, 3 + index].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
            ws.Cells[3 + HeaderRow + ColorList.Count, 1, 3 + HeaderRow + ColorList.Count, 3 + index].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
            ws.Calculate();
            // auto fit unfortunately doesn't work for merged cells (project headers)
            //ws.Cells.AutoFitColumns();
            
            
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialFileName = "ColorList";
            dlg.Filters = new List<FileDialogFilter>() {
                new FileDialogFilter() { Extensions = new List<string> { "xlsx" }, Name = "Excel files" },
                new FileDialogFilter() { Extensions = new List<string> { "*" }, Name = "All files" }
            };
            var result = await dlg.ShowAsync(MainWindowViewModel.GetWindow());
            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    p.SaveAs(new FileInfo(result));
                    var process = new Process();
                    process.StartInfo = new ProcessStartInfo(result) { UseShellExecute = true };
                    process.Start();
                }
                catch (Exception ex)
                {
                    await Errorhandler.RaiseMessage("Save failed:" + ex.Message, "Fail", Errorhandler.MessageType.Error);
                }
            }

        }
        private ValueTuple<int, int> ExportAssemblyToExcel(ExcelWorksheet ws, AssemblyNode assy, int level, int start_column, int projectcount_startindex, int header_rows)
        {
            int projectindex = projectcount_startindex;
            int index = start_column;
            int top_offset;
            int left_offset;
            foreach (var p in assy.Obj.children)
            {
                top_offset = 3 + level + 1;
                left_offset = 4 + index;
                var header_cell = ws.Cells[top_offset, left_offset];
                if (p is AssemblyNode ass_child)
                {
                    if (ass_child.ColorPathMatches(assy))
                    {
                        var subresult = ExportAssemblyToExcel(ws, ass_child, level + 1, index, projectindex, header_rows);
                        index += subresult.Item1;
                        projectindex += subresult.Item2;
                    }
                    else
                    {
                        header_cell.Value = Path.GetFileNameWithoutExtension(ass_child.Path);
                        header_cell.Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        ws.Cells[top_offset, left_offset, header_rows + 3, left_offset].Merge = true;
                        index++;
                    }

                }
                if (p is DocumentNode dn)
                {
                    header_cell.Value = Path.GetFileNameWithoutExtension(dn.RelativePath);
                    if (!dn.ColorPathMatches(assy))
                    {
                        header_cell.Style.Font.Color.SetColor(System.Drawing.Color.Red);

                    }
                    else
                    {
                        for (int i = 0; i < ColorList.Count; i++)
                        {
                            if (projectindex < ColorList[i].ProjectCount.Count)
                            {
                                ws.Cells[header_rows + 4 + i, left_offset].Value = ColorList[i].ProjectCount[projectindex];
                            }
                        }
                        ws.Cells[header_rows + 4 + ColorList.Count, left_offset].Formula = $"SUM({ws.Cells[header_rows + 5, left_offset].Address}:{ws.Cells[header_rows + 3 + ColorList.Count, left_offset].Address})";
                        projectindex++;
                    }
                    ws.Cells[top_offset, left_offset, header_rows + 3, left_offset].Merge = true;
                    header_cell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom;
                    index++;
                    
                }
            }
            // add assembly header
            top_offset = 3 + level;
            left_offset = 4 + start_column;
            // set assemby name
            
            ws.Cells[top_offset, left_offset].Value = Path.GetFileNameWithoutExtension(assy.Path);
            
            // join assembly cells
            var colspan = index - start_column; // +1 for sum column
            ws.Cells[top_offset, left_offset, top_offset, left_offset + colspan].Merge = true;
            ws.Cells[top_offset, left_offset].Style.Font.Bold = true;
            // sum column
            int sum_col = left_offset + colspan;
            ws.Cells[top_offset + 1, sum_col].Value = "Sum";
            ws.Cells[top_offset + 1, sum_col, header_rows + 3, sum_col].Merge = true;
            ws.Cells[top_offset + 1, sum_col].Style.Font.Italic = true;
            for (int i = 0; i < ColorList.Count; i++)
            {
                if (projectindex < ColorList[i].ProjectCount.Count)
                {
                    ws.Cells[header_rows + 4 + i, sum_col].Value = ColorList[i].ProjectCount[projectindex];
                    ws.Cells[header_rows + 4 + i, sum_col].Style.Font.Italic = true;
                }
            }
            ws.Cells[header_rows + 4 + ColorList.Count, sum_col].Formula = $"SUM({ws.Cells[header_rows + 5, sum_col].Address}:{ws.Cells[header_rows + 3 + ColorList.Count, sum_col].Address})";
            index++;
            projectindex++;
            return (index - start_column, projectindex - projectcount_startindex);
        }
        private void RemoveSelected()
        {
            if (SelectedStone.DominoColor is IDominoColor dominoColor && !(dominoColor is EmptyDomino))
            {
                DeleteColorOperation op = new DeleteColorOperation(SelectedStone);
                op.Apply();
                undoStack.Push(op);
                UnsavedChanges = true;
            }
        }
        private void Move(bool up)
        {
            try
            {
                if (SelectedStone.DominoColor is IDominoColor dominoColor && !(dominoColor is EmptyDomino))
                {
                    MoveColorOperation op = new MoveColorOperation(ColorRepository, dominoColor, up);
                    op.Apply();
                    if (op.valid)
                    {
                        undoStack.Push(op);
                        UnsavedChanges = true;
                    }
                }
                TabPropertyChanged("ColorList", ProducesUnsavedChanges: false);
            }
            catch (Exception) { }
        }

        private void AddNewColor(bool colorMix)
        {
            AddColorOperation op = new AddColorOperation(ColorRepository, _ColorList, colorMix);
            op.Apply();
            if (op.added != null)
            {
                op.added.ValueChanged = PropertyValueChanged;
                SelectedStone = op.added;
            }
            undoStack.Push(op);
            UnsavedChanges = true;
        }

        public override bool Save()
        {
            if (FilePath == string.Empty)
            {
                SaveFileDialog ofd = new SaveFileDialog
                {
                    DefaultExtension = Properties.Settings.Default.ColorExtension
                };
                ofd.Filters.Add(new FileDialogFilter() { Extensions = new List<string> { Properties.Settings.Default.ColorExtension }, Name = "Color files" });
                ofd.Filters.Add(new FileDialogFilter() { Extensions = new List<string> { "*" }, Name = "All files" });
                var filename = ofd.ShowDialog();
                if (filename != null && filename != "")
                {
                    if (filename != string.Empty)
                    {
                        FilePath = filename;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            ColorRepository.Save(FilePath);
            UnsavedChanges = false;
            return true;
        }

        private void Anzeigeindizes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < ColorRepository.Anzeigeindizes.Count; i++)
            {
                _ColorList[i+1].SortIndex = ColorRepository.Anzeigeindizes[i];
            }
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

        private ICommand _BtnAddColorMix;
        public ICommand BtnAddColorMix { get { return _BtnAddColorMix; } set { if (value != _BtnAddColorMix) { _BtnAddColorMix = value; } } }

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
        private AvaloniaList<Column> _columnConfig;

        public AvaloniaList<Column> ColumnConfig
        {
            get { return _columnConfig; }
            set { _columnConfig = value; RaisePropertyChanged(); }
        }

        private AssemblyNode dominoAssembly;

        public AssemblyNode DominoAssembly
        {
            get { return dominoAssembly; }
            set { dominoAssembly = value; ResetContent(); RaisePropertyChanged(); }
        }
        private ColorListEntry _SelectedStone;
        public ColorListEntry SelectedStone
        {
            get { return _SelectedStone; }
            set
            {
                _SelectedStone = value;
                TabPropertyChanged(ProducesUnsavedChanges: false);
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
        public ColorRepository ColorRepository
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
                    if (_ColorList != null) _ColorList.CollectionChanged -= ColorList_CollectionChanged;
                    _ColorList = value;
                    _ColorList.CollectionChanged += ColorList_CollectionChanged;
                    RaisePropertyChanged();
                }
            }
        }

        private void ColorList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("ColorList");
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
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }
        private string _warningLabelText;

        public string WarningLabelText
        {
            get { return _warningLabelText; }
            set { _warningLabelText = value; TabPropertyChanged(ProducesUnsavedChanges: false); }
        }
        async Task<int[]> AddProjectCounts(AssemblyNode assy)
        {
            int[] sum = new int[assy.Obj.Colors.Length];
            foreach (var child in assy?.Obj.children)
            {
                if (child is AssemblyNode child_as)
                {
                    try
                    {
                        if (Path.GetFullPath(child_as.Obj.AbsoluteColorPath) == Path.GetFullPath(assy.Obj.AbsoluteColorPath))
                        {
                            sum = sum.Zip(await AddProjectCounts(child_as), (x, y) => x + y).ToArray();
                        }
                    }
                    catch
                    {
                        await Errorhandler.RaiseMessage($"Unable to load counts from project {Path.GetFileNameWithoutExtension(child_as.Path)}.", "Error", Errorhandler.MessageType.Warning);
                    }
                }
                if (child is DocumentNode dn)
                {
                    try
                    {
                        string relpath = dn.RelativePath;
                        var counts2 = Workspace.LoadColorList<IDominoProviderPreview>(ref relpath, assy.Obj);
                        dn.RelativePath = relpath;
                        if (Path.GetFullPath(counts2.Item1) == Path.GetFullPath(assy.Obj.AbsoluteColorPath))
                        {
                            sum = sum.Zip(counts2.Item2, (x, y) => x + y).ToArray();
                            for (int i = 0; i < counts2.Item2.Length; i++)
                            {
                                if (i < _ColorList.Count)
                                {
                                    _ColorList[i].ProjectCount.Add(counts2.Item2[i]);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await Errorhandler.RaiseMessage($"Unable to load counts from project {Path.GetFileNameWithoutExtension(dn.RelativePath)}.", "Error", Errorhandler.MessageType.Warning);
                    }
                }
            }
            for (int i = 0; i < sum.Length; i++)
            {
                if (i < _ColorList.Count)
                {
                    _ColorList[i].ProjectCount.Add(sum[i]);
                }
            }
            return sum;
        }

        internal override async void ResetContent()
        {
            ShowProjects = true;
            foreach (ColorListEntry cle in _ColorList)
            {
                cle.ProjectCount.Clear();
            }
            await AddProjectCounts(DominoAssembly);

        }
        #endregion
    }
    public class DeletedColorVisibilityConverter : IMultiValueConverter
    {
        // Colors can have three states:
        // - used, but deleted: -> gray, but visible
        // - not deleted -> black
        // - deleted and not used -> invisible
        // deleted colors won't be used anymore
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool deleted)
                if (!deleted)
                    return true;
            if (values[1] is ObservableCollection<int> counts)
            {
                foreach (int count in counts)
                    if (count > 0)
                        return true;
            }
            return false;
        }
    }
    public class MoveColorOperation : PostFilter
    {
        private readonly ColorRepository repo;
        private readonly IDominoColor stoneToMove;
        private readonly bool up;
        public bool valid = true;
        public MoveColorOperation(ColorRepository repo, IDominoColor stoneToMove, bool up)
        {
            this.repo = repo;
            this.stoneToMove = stoneToMove;
            this.up = up;
        }
        public override void Apply()
        {
            try
            {
                if (up)
                    repo.MoveUp(stoneToMove);
                else
                    repo.MoveDown(stoneToMove);
            }
            catch (InvalidOperationException)
            {
                valid = false;
            }
        }

        public override void Undo()
        {
            try
            {
                if (valid)
                {
                    if (up)
                        repo.MoveDown(stoneToMove);
                    else
                        repo.MoveUp(stoneToMove);
                }
            }
            catch { }
        }
    }
    public class AddColorOperation : PostFilter
    {
        private readonly ColorRepository repo;
        private readonly ObservableCollection<ColorListEntry> _ColorList;
        internal ColorListEntry added = null;
        private readonly bool IsColorMix;
        public AddColorOperation(ColorRepository repo, ObservableCollection<ColorListEntry> _ColorList, bool isColorMix)
        {
            this.repo = repo;
            this._ColorList = _ColorList;
            this.IsColorMix = isColorMix;
        }
        public override void Apply()
        {
            if (added == null)
            {
                IDominoColor result;
                if (!IsColorMix)
                {
                    result = new DominoColor(Avalonia.Media.Colors.IndianRed, 0, "New Color");
                }
                else
                {
                    result = new ColorMixColor() { name = "Colormix" };
                }
                repo.Add(result);
                added = ColorListEntry.ColorListEntryFactory(result, repo, _ColorList[0].ProjectCount.Count);
                _ColorList.Add(added);
            }
            else
            {
                // redo
                added.Deleted = false;
            }
        }

        public override void Undo()
        {
            added.Deleted = true;
        }
    }
    public class DeleteColorOperation : PostFilter
    {
        private readonly ColorListEntry entry;
        private readonly bool OldState;
        public DeleteColorOperation(ColorListEntry entry)
        {
            this.entry = entry;
            this.OldState = entry.Deleted;
        }

        public override void Apply()
        {
            entry.Deleted = !this.OldState;
        }

        public override void Undo()
        {
            entry.Deleted = this.OldState;
        }
    }

}