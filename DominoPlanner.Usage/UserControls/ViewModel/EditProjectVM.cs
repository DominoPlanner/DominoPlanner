using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class EditProjectVM : DominoProviderTabItem
    {
        #region CTOR
        public EditProjectVM(IDominoProvider dominoProvider) : base()
        {
            HaveBuildtools = dominoProvider.HasProtocolDefinition ? Visibility.Visible : Visibility.Hidden;

            UICursor = null;
            selectedDominoes = new List<int>();
            UnsavedChanges = false;
            CurrentProject = dominoProvider;
            
            _DominoList = new ObservableCollection<ColorListEntry>();

            _DominoList.Clear();
            CurrentProject.colors.Anzeigeindizes.CollectionChanged += Anzeigeindizes_CollectionChanged;
            refreshList();
            selectedColors = new int[CurrentProject.colors.Length];
            SaveField = new RelayCommand(o => { Save(); });
            RestoreBasicSettings = new RelayCommand(o => { redoStack = new Stack<PostFilter>(); Editing = false; });
            
            SelectColor = new RelayCommand(o => { SelectAllStonesWithColor(); });
            MouseClickCommand = new RelayCommand(o => { ChangeColor(); });
            ClearSelection = new RelayCommand(o => { ClearFullSelection(); });
            CopyCom = new RelayCommand(o => { Copy(); });
            PasteCom = new RelayCommand(o => { Paste(); });

            AddRowAbove = new RelayCommand(o => { AddRow(false); });
            AddRowBelow = new RelayCommand(o => { AddRow(true); });
            AddColumnRight = new RelayCommand(o => { AddColumn(true); });
            AddColumnLeft = new RelayCommand(o => { AddColumn(false); });
            RemoveRows = new RelayCommand(o => { RemoveSelRows(); });
            RemoveColumns = new RelayCommand(o => { RemoveSelColumns(); });
            FlipHorizontallyCom = new RelayCommand(o => { System.Diagnostics.Debug.WriteLine("asdf"); ; });
            FlipVerticallyCom = new RelayCommand(o => { System.Diagnostics.Debug.WriteLine("asdf"); ; });
            MouseInPicture = new RelayCommand(o => { UICursor = Cursors.Hand; });
            MouseOutPicture = new RelayCommand(o => { UICursor = null; });
            SelectAllCom = new RelayCommand(o => { SelectAll(); });
            UnsavedChanges = false;
            SelectionTool = new SelectionToolVM(this);
            DisplaySettingsTool = new DisplaySettingsToolVM(this);
            EditingTools = new ObservableCollection<EditingToolVM>() {
                SelectionTool,
                new EditingToolVM() {Image = "ruler2DrawingImage", Name = "Measure distance"},
                new EditingToolVM() {Image = "add_delete_rowDrawingImage", Name="Add or delete rows and columns" },
                new EditingToolVM() { Image = "textDrawingImage", Name="Write text"},
                new EditingToolVM() {Image = "fill_bucketDrawingImage", Name="Fill area" },
                new EditingToolVM() {Image= "zoomDrawingImage", Name = "Zoom" },
                DisplaySettingsTool
            };
            SelectedTool = SelectionTool;
            UpdateUIElements();

        }
        #endregion

        #region fields
        internal List<int> selectedDominoes;
        private int[] selectedColors;
        private int startindex;
        internal DominoTransfer dominoTransfer;

        private SelectionToolVM SelectionTool;
        public DisplaySettingsToolVM DisplaySettingsTool { get; set; }
        #endregion

        #region events
        internal event EventHandler RefreshSize;
        #endregion

        #region prope

        private ObservableCollection<EditingToolVM> editingTools;

        public ObservableCollection<EditingToolVM> EditingTools
        {
            get { return editingTools; }
            set { editingTools = value; TabPropertyChanged(ProducesUnsavedChanges: false); }
        }
        private EditingToolVM selectedTool;

        public EditingToolVM SelectedTool
        {
            get { return selectedTool; }
            set
            {
                if (value != null)
                    selectedTool = value;
                TabPropertyChanged(ProducesUnsavedChanges: false);

            }
        }


        private Visibility _HaveBuildtools;
        public Visibility HaveBuildtools
        {
            get { return _HaveBuildtools; }
            set
            {
                if (_HaveBuildtools != value)
                {
                    _HaveBuildtools = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }
        

        private Cursor _UICursor;
        public Cursor UICursor
        {
            get { return _UICursor; }
            set
            {
                if (_UICursor != value)
                {
                    _UICursor = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }
        
        public override TabItemType tabType
        {
            get
            {
                return TabItemType.EditProject;
            }
        }
        private ObservableCollection<ColorListEntry> _DominoList;
        public ObservableCollection<ColorListEntry> DominoList
        {
            get { return new ObservableCollection<ColorListEntry>(_DominoList.OrderBy(x => x.SortIndex)); }
            set
            {
                if (_DominoList != value)
                {
                    _DominoList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ColorListEntry _SelectedColor;
        public ColorListEntry SelectedColor
        {
            get { return _SelectedColor; }
            set
            {
                if (_SelectedColor != value)
                {
                    _SelectedColor = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }
        private string _ProjectName;
        public string ProjectName
        {
            get { return _ProjectName; }
            set
            {
                if (_ProjectName != value)
                {
                    _ProjectName = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }

        private string _ProjectAmount;
        public string ProjectAmount
        {
            get { return _ProjectAmount; }
            set
            {
                if (_ProjectAmount != value)
                {
                    _ProjectAmount = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }

        private string _ProjectHeight;
        public string ProjectHeight
        {
            get { return _ProjectHeight; }
            set
            {
                if (_ProjectHeight != value)
                {
                    _ProjectHeight = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }

        private string _ProjectWidth;
        public string ProjectWidth
        {
            get { return _ProjectWidth; }
            set
            {
                if (_ProjectWidth != value)
                {
                    _ProjectWidth = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }

        private ColumnConfig _colorColumnConfig;

        public ColumnConfig ColorColumnConfig
        {
            get { return _colorColumnConfig; }
            set { _colorColumnConfig = value; }
        }

        #endregion

        #region Methods
        internal override void Close()
        {
            base.Close();

            DisplaySettingsTool.DeleteImage();

            ClearCanvas();
        }

        internal void ClearCanvas()
        {
            DisplaySettingsTool.ClearPastePositions();
            ClearFullSelection();
            if (DisplaySettingsTool.DominoProject != null)
                DisplaySettingsTool.RemoveStones();
        }

        private void Anzeigeindizes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    _DominoList.Where(x => x.DominoColor is DominoColor).ElementAt(e.NewStartingIndex).SortIndex = (int)e.NewItems[0];
                    break;
            }
            UnsavedChanges = false;
            RaisePropertyChanged("DominoList");
        }
        public void UpdateUIElements()
        {
            RefreshColorAmount();
            DisplaySettingsTool.Redraw();
        }
        private void RefreshColorAmount()
        {
            var projectCounts = CurrentProject.Counts;
            for (int i = 0; i < _DominoList.Count(); i++)
            {
                if (_DominoList[i].ProjectCount.Count != 2)
                {
                    _DominoList[i].ProjectCount.Clear();
                    // add dummy entries
                    _DominoList[i].ProjectCount.Add(0);
                    _DominoList[i].ProjectCount.Add(0);
                }

                if (projectCounts.Length > i + 1)
                {
                    _DominoList[i].ProjectCount[0] = projectCounts[i + 1];
                    _DominoList[i].ProjectCount[1] = selectedColors[i + 1];
                }
                else
                {
                    _DominoList[i].ProjectCount[0] = projectCounts[0];
                    _DominoList[i].ProjectCount[1] = selectedColors[0];
                }
            }
        }
        private void refreshList()
        {
            // Setup Columns
            ColorColumnConfig = new ColumnConfig();

            var columns = new ObservableCollection<Column>();
            columns.Add(new Column() { DataField = "DominoColor.mediaColor", Header = "" });
            columns.Add(new Column() { DataField = "DominoColor.name", Header = "Name" });
            columns.Add(new Column() { DataField = "DominoColor.count", Header = "Total" });
            columns.Add(new Column() { DataField = "ProjectCount[0]", Header = "Used", HighlightDataField= "DominoColor.count" });
            columns.Add(new Column() { DataField = "ProjectCount[1]", Header = "Selected" });
            ColorColumnConfig.Columns = columns;
            
            _DominoList.Clear();
            int counter = 0;

            foreach (DominoColor domino in CurrentProject.colors.RepresentionForCalculation.OfType<DominoColor>())
            {
                _DominoList.Add(new ColorListEntry() { DominoColor = domino, SortIndex = CurrentProject.colors.Anzeigeindizes[counter] });
                counter++;
            }

            if (CurrentProject.colors.RepresentionForCalculation.OfType<EmptyDomino>().Count() == 1)
            {
                _DominoList.Add(new ColorListEntry() { DominoColor = CurrentProject.colors.RepresentionForCalculation.OfType<EmptyDomino>().First(), SortIndex = -1 });
            }
        }
        private void SelectAll()
        {
            for (int i = 0; i < dominoTransfer.length; i++)
            {
                AddToSelectedDominoes(i);
            }
        }
        List<int> toCopy = new List<int>();
        private void Copy()
        {
            if (!(CurrentProject is ICopyPasteable)) Errorhandler.RaiseMessage("Could not copy in this project.", "Copy", Errorhandler.MessageType.Warning);
            DisplaySettingsTool.ClearPastePositions();
            if (selectedDominoes.Count < 0)
            {
                Errorhandler.RaiseMessage("Nothing to copy!", "No selection", Errorhandler.MessageType.Error);
                return;
            }
            toCopy = new List<int>(selectedDominoes);
            startindex = selectedDominoes.Min();
            ClearFullSelection();
            try
            {
                int[] validPositions = ((ICopyPasteable)this.CurrentProject).GetValidPastePositions(startindex);
                DisplaySettingsTool.HighlightPastePositions(validPositions);
            }
            catch (InvalidOperationException ex)
            {
                Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
            UpdateUIElements();
        }

        private void Paste()
        {
            try
            {
                if (!(CurrentProject is ICopyPasteable)) Errorhandler.RaiseMessage("Could not paste in this project.", "Paste", Errorhandler.MessageType.Warning);
                if (selectedDominoes.Count == 0) return;
                int pasteindex = selectedDominoes.First();
                RemoveFromSelectedDominoes(pasteindex);
                selectedDominoes.Clear();
                PasteFilter paste = new PasteFilter(CurrentProject as ICopyPasteable, startindex, toCopy.ToArray(), pasteindex);
                paste.Apply();
                undoStack.Push(paste);
                DisplaySettingsTool.ClearPastePositions();
                UpdateUIElements();
            }
            catch (InvalidOperationException ex)
            {
                Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
        }

        public override void Undo()
        {
            if (undoStack.Count == 0) return;
            PostFilter undoFilter = undoStack.Pop();
            redoStack.Push(undoFilter);
            undoFilter.Undo();
            if (!(undoFilter is EditingActivatedOperation))
            {
                ClearCanvas();
                DisplaySettingsTool.ResetCanvas();
                if (undoStack.Count == 0) UnsavedChanges = false;
            }
        }
        public override void Redo()
        {
            if (redoStack.Count == 0) return;
            PostFilter redoFilter = redoStack.Pop();
            undoStack.Push(redoFilter);
            redoFilter.Apply();

            //if (!(redoFilter is SetColorOperation || redoFilter is PasteFilter))
            if (!(redoFilter is EditingDeactivatedOperation))
            {
                ClearCanvas();
                DisplaySettingsTool.ResetCanvas();
            }
        }
        internal void PressedKey(Key key)
        {
            ClearFullSelection();
        }
        internal override void ResetContent()
        {
            base.ResetContent();
            RefreshSize?.Invoke(this, EventArgs.Empty);
        }
        private void ChangeColor()
        {
            SetColorOperation sco = new SetColorOperation(CurrentProject, selectedDominoes.ToArray(), CurrentProject.colors.RepresentionForCalculation.ToList().IndexOf(SelectedColor.DominoColor));
            undoStack.Push(sco);
            sco.Apply();
            ClearFullSelection();
            UnsavedChanges = true;
            UpdateUIElements();
        }

        private void AddRow(bool addBelow)
        {
            try
            {
                if (selectedDominoes.Count > 0)
                {
                    int selDomino = selectedDominoes.First();
                    if (CurrentProject is IRowColumnAddableDeletable)
                    {
                        AddRows addRows = new AddRows((CurrentProject as IRowColumnAddableDeletable), selDomino, 1, dominoTransfer[selDomino].color, addBelow);
                        addRows.Apply();
                        undoStack.Push(addRows);
                        ClearCanvas();
                        DisplaySettingsTool.ResetCanvas();
                        foreach (int i in addRows.added_indizes)
                            AddToSelectedDominoes(i);
                        UpdateUIElements();
                    }
                    else
                    {
                        Errorhandler.RaiseMessage("Could not add a row in this project.", "Add Row", Errorhandler.MessageType.Warning);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
        }

        private void AddColumn(bool addRight)
        {
            try
            {
                if (selectedDominoes.Count > 0)
                {
                    int selDomino = selectedDominoes.First();
                    if (CurrentProject is IRowColumnAddableDeletable)
                    {
                        AddColumns addRows = new AddColumns((CurrentProject as IRowColumnAddableDeletable), selDomino, 1, dominoTransfer[selDomino].color, addRight);
                        addRows.Apply();
                        undoStack.Push(addRows);
                        ClearCanvas();
                        DisplaySettingsTool.ResetCanvas();
                        foreach (int i in addRows.added_indizes)
                            AddToSelectedDominoes(i);
                        UpdateUIElements();
                    }
                    else
                    {
                        Errorhandler.RaiseMessage("Could not add a row in this project.", "Add Row", Errorhandler.MessageType.Warning);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
        }

        private void RemoveSelRows()
        {
            try
            {
                if (CurrentProject is IRowColumnAddableDeletable)
                {
                    if (selectedDominoes.Count > 0)
                    {
                        DeleteRows deleteRows = new DeleteRows((CurrentProject as IRowColumnAddableDeletable), selectedDominoes.ToArray());
                        deleteRows.Apply();
                        undoStack.Push(deleteRows);
                        ClearCanvas();
                        DisplaySettingsTool.ResetCanvas();
                    }
                }
                else
                {
                    Errorhandler.RaiseMessage("Could not remove a row in this project.", "Remove Row", Errorhandler.MessageType.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
        }

        private void RemoveSelColumns()
        {
            try
            {
                if (CurrentProject is IRowColumnAddableDeletable)
                {
                    if (selectedDominoes.Count > 0)
                    {
                        DeleteColumns deleteColumns = new DeleteColumns((CurrentProject as IRowColumnAddableDeletable), selectedDominoes.ToArray());
                        deleteColumns.Apply();
                        undoStack.Push(deleteColumns);
                        ClearCanvas();
                        DisplaySettingsTool.ResetCanvas();
                    }
                }
                else
                {
                    Errorhandler.RaiseMessage("Could not remove a column in this project.", "Remove Column", Errorhandler.MessageType.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
        }
        internal void ClearFullSelection()
        {
            foreach (int i in selectedDominoes.ToArray())
            { 
                RemoveFromSelectedDominoes(i);
            }
            selectedColors = new int[CurrentProject.colors.Length];
            DisplaySettingsTool.Redraw();
            SelectionTool.CurrentSelectionDomain.ResetSelectionArea();
            RefreshColorAmount();
        }

        private void RefreshSizeLabels()
        {
            ProjectHeight = dominoTransfer.FieldPlanHeight.ToString();
            ProjectWidth = dominoTransfer.FieldPlanLength.ToString();
            ProjectAmount = dominoTransfer.shapes.Count().ToString();
            PhysicalLength = dominoTransfer.physicalLength;
            PhysicalHeight = dominoTransfer.physicalHeight;
        }

        private void Dic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DominoInCanvas dic = (DominoInCanvas)sender;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!selectedDominoes.Contains(dic.idx))
                {
                    AddToSelectedDominoes(dic.idx);
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                RemoveFromSelectedDominoes(dic.idx);
            }
        }
        private void SelectAllStonesWithColor()
        {
            if (SelectedColor == null) return;
            var selectedIndex = CurrentProject.colors.RepresentionForCalculation.ToList().IndexOf(SelectedColor.DominoColor);
            IEnumerable<int> oldSelection = selectedDominoes.ToArray();
            if (oldSelection.Count() == 0)
            {
                oldSelection = Enumerable.Range(0, dominoTransfer.length);
            }
            ClearFullSelection();
            IEnumerable<int> newSelection = oldSelection.Where(x => dominoTransfer[x].color == selectedIndex);
            foreach (int i in newSelection)
            {
                AddToSelectedDominoes(i);
            }
            UpdateUIElements();
        }

        internal void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedTool?.MouseDown(sender, e);
        }

        internal void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            SelectedTool?.MouseMove(sender, e);
        }

        internal void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectedTool?.MouseUp(sender, e);
            UpdateUIElements();
        }
        public void AddToSelectedDominoes(int i)
        {
            if (DisplaySettingsTool.SelectDominoVisual(i))
            { 
                selectedDominoes.Add(i);
                selectedColors[dominoTransfer[i].color]++;
            }
        }
        public void RemoveFromSelectedDominoes(int i)
        {
            if (DisplaySettingsTool.DeSelectDominoVisual(i))
            {
                selectedDominoes.Remove(i);
                selectedColors[dominoTransfer[i].color]--;
            }
        }
        #endregion

        #region Command
        private ICommand _ClearSelection;
        public ICommand ClearSelection { get { return _ClearSelection; } set { if (value != _ClearSelection) { _ClearSelection = value; } } }

        private ICommand _SelectColor;
        public ICommand SelectColor { get { return _SelectColor; } set { if (value != _SelectColor) { _SelectColor = value; } } }

        private ICommand _SaveField;
        public ICommand SaveField { get { return _SaveField; } set { if (value != _SaveField) { _SaveField = value; } } }

        private ICommand _RestoreBasicSettings;
        public ICommand RestoreBasicSettings { get { return _RestoreBasicSettings; } set { if (value != _RestoreBasicSettings) { _RestoreBasicSettings = value; } } }

        
        private ICommand _MouseClickCommand;
        public ICommand MouseClickCommand { get { return _MouseClickCommand; } set { if (value != _MouseClickCommand) { _MouseClickCommand = value; } } }

        private ICommand _GridSizeChanged;
        public ICommand GridSizeChanged { get { return _GridSizeChanged; } set { if (value != _GridSizeChanged) { _GridSizeChanged = value; } } }

        private ICommand _AddRowAbove;
        public ICommand AddRowAbove { get { return _AddRowAbove; } set { if (value != _AddRowAbove) { _AddRowAbove = value; } } }

        private ICommand _AddRowBelow;
        public ICommand AddRowBelow { get { return _AddRowBelow; } set { if (value != _AddRowBelow) { _AddRowBelow = value; } } }

        private ICommand _AddColumnRight;
        public ICommand AddColumnRight { get { return _AddColumnRight; } set { if (value != _AddColumnRight) { _AddColumnRight = value; } } }

        private ICommand _AddColumnLeft;
        public ICommand AddColumnLeft { get { return _AddColumnLeft; } set { if (value != _AddColumnLeft) { _AddColumnLeft = value; } } }

        private ICommand _RemoveRows;
        public ICommand RemoveRows { get { return _RemoveRows; } set { if (value != _RemoveRows) { _RemoveRows = value; } } }

        private ICommand _RemoveColumns;
        public ICommand RemoveColumns { get { return _RemoveColumns; } set { if (value != _RemoveColumns) { _RemoveColumns = value; } } }

        private ICommand _CopyCom;
        public ICommand CopyCom { get { return _CopyCom; } set { if (value != _CopyCom) { _CopyCom = value; } } }

        private ICommand _PasteCom;
        public ICommand PasteCom { get { return _PasteCom; } set { if (value != _PasteCom) { _PasteCom = value; } } }

        private ICommand _FlipHorizontallyCom;
        public ICommand FlipHorizontallyCom { get { return _FlipHorizontallyCom; } set { if (value != _FlipHorizontallyCom) { _FlipHorizontallyCom = value; } } }

        private ICommand _FlipVerticallyCom;
        public ICommand FlipVerticallyCom { get { return _FlipVerticallyCom; } set { if (value != _FlipVerticallyCom) { _FlipVerticallyCom = value; } } }

        private ICommand _MouseInPicture;
        public ICommand MouseInPicture { get { return _MouseInPicture; } set { if (value != _MouseInPicture) { _MouseInPicture = value; } } }

        private ICommand _MouseOutPicture;
        public ICommand MouseOutPicture { get { return _MouseOutPicture; } set { if (value != _MouseOutPicture) { _MouseOutPicture = value; } } }

        private ICommand _SelectAllCom;
        public ICommand SelectAllCom { get { return _SelectAllCom; } set { if (value != _SelectAllCom) { _SelectAllCom = value; } } }

        #endregion
    }
}

