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
using System.Xml.Linq;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class EditProjectVM : TabBaseVM
    {
        #region CTOR
        public EditProjectVM(DocumentNode dominoProvider) : base()
        {
            ProjectName = Path.GetFileNameWithoutExtension(dominoProvider.relativePath);

            HaveBuildtools = dominoProvider.obj.HasProtocolDefinition ? Visibility.Visible : Visibility.Hidden;

            IsExpandible = dominoProvider is FieldNode ? Visibility.Visible : Visibility.Hidden;
            string relativePath = dominoProvider.relativePath;
            string filepath = Workspace.AbsolutePathFromReference(ref relativePath, dominoProvider.parent);
            ImageSource = ImageHelper.GetImageOfFile(filepath);

            UICursor = null;
            selectedDominoes = new List<DominoInCanvas>();
            UnsavedChanges = false;
            CurrentProject = dominoProvider.obj;

            _DominoList = new ObservableCollection<ColorListEntry>();

            _DominoList.Clear();
            CurrentProject.colors.Anzeigeindizes.CollectionChanged += Anzeigeindizes_CollectionChanged;
            refreshList();
            selectedColors = new int[CurrentProject.colors.Length];
            SaveField = new RelayCommand(o => { Save(); });
            RestoreBasicSettings = new RelayCommand(o => { redoStack = new Stack<PostFilter>(); Editing = false; });
            BuildtoolsClick = new RelayCommand(o => { OpenBuildTools(); });
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
            ShowImageClick = new RelayCommand(o => { ShowImage(); });
            MouseInPicture = new RelayCommand(o => { UICursor = Cursors.Hand; });
            MouseOutPicture = new RelayCommand(o => { UICursor = null; });
            SelectAllCom = new RelayCommand(o => { SelectAll(); });
            RefreshCanvas();
            UnsavedChanges = false;
        }
        #endregion

        #region fields
        private double visibleWidth = 0;
        private double visibleHeight = 0;
        private double largestX = 0;
        private double largestY = 0;
        private List<DominoInCanvas> selectedDominoes;
        private List<DominoInCanvas> possibleToPaste = new List<DominoInCanvas>();
        private int[] selectedColors;
        private DominoInCanvas[] copyedDominoes;
        private int startindex;
        private System.Windows.Point SelectionStartPoint;
        private System.Windows.Shapes.Rectangle rect;
        private DominoTransfer dominoTransfer;
        public string assemblyname;
        public Stack<PostFilter> undoStack = new Stack<PostFilter>();
        public Stack<PostFilter> redoStack = new Stack<PostFilter>();
        #endregion

        #region events
        internal event EventHandler RefreshSize;
        #endregion

        #region prope
        public bool Editing
        {
            get { return CurrentProject.Editing; }
            set
            {
                EditingDeactivatedOperation op = new EditingDeactivatedOperation(this);
                op.Apply();
                undoStack.Push(op);
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
                    RaisePropertyChanged();
                }
            }
        }
        private Visibility _IsExpandible;
        public Visibility IsExpandible
        {
            get { return _IsExpandible; }
            set
            {
                if (_IsExpandible != value)
                {
                    _IsExpandible = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _Expanded;
        public bool Expanded
        {
            get => _Expanded;
            set
            {
                if (_Expanded != value)
                {
                    _Expanded = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                    RefreshCanvas();
                    UpdateUIElements();
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

        private string _ImageSource;
        public string ImageSource
        {
            get { return _ImageSource; }
            set
            {
                if (_ImageSource != value)
                {
                    _ImageSource = value;
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

        private ProjectCanvas _DominoProject;
        public ProjectCanvas DominoProject
        {
            get { return _DominoProject; }
            set
            {
                if (_DominoProject != value)
                {
                    if (_DominoProject != null)
                    {
                        _DominoProject.SizeChanged -= _DominoProject_SizeChanged;
                    }
                    _DominoProject = value;
                    RaisePropertyChanged();
                    _DominoProject.SizeChanged += _DominoProject_SizeChanged;

                    _DominoProject.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _DominoProject.VerticalAlignment = VerticalAlignment.Stretch;
                }
            }
        }

        private int _ZoomValue = 1;
        public int ZoomValue
        {
            get { return _ZoomValue; }
            set
            {
                if (_ZoomValue != value)
                {
                    double scale = _DominoProject.LayoutTransform.Value.M11 / _ZoomValue * value;
                    _ZoomValue = value;
                    _DominoProject.LayoutTransform = new ScaleTransform(scale, scale);
                    RaisePropertyChanged();
                }
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
        private int _physicalLength;
        public int PhysicalLength
        {
            get
            {
                return _physicalLength;
            }
            set
            {
                if (_physicalLength != value)
                {
                    _physicalLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _physicalHeight;
        public int PhysicalHeight
        {
            get { return _physicalHeight; }
            set
            {
                if (_physicalHeight != value)
                {
                    _physicalHeight = value;
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
            ClearCanvas();
        }

        internal void ClearCanvas()
        {
            clearPossibleToPaste();
            ClearFullSelection();
            RemoveStones();
        }

        private void RemoveStones()
        {
            while (DominoProject.Stones.Count > 0)
            {
                if (DominoProject.Stones[0] is DominoInCanvas dic)
                    dic.DisposeStone();
                DominoProject.Stones.RemoveAt(0);
            }
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
            DominoProject.InvalidateVisual();
        }
        private void RefreshColorAmount()
        {
            for (int i = 0; i < _DominoList.Count(); i++)
            {
                if (_DominoList[i].ProjectCount.Count != 2)
                {
                    _DominoList[i].ProjectCount.Clear();
                    // add dummy entries
                    _DominoList[i].ProjectCount.Add(0);
                    _DominoList[i].ProjectCount.Add(0);
                }

                if (CurrentProject.Counts.Length > i + 1)
                {
                    _DominoList[i].ProjectCount[0] = CurrentProject.Counts[i + 1];
                    _DominoList[i].ProjectCount[1] = selectedColors[i + 1];
                }
                else
                {
                    _DominoList[i].ProjectCount[0] = CurrentProject.Counts[0];
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
            foreach (DominoInCanvas dic in DominoProject.Stones)
            {
                if (dic.isSelected == false)
                {
                    dic.isSelected = true;
                    selectedDominoes.Add(dic);
                    selectedColors[dic.domino.color]++;
                }
            }
        }

        private void ShowImage()
        {
            try
            {
                Process.Start(ImageSource);
            }
            catch (Exception)
            {

            }
        }

        List<int> toCopy = new List<int>();
        private void Copy()
        {
            if (!(CurrentProject is ICopyPasteable)) Errorhandler.RaiseMessage("Could not copy in this project.", "Copy", Errorhandler.MessageType.Warning);
            toCopy.Clear();
            clearPossibleToPaste();
            if (selectedDominoes.Count < 0)
            {
                Errorhandler.RaiseMessage("Nothing to copy!", "No selection", Errorhandler.MessageType.Error);
                return;
            }
            copyedDominoes = new DominoInCanvas[selectedDominoes.Count];
            selectedDominoes.CopyTo(copyedDominoes);

            startindex = DominoProject.Stones.Count - 1;
            foreach (DominoInCanvas dic in selectedDominoes)
            {
                if (startindex > dic.idx)
                    startindex = dic.idx;
                dic.isSelected = false;
                toCopy.Add(dic.idx);
            }

            selectedDominoes = new List<DominoInCanvas>();
            try
            {
                int[] validPositions = ((ICopyPasteable)this.CurrentProject).GetValidPastePositions(startindex);

                foreach (DominoInCanvas dic in DominoProject.Stones.OfType<DominoInCanvas>())
                {
                    if (validPositions.Contains(dic.idx))
                    {
                        possibleToPaste.Add(dic);
                        dic.PossibleToPaste = true;
                    }
                }
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
                int pasteindex = selectedDominoes.First().idx;
                selectedDominoes.First().isSelected = false;
                selectedDominoes.Clear();
                PasteFilter paste = new PasteFilter(CurrentProject as ICopyPasteable, startindex, toCopy.ToArray(), pasteindex);
                paste.Apply();
                undoStack.Push(paste);
                clearPossibleToPaste();
                UpdateUIElements();
            }
            catch (InvalidOperationException ex)
            {
                Errorhandler.RaiseMessage(ex.Message, "Error", Errorhandler.MessageType.Error);
            }
        }

        private void clearPossibleToPaste()
        {
            foreach (DominoInCanvas dic in possibleToPaste)
            {
                dic.PossibleToPaste = false;
            }
            possibleToPaste.Clear();

            UpdateUIElements();
        }

        public override void Undo()
        {
            if (undoStack.Count == 0) return;
            PostFilter undoFilter = undoStack.Pop();
            redoStack.Push(undoFilter);
            undoFilter.Undo();

            ClearCanvas();
            RefreshCanvas();
        }
        public override void Redo()
        {
            if (redoStack.Count == 0) return;
            PostFilter redoFilter = redoStack.Pop();
            undoStack.Push(redoFilter);
            redoFilter.Apply();

            //if (!(redoFilter is SetColorOperation || redoFilter is PasteFilter))
            {
                ClearCanvas();
                RefreshCanvas();
            }
        }

        internal void SizeChanged(double width, double height)
        {
            visibleWidth = width;
            visibleHeight = height;
            RefreshTransformation();
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
        private void _DominoProject_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshTransformation();
        }

        internal void RefreshTransformation()
        {
            double ScaleX, ScaleY;

            ScaleX = visibleWidth / largestX * ZoomValue;
            ScaleY = visibleHeight / largestY * ZoomValue;

            if (ScaleX < ScaleY)
                _DominoProject.LayoutTransform = new ScaleTransform(ScaleX, ScaleX);
            else
                _DominoProject.LayoutTransform = new ScaleTransform(ScaleY, ScaleY);

            _DominoProject.UpdateLayout();
        }

        private void ChangeColor()
        {
            List<int> selectedIndices = new List<int>();
            foreach (DominoInCanvas dic in selectedDominoes)
            {
                selectedIndices.Add(dic.idx);
                dic.isSelected = false;
            }
            SetColorOperation sco = new SetColorOperation(CurrentProject, selectedIndices.ToArray(), CurrentProject.colors.RepresentionForCalculation.ToList().IndexOf(SelectedColor.DominoColor));
            undoStack.Push(sco);
            sco.Apply();

            selectedDominoes.Clear();
            selectedColors = new int[CurrentProject.colors.Length];
            UnsavedChanges = true;
            UpdateUIElements();
        }

        private void AddRow(bool addBelow)
        {
            try
            {
                if (selectedDominoes.Count > 0)
                {
                    DominoInCanvas selDomino = selectedDominoes.First();
                    if (CurrentProject is IRowColumnAddableDeletable)
                    {
                        AddRows addRows = new AddRows((CurrentProject as IRowColumnAddableDeletable), selDomino.idx, 1, selDomino.domino.color, addBelow);
                        addRows.Apply();
                        undoStack.Push(addRows);
                        ClearCanvas();
                        RefreshCanvas();
                        for (int i = 0; i < addRows.added_indizes.Count(); i++)
                        {
                            AddToSelectedDominoes(DominoProject.Stones[addRows.added_indizes[i]]);
                        }
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
                    DominoInCanvas selDomino = selectedDominoes.First();
                    if (CurrentProject is IRowColumnAddableDeletable)
                    {
                        AddColumns addRows = new AddColumns((CurrentProject as IRowColumnAddableDeletable), selDomino.idx, 1, selDomino.domino.color, addRight);
                        addRows.Apply();
                        undoStack.Push(addRows);
                        ClearCanvas();
                        RefreshCanvas();
                        for (int i = 0; i < addRows.added_indizes.Count(); i++)
                        {
                            AddToSelectedDominoes(DominoProject.Stones[addRows.added_indizes[i]]);
                        }
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
                        List<int> toRemove = new List<int>();
                        foreach (DominoInCanvas selDomino in selectedDominoes)
                        {
                            toRemove.Add(selDomino.idx);
                        }
                        DeleteRows deleteRows = new DeleteRows((CurrentProject as IRowColumnAddableDeletable), toRemove.ToArray());
                        deleteRows.Apply();
                        undoStack.Push(deleteRows);
                        ClearCanvas();
                        RefreshCanvas();
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
                        List<int> toRemove = new List<int>();
                        foreach (DominoInCanvas selDomino in selectedDominoes)
                        {
                            toRemove.Add(selDomino.idx);
                        }
                        DeleteColumns deleteColumns = new DeleteColumns((CurrentProject as IRowColumnAddableDeletable), toRemove.ToArray());
                        deleteColumns.Apply();
                        undoStack.Push(deleteColumns);
                        ClearCanvas();
                        RefreshCanvas();
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

        private void ClearFullSelection()
        {
            foreach (DominoInCanvas dic in selectedDominoes)
                dic.isSelected = false;
            selectedDominoes.Clear();
            selectedColors = new int[CurrentProject.colors.Length];
            RefreshCanvas();
        }

        internal void RefreshCanvas()
        {
            if (DominoProject != null)
            {
                RemoveStones();
                DominoProject.MouseDown -= Canvas_MouseDown;
                DominoProject.MouseMove -= Canvas_MouseMove;
                DominoProject.MouseUp -= Canvas_MouseUp;
            }
            largestX = 0;
            largestY = 0;
            DominoProject = new ProjectCanvas();
            DominoProject.MouseDown += Canvas_MouseDown;
            DominoProject.MouseMove += Canvas_MouseMove;
            DominoProject.MouseUp += Canvas_MouseUp;
            DominoProject.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
            dominoTransfer = CurrentProject.Generate(new System.Threading.CancellationToken(), progress);

            for (int i = 0; i < dominoTransfer.shapes.Count(); i++)
            {
                DominoInCanvas dic = new DominoInCanvas(i, dominoTransfer[i], CurrentProject.colors, !Expanded);
                dic.MouseDown += Dic_MouseDown;
                System.Windows.Shapes.Path sd = new System.Windows.Shapes.Path();

                DominoProject.Stones.Add(dic);
            }
            largestX = dominoTransfer.shapes.Max(x => x.GetContainer(expanded: Expanded).x2);
            largestY = dominoTransfer.shapes.Max(x => x.GetContainer(expanded: Expanded).y2);
            DominoProject.Width = largestX;
            DominoProject.Height = largestY;

            UpdateUIElements();

            RefreshSizeLabels();
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
                if (!selectedDominoes.Contains(dic))
                {
                    AddToSelectedDominoes(dic);
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                RemoveFromSelectedDominoes(dic);
            }
        }

        public override bool Save()
        {
            try
            {
                CurrentProject.Save();
                UnsavedChanges = false;
                return true;
            }
            catch (Exception) { return false; }
        }

        private void OpenBuildTools()
        {
            ProtocolV protocolV = new ProtocolV();
            protocolV.DataContext = new ProtocolVM(CurrentProject, ProjectName, assemblyname);
            protocolV.ShowDialog();
        }

        private void SelectAllStonesWithColor()
        {
            if (SelectedColor == null) return;
            if (selectedDominoes.Count > 0)
            {
                for (int i = 0; i < selectedDominoes.Count; i++)
                {
                    if (selectedDominoes[i].StoneColor != SelectedColor.DominoColor.mediaColor)
                    {
                        RemoveFromSelectedDominoes(selectedDominoes[i]);
                        i--;
                    }
                }
            }
            else
            {
                for (int i = 0; i < DominoProject.Stones.Count; i++)
                {
                    if (!(DominoProject.Stones[i] is DominoInCanvas)) continue;
                    if (((DominoInCanvas)DominoProject.Stones[i]).StoneColor == SelectedColor.DominoColor.mediaColor && ((DominoInCanvas)DominoProject.Stones[i]).isSelected == false)
                    {
                        AddToSelectedDominoes(DominoProject.Stones[i]);
                    }
                }
            }
            UpdateUIElements();
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed) return;

            SelectionStartPoint = e.GetPosition(DominoProject);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                rect = new System.Windows.Shapes.Rectangle
                {
                    Stroke = Brushes.LightBlue,
                    StrokeThickness = 8
                };
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                rect = new System.Windows.Shapes.Rectangle
                {
                    Stroke = Brushes.IndianRed,
                    StrokeThickness = 8
                };
            }
            Canvas.SetLeft(rect, SelectionStartPoint.X);
            Canvas.SetTop(rect, SelectionStartPoint.Y);
            rect.Visibility = System.Windows.Visibility.Hidden;
            DominoProject.Children.Add(rect);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
            {
                if (rect != null)
                {
                    DominoProject.Children.Remove(rect);
                    rect = null;
                }
                return;
            }

            if (rect == null) return;

            var pos = e.GetPosition((Canvas)sender);

            var x = Math.Min(pos.X, SelectionStartPoint.X);
            var y = Math.Min(pos.Y, SelectionStartPoint.Y);

            var w = Math.Max(pos.X, SelectionStartPoint.X) - x;
            var h = Math.Max(pos.Y, SelectionStartPoint.Y) - y;

            rect.Width = w;
            rect.Height = h;

            if (w > 10 || h > 10)
                rect.Visibility = Visibility.Visible;
            else
                rect.Visibility = Visibility.Hidden;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (rect == null || rect.Visibility != Visibility.Visible)
            {
                for (int i = 0; i < DominoProject.Stones.Count; i++)
                {
                    if (DominoProject.Stones[i] is DominoInCanvas dic)
                    {
                        double _top = double.MaxValue;
                        double _bottom = 0;
                        double _left = double.MaxValue;
                        double _right = 0;

                        foreach (System.Windows.Point point in dic.canvasPoints)
                        {
                            if (point.Y < _top) _top = point.Y;
                            if (point.Y > _bottom) _bottom = point.Y;
                            if (point.X < _left) _left = point.X;
                            if (point.X > _right) _right = point.X;
                        }
                        if (_left < e.GetPosition(DominoProject).X && _right > e.GetPosition(DominoProject).X
                            && _top < e.GetPosition(DominoProject).Y && _bottom > e.GetPosition(DominoProject).Y)
                        {
                            if (e.ChangedButton == MouseButton.Left)
                            {
                                if (!((DominoInCanvas)DominoProject.Stones[i]).isSelected)
                                {
                                    AddToSelectedDominoes(DominoProject.Stones[i]);
                                }
                            }
                            else if (e.ChangedButton == MouseButton.Right)
                            {
                                if (((DominoInCanvas)DominoProject.Stones[i]).isSelected)
                                {
                                    RemoveFromSelectedDominoes(DominoProject.Stones[i]);
                                }
                            }
                        }
                    }
                }

                UpdateUIElements();
                return;
            }
            double top = Canvas.GetTop(rect);
            double right = Canvas.GetLeft(rect) + rect.ActualWidth;
            double bottom = Canvas.GetTop(rect) + rect.ActualHeight;
            double left = Canvas.GetLeft(rect);

            for (int i = 0; i < DominoProject.Stones.Count; i++)
            {
                if (DominoProject.Stones[i] is DominoInCanvas dic)
                {
                    double _top = double.MaxValue;
                    double _bottom = 0;
                    double _left = double.MaxValue;
                    double _right = 0;

                    foreach (System.Windows.Point point in dic.canvasPoints)
                    {
                        if (point.Y < _top) _top = point.Y;
                        if (point.Y > _bottom) _bottom = point.Y;
                        if (point.X < _left) _left = point.X;
                        if (point.X > _right) _right = point.X;
                    }

                    /*if ((dic.RenderedGeometry.Bounds.Left > left && dic.RenderedGeometry.Bounds.Left < right
                          || dic.RenderedGeometry.Bounds.Right > left && dic.RenderedGeometry.Bounds.Right < right
                          || dic.RenderedGeometry.Bounds.Left < left && dic.RenderedGeometry.Bounds.Right > left 
                          && dic.RenderedGeometry.Bounds.Left < right && dic.RenderedGeometry.Bounds.Right > right)
                          && (dic.RenderedGeometry.Bounds.Top > top && dic.RenderedGeometry.Bounds.Top < bottom
                          || dic.RenderedGeometry.Bounds.Bottom > top && dic.RenderedGeometry.Bounds.Bottom < bottom
                          || (dic.RenderedGeometry.Bounds.Top < top && dic.RenderedGeometry.Bounds.Bottom > top
                          && dic.RenderedGeometry.Bounds.Top < bottom && dic.RenderedGeometry.Bounds.Bottom > bottom)))*/
                    if ((_left > left && _left < right
                      || _right > left && _right < right
                      || _left < left && _right > left
                      && _left < right && _right > right)
                      && (_top > top && _top < bottom
                      || _bottom > top && _bottom < bottom
                      || (_top < top && _bottom > top
                      && _top < bottom && _bottom > bottom)))
                    {
                        if (e.ChangedButton == MouseButton.Left)
                        {
                            if (!((DominoInCanvas)DominoProject.Stones[i]).isSelected)
                            {
                                AddToSelectedDominoes(DominoProject.Stones[i]);
                            }
                        }
                        else if (e.ChangedButton == MouseButton.Right)
                        {
                            if (((DominoInCanvas)DominoProject.Stones[i]).isSelected)
                            {
                                RemoveFromSelectedDominoes(DominoProject.Stones[i]);
                            }
                        }
                    }
                }
            }

            rect.Visibility = Visibility.Hidden;
            DominoProject.Children.Remove(rect);

            UpdateUIElements();
        }
        private void AddToSelectedDominoes(DominoInCanvas dic)
        {
            dic.isSelected = true;
            selectedDominoes.Add(dic);
            selectedColors[dic.domino.color]++;
        }
        private void RemoveFromSelectedDominoes(DominoInCanvas dic)
        {
            dic.isSelected = false;
            selectedDominoes.Remove(dic);
            selectedColors[dic.domino.color]--;
        }
        #endregion

        #region Command
        private ICommand _ShowImageClick;
        public ICommand ShowImageClick { get { return _ShowImageClick; } set { if (value != _ShowImageClick) { _ShowImageClick = value; } } }

        private ICommand _ClearSelection;
        public ICommand ClearSelection { get { return _ClearSelection; } set { if (value != _ClearSelection) { _ClearSelection = value; } } }

        private ICommand _SelectColor;
        public ICommand SelectColor { get { return _SelectColor; } set { if (value != _SelectColor) { _SelectColor = value; } } }

        private ICommand _SaveField;
        public ICommand SaveField { get { return _SaveField; } set { if (value != _SaveField) { _SaveField = value; } } }

        private ICommand _RestoreBasicSettings;
        public ICommand RestoreBasicSettings { get { return _RestoreBasicSettings; } set { if (value != _RestoreBasicSettings) { _RestoreBasicSettings = value; } } }

        private ICommand _BuildtoolsClick;
        public ICommand BuildtoolsClick { get { return _BuildtoolsClick; } set { if (value != _BuildtoolsClick) { _BuildtoolsClick = value; } } }

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

