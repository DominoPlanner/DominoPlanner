using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Collections;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class EditProjectVM : DominoProviderTabItem
    {
        #region CTOR
        public EditProjectVM(IDominoProvider dominoProvider) : base()
        {
            HaveBuildtools = dominoProvider.HasProtocolDefinition;

            UICursor = null;
            Dominoes = new AvaloniaList<EditingDominoVM>();
            PossiblePastePositions = new List<int>();
            selectedDominoes = new AvaloniaList<int>();
            UnsavedChanges = false;
            CurrentProject = dominoProvider;
            dominoTransfer = dominoProvider.Last;

            _DominoList = new ObservableCollection<ColorListEntry>();

            _DominoList.Clear();
            CurrentProject.colors.Anzeigeindizes.CollectionChanged += Anzeigeindizes_CollectionChanged;
            RefreshList();
            selectedColors = new int[CurrentProject.colors.Length];
            SaveField = new RelayCommand(o => { Save(); });
            RestoreBasicSettings = new RelayCommand(o => { redoStack = new Stack<PostFilter>(); Editing = false; });
            
            SelectColor = new RelayCommand(o => { SelectAllStonesWithColor(); });
            MouseClickCommand = new RelayCommand(o => { ChangeColor(); });
            ClearSelection = new RelayCommand(o => { ClearFullSelection(true); });
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
            MouseInPicture = new RelayCommand(o => { UICursor = new Cursor(StandardCursorType.Hand); });
            MouseOutPicture = new RelayCommand(o => { UICursor = null; });
            SelectAllCom = new RelayCommand(o => { SelectAll(); });
            UnsavedChanges = false;
            SelectionTool = new SelectionToolVM(this);
            DisplaySettingsTool = new DisplaySettingsToolVM(this);
            ZoomTool = new ZoomToolVM(this);
            RulerTool = new RulerToolVM(this);
            EditingTools = new ObservableCollection<EditingToolVM>() {
                SelectionTool,
                RulerTool,
                new EditingToolVM() {Image = "add_delete_rowDrawingImage", Name="Add or delete rows and columns" },
                new EditingToolVM() { Image = "textDrawingImage", Name="Write text"},
                ZoomTool,
                DisplaySettingsTool
            };
            SelectedTool = SelectionTool;
            UpdateUIElements();
        }
        #endregion

        #region fields


        internal DominoTransfer dominoTransfer;

        internal AvaloniaList<int> selectedDominoes;
        public AvaloniaList<int> SelectedDominoes
        {
            get { return selectedDominoes; }
            set { selectedDominoes = value; RaisePropertyChanged(); }
        }
        private int[] selectedColors;
        private int startindex;

        private AvaloniaList<EditingDominoVM> dominoes;

        public AvaloniaList<EditingDominoVM> Dominoes
        {
            get { return dominoes; }
            set { dominoes = value; RaisePropertyChanged(); }
        }
        private AvaloniaList<CanvasDrawable> additionalDrawables;
        public AvaloniaList<CanvasDrawable> AdditionalDrawables
        {
            get { return additionalDrawables; }
            set { additionalDrawables = value; RaisePropertyChanged(); }
        }
        


        private SelectionToolVM SelectionTool { get; set; }
        public DisplaySettingsToolVM DisplaySettingsTool { get; set; }
        public ZoomToolVM ZoomTool;
        public RulerToolVM RulerTool;
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
                if (value != null && value != selectedTool)
                {
                    selectedTool?.LeaveTool();
                    selectedTool = value;
                    selectedTool.EnterTool();
                }
                TabPropertyChanged(ProducesUnsavedChanges: false);

            }
        }
        private bool _HaveBuildtools;
        public bool HaveBuildtools
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

        private AvaloniaList<ColorControl.Column> _colorColumnConfig;

        public AvaloniaList<ColorControl.Column> ColorColumnConfig
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

        internal void ClearCanvas(bool ClearSelection = true)
        {
            ClearPastePositions();
            if (ClearSelection) ClearFullSelection(true);
            /*if (DisplaySettingsTool.DominoProject != null)
                DisplaySettingsTool.RemoveStones();*/
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
            Redraw();
            RefreshSizeLabels();
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
        private void RefreshList()
        {
            // Setup Columns
            ColorColumnConfig = new AvaloniaList<ColorControl.Column>
            {
                new ColorControl.Column() { DataField = "DominoColor.mediaColor", Header = "", Class = "Color" },
                new ColorControl.Column() { DataField = "DominoColor.name", Header = "Name" },
                new ColorControl.Column() { DataField = "DominoColor.count", Header = "Total" },
                new ColorControl.Column() { DataField = "ProjectCount[0]", Header = "Used", HighlightDataField = "DominoColor.count" },
                new ColorControl.Column() { DataField = "ProjectCount[1]", Header = "Selected" }
            };

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
            SelectionTool.Select(Enumerable.Range(0, dominoTransfer.Length).ToList(), true);
            UpdateUIElements();
        }
        List<int> toCopy = new List<int>();
        private async void Copy()
        {
            if (!(CurrentProject is ICopyPasteable)) await Errorhandler.RaiseMessage("Could not copy in this project.", "Copy", Errorhandler.MessageType.Warning);
            ClearPastePositions();
            if (selectedDominoes.Count < 0)
            {
                await Errorhandler.RaiseMessage("Nothing to copy!", "No selection", Errorhandler.MessageType.Error);
                return;
            }
            toCopy = new List<int>(selectedDominoes);
            startindex = selectedDominoes.Min();
            ClearFullSelection(true);
            try
            {
                int[] validPositions = ((ICopyPasteable)this.CurrentProject).GetValidPastePositions(startindex);
                HighlightPastePositions(validPositions);
            }
            catch (InvalidOperationException ex)
            {
                await Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
            UpdateUIElements();
        }

        private async void Paste()
        {
            try
            {
                if (!(CurrentProject is ICopyPasteable)) await Errorhandler.RaiseMessage("Could not paste in this project.", "Paste", Errorhandler.MessageType.Warning);
                if (selectedDominoes.Count == 0) return;
                int pasteindex = selectedDominoes.First();
                RemoveFromSelectedDominoes(pasteindex);
                ClearFullSelection(true);
                PasteFilter paste = new PasteFilter(CurrentProject as ICopyPasteable, startindex, toCopy.ToArray(), pasteindex);
                paste.Apply();
                undoStack.Push(paste);
                ClearPastePositions();
                UpdateUIElements();
            }
            catch (InvalidOperationException ex)
            {
                await Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
        }
        public void ExecuteOperation(PostFilter pf)
        {
            pf.Apply();
            undoStack.Push(pf);
            redoStack.Clear();
        }

        public override void Undo()
        {
            UndoInternal(false);
        }
        public void UndoInternal(bool IncludeSelectionOperation = false)
        {
            if (undoStack.Count == 0) return;
            PostFilter undoFilter;
            do
            {
                undoFilter = undoStack.Pop();
                redoStack.Push(undoFilter);
                undoFilter.Undo();
            } while ((!IncludeSelectionOperation && (undoFilter is SelectionOperation)) && (undoStack.Count != 0));

            if (!(undoFilter is EditingActivatedOperation || undoFilter is SelectionOperation || undoFilter is SetColorOperation) )
            {
                ClearCanvas(false);
                RecreateCanvasViewModel();
                UpdateUIElements();
                if (undoStack.Count == 0) UnsavedChanges = false;
            }
            else
            {
                UpdateUIElements();
            }
            
        }
            
        public override void Redo()
        {
            RedoInternal(false);
        }
        public void RedoInternal(bool IncludeSelectionOperation = false)
        {
            if (redoStack.Count == 0) return;
            PostFilter redoFilter;
            do
            {
                redoFilter = redoStack.Pop();
                undoStack.Push(redoFilter);
                redoFilter.Apply();
            } while ((!IncludeSelectionOperation && redoFilter is SelectionOperation) && redoStack.Count != 0);
          
            if (!(redoFilter is EditingDeactivatedOperation || redoFilter is SelectionOperation || redoFilter is SetColorOperation))
            {
                ClearCanvas(false);
                RecreateCanvasViewModel();
                UpdateUIElements();
            }
            else
            {
                UpdateUIElements();
            }
        }
        internal override void KeyPressed(object sender, KeyEventArgs args)
        {
            if (!args.Handled)
                SelectedTool.KeyPressed(args);
        }
        internal override void ResetContent()
        {
            base.ResetContent();
            RefreshSize?.Invoke(this, EventArgs.Empty);
        }
        private void ChangeColor()
        {
            
            SetColorOperation sco = new SetColorOperation(CurrentProject, selectedDominoes.ToArray(), CurrentProject.colors.RepresentionForCalculation.ToList().IndexOf(SelectedColor.DominoColor));
            ClearFullSelection(true);
            ExecuteOperation(sco);
            UnsavedChanges = true;
            UpdateUIElements();
        }

        private async void AddRow(bool addBelow)
        {
            try
            {
                if (selectedDominoes.Count > 0)
                {
                    int selDomino = selectedDominoes.First();
                    if (CurrentProject is IRowColumnAddableDeletable)
                    {
                        AddRows addRows = new AddRows((CurrentProject as IRowColumnAddableDeletable), selDomino, 1, dominoTransfer[selDomino].Color, addBelow);
                        ClearCanvas();
                        ExecuteOperation(addRows);
                        
                        RecreateCanvasViewModel();
                        SelectionTool.Select(addRows.added_indizes, true);
                        UpdateUIElements();
                    }
                    else
                    {
                        await Errorhandler.RaiseMessage("Could not add a row in this project.", "Add Row", Errorhandler.MessageType.Warning);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                await Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
        }

        private async void AddColumn(bool addRight)
        {
            try
            {
                if (selectedDominoes.Count > 0)
                {
                    int selDomino = selectedDominoes.First();
                    if (CurrentProject is IRowColumnAddableDeletable)
                    {
                        AddColumns addRows = new AddColumns((CurrentProject as IRowColumnAddableDeletable), selDomino, 1, dominoTransfer[selDomino].Color, addRight);
                        ClearCanvas();
                        ExecuteOperation(addRows);
                       
                        RecreateCanvasViewModel();
                        SelectionTool.Select(addRows.added_indizes, true);
                        UpdateUIElements();
                    }
                    else
                    {
                        await Errorhandler.RaiseMessage("Could not add a row in this project.", "Add Row", Errorhandler.MessageType.Warning);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                await Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
        }

        private async void RemoveSelRows()
        {
            try
            {
                if (CurrentProject is IRowColumnAddableDeletable)
                {
                    if (selectedDominoes.Count > 0)
                    {
                        DeleteRows deleteRows = new DeleteRows((CurrentProject as IRowColumnAddableDeletable), selectedDominoes.ToArray());
                        ClearCanvas();
                        ExecuteOperation(deleteRows);
                        
                        RecreateCanvasViewModel();
                    }
                }
                else
                {
                    await Errorhandler.RaiseMessage("Could not remove a row in this project.", "Remove Row", Errorhandler.MessageType.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                await Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
        }

        private async void RemoveSelColumns()
        {
            try
            {
                if (CurrentProject is IRowColumnAddableDeletable)
                {
                    if (selectedDominoes.Count > 0)
                    {
                        DeleteColumns deleteColumns = new DeleteColumns((CurrentProject as IRowColumnAddableDeletable), selectedDominoes.ToArray());
                        ClearCanvas();
                        ExecuteOperation(deleteColumns);
                        RecreateCanvasViewModel();
                    }
                }
                else
                {
                    await Errorhandler.RaiseMessage("Could not remove a column in this project.", "Remove Column", Errorhandler.MessageType.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                await Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
        }
        internal void ClearFullSelection(bool undoable = false)
        {
            if (undoable)
            {
                SelectionTool.Select(selectedDominoes, false);

            }
            else
            {
                foreach (int i in selectedDominoes.ToArray())
                {
                    RemoveFromSelectedDominoes(i);
                }
                while (undoStack.Count > 0 && undoStack.Peek() is SelectionOperation)
                {
                    undoStack.Pop();
                }
            }
            selectedColors = new int[CurrentProject.colors.Length];
            UpdateUIElements();
            SelectionTool.CurrentSelectionDomain.ResetSelectionArea();
            RefreshColorAmount();
        }

        private void RefreshSizeLabels()
        {
            if (dominoTransfer != null)
            {
                ProjectHeight = dominoTransfer.FieldPlanHeight.ToString();
                ProjectWidth = dominoTransfer.FieldPlanLength.ToString();
                ProjectAmount = dominoTransfer.shapes.Count().ToString();
                PhysicalLength = dominoTransfer.PhysicalLength;
                PhysicalHeight = dominoTransfer.PhysicalHeight;
            }
        }
        private void SelectAllStonesWithColor()
        {
            if (SelectedColor == null) return;
            var selectedIndex = CurrentProject.colors.RepresentionForCalculation.ToList().IndexOf(SelectedColor.DominoColor);
            IEnumerable<int> oldSelection = selectedDominoes.ToArray();
            if (oldSelection.Count() == 0)
            {
                oldSelection = Enumerable.Range(0, dominoTransfer.Length);
            }
            IEnumerable<int> newSelection = oldSelection.Where(x => dominoTransfer[x].Color == selectedIndex);
            if (selectedDominoes.Count == 0)
            {
                SelectionTool.Select(newSelection.ToList(), true);
            }
            else
            {
                SelectionTool.Select(oldSelection.Except(newSelection).ToList(), false);
            }
            UpdateUIElements();
        }

        internal void Canvas_MouseDown(Avalonia.Point dominoPoint, PointerPressedEventArgs e)
        {
            SelectedTool?.MouseDown(dominoPoint, e);
        }

        internal void Canvas_MouseMove(Avalonia.Point dominoPoint, PointerEventArgs e)
        {
            SelectedTool?.MouseMove(dominoPoint, e);
        }

        internal void Canvas_MouseUp(Avalonia.Point dominoPoint, PointerReleasedEventArgs e)
        {
            SelectedTool?.MouseUp(dominoPoint, e);
            UpdateUIElements();
        }
        internal void Canvas_MouseWheel(Avalonia.Point dominoPoint, PointerWheelEventArgs e)
        {
            SelectedTool?.MouseWheel(dominoPoint, e);
            
        }
        public void AddToSelectedDominoes(int i)
        {
            if (SelectDominoVisual(i))
            { 
                selectedDominoes.Add(i);
                selectedColors[dominoTransfer[i].Color]++;
            }
        }
        public void RemoveFromSelectedDominoes(int i)
        {
            if (DeSelectDominoVisual(i))
            {
                selectedDominoes.Remove(i);
                selectedColors[dominoTransfer[i].Color]--;
            }
        }
        public bool IsSelected(int i)
        {
            return Dominoes[i].IsSelected;
        }
        internal void RecreateCanvasViewModel()
        {
            if (CurrentProject.Last == null)
            {
                return;
            }
            Dominoes.Clear();
            for (int i = 0; i < CurrentProject.Last.shapes.Count(); i++)
            {
                EditingDominoVM dic = new EditingDominoVM(i, CurrentProject.Last[i], CurrentProject.colors, DisplaySettingsTool.Expanded);
                Dominoes.Add(dic);
            }
        }
        internal void Redraw()
        {
            DisplaySettingsTool.ForceRedraw = true;
        }
        private List<int> PossiblePastePositions;
        public void HighlightPastePositions(int[] validPositions)
        {
            PossiblePastePositions = new List<int>();
            foreach (int i in validPositions)
            {
                var dic = Dominoes[i];
                dic.PossibleToPaste = true;
                PossiblePastePositions.Add(i);
            }
            Redraw();
        }
        public void ClearPastePositions()
        {
            foreach (int i in PossiblePastePositions)
            {
                Dominoes[i].PossibleToPaste = false;
            }
            PossiblePastePositions.Clear();
            Redraw();
        }
        public bool SelectDominoVisual(int position)
        {
            var dic = Dominoes[position];
            if (dic.IsSelected == false)
            {
                dic.IsSelected = true;
                return true;
            }
            return false;
        }
        public bool DeSelectDominoVisual(int position)
        {
            var dic = Dominoes[position];
            if (dic.IsSelected == true)
            {
                dic.IsSelected = false;
                return true;
            }
            return false;
        }
        public EditingDominoVM FindDominoAtPosition(Avalonia.Point pos)
        {
            double min_dist = int.MaxValue;
            EditingDominoVM result = null;
            foreach (var shape in Dominoes)
            {
                if (shape.domino.IsInside(new Core.Point(pos.X, pos.Y))) return shape;
                var rect = shape.domino.GetBoundingRectangle();
                double dist = Math.Pow((rect.x + rect.width / 2) - pos.X, 2) + Math.Pow(rect.y + rect.height / 2 - pos.Y, 2);
                if (min_dist > dist)
                {
                    min_dist = dist;
                    result = shape;

                }
            }
            return result;
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

