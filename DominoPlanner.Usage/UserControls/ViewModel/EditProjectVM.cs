using DominoPlanner.Core;
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
    class EditProjectVM : TabBaseVM
    {
        #region CTOR
        public EditProjectVM() : base()
        {
            UICursor = null;
            selectedDominoes = new List<DominoInCanvas>();
            UnsavedChanges = false;
            ImageSource = @"TestImages\mountain.jpg";
            string ColorSource = @"TestImages\colors.DColor";
            Workspace.Instance.root_path = Path.GetFullPath("..\\..\\..\\");
            ProjectProperties = new FieldParameters(ImageSource, ColorSource, 8, 8, 24, 8, 2000, Emgu.CV.CvEnum.Inter.Lanczos4, new CieDe2000Comparison(), new Dithering(), new NoColorRestriction());

            /*StreamReader sr = new StreamReader(new FileStream(@"C:\Users\johan\Dropbox\JoJoJo\Structures.xml", FileMode.Open));
            XElement xml = XElement.Parse(sr.ReadToEnd());
            ProjectProperties = new StructureParameters(ImageSource, xml.Elements().ElementAt(6), 3000,
                 @"C:\Users\johan\Desktop\colors.DColor", ColorDetectionMode.CieDe2000Comparison, new Dithering(),
                AverageMode.Corner, new NoColorRestriction(), true);
            sr.Close();*/
            
            _DominoList = new ObservableCollection<ColorListEntry>();
            
            _DominoList.Clear();
            ProjectProperties.colors.Anzeigeindizes.CollectionChanged += Anzeigeindizes_CollectionChanged;
            refreshList();


            SaveField = new RelayCommand(o => { Save(); });
            RestoreBasicSettings = new RelayCommand(o => { MessageBox.Show("asdf"); });
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
        }
        #endregion

        #region fields
        private IDominoProvider ProjectProperties;
        private double largestX = 0;
        private double largestY = 0;
        private List<DominoInCanvas> selectedDominoes;
        private List<DominoInCanvas> possibleToPaste = new List<DominoInCanvas>();
        private DominoInCanvas[] copyedDominoes;
        private int startindex;
        private System.Windows.Point SelectionStartPoint;
        private System.Windows.Shapes.Rectangle rect;
        private DominoTransfer dominoTransfer;

        private Stack<PostFilter> undoStack = new Stack<PostFilter>();
        private Stack<PostFilter> redoStack = new Stack<PostFilter>();
        #endregion

        #region prope
        private Cursor _UICursor;
        public Cursor UICursor
        {
            get { return _UICursor; }
            set
            {
                if (_UICursor != value)
                {
                    _UICursor = value;
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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

        private Canvas _DominoProject;
        public Canvas DominoProject
        {
            get { return _DominoProject; }
            set
            {
                if (_DominoProject != value)
                {
                    if (_DominoProject != null)
                        _DominoProject.SizeChanged -= _DominoProject_SizeChanged;
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

        private int _ProjectAmount;
        public int ProjectAmount
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

        #endregion

        #region Methods
        internal override void Close()
        {
            base.Close();
            clearCanvas();
        }

        private void clearCanvas()
        {
            clearPossibleToPaste();
            ClearFullSelection();
            while (DominoProject.Children.Count > 0)
            {
                if (DominoProject.Children[0] is DominoInCanvas dic)
                    dic.DisposeStone();
                DominoProject.Children.RemoveAt(0);
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

        private void refreshList()
        {
            int counter = 0;
            foreach (DominoColor domino in ProjectProperties.colors.RepresentionForCalculation.OfType<DominoColor>())
            {
                _DominoList.Add(new ColorListEntry() { DominoColor = domino, SortIndex = ProjectProperties.colors.Anzeigeindizes[counter] });
                counter++;
            }

            if (ProjectProperties.colors.RepresentionForCalculation.OfType<EmptyDomino>().Count() == 1)
            {
                _DominoList.Add(new ColorListEntry() { DominoColor = ProjectProperties.colors.RepresentionForCalculation.OfType<EmptyDomino>().First(), SortIndex = -1 });
            }
        }
        private void SelectAll()
        {
            foreach (DominoInCanvas dic in DominoProject.Children)
            {
                if (dic.isSelected == false)
                {
                    dic.isSelected = true;
                    selectedDominoes.Add(dic);
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
            toCopy.Clear();
            clearPossibleToPaste();
            if (selectedDominoes.Count < 0)
            {
                MessageBox.Show("gibt nichts");
                return;
            }
            copyedDominoes = new DominoInCanvas[selectedDominoes.Count];
            selectedDominoes.CopyTo(copyedDominoes);

            startindex = DominoProject.Children.Count - 1;
            foreach (DominoInCanvas dic in selectedDominoes)
            {
                if (startindex > dic.idx)
                    startindex = dic.idx;
                dic.isSelected = false;
                toCopy.Add(dic.idx);
            }

            selectedDominoes = new List<DominoInCanvas>();

            int[] validPositions = ((ICopyPasteable)this.ProjectProperties).GetValidPastePositions(startindex);
            
            foreach (DominoInCanvas dic in DominoProject.Children.OfType<DominoInCanvas>())
            {
                if (validPositions.Contains(dic.idx))
                {
                    possibleToPaste.Add(dic);
                    dic.PossibleToPaste = true;
                }
            }
        }

        private void Paste()
        {
            int pasteindex = selectedDominoes.First().idx;
            selectedDominoes.First().isSelected = false;
            selectedDominoes.Clear();
            PasteFilter paste = new PasteFilter(ProjectProperties as ICopyPasteable, startindex, toCopy.ToArray(), pasteindex);
            undoStack.Push(paste);
            paste.Apply();

            clearPossibleToPaste();
        }

        private void clearPossibleToPaste()
        {
            foreach(DominoInCanvas dic in possibleToPaste)
            {
                dic.PossibleToPaste = false;
            }
            possibleToPaste.Clear();
        }

        public override void Undo()
        {
            if (undoStack.Count == 0) return;
            PostFilter undoFilter = undoStack.Pop();
            redoStack.Push(undoFilter);
            undoFilter.Undo();

            if (!(undoFilter is SetColorOperation || undoFilter is PasteFilter))
            {
                clearCanvas();
                RefreshCanvas();
            }
        }
        public override void Redo()
        {
            if (redoStack.Count == 0) return;
            PostFilter redoFilter = redoStack.Pop();
            undoStack.Push(redoFilter);
            redoFilter.Apply();

            if (!(redoFilter is SetColorOperation || redoFilter is PasteFilter))
            {
                clearCanvas();
                RefreshCanvas();
            }
        }
        internal void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _DominoProject_SizeChanged(sender, e);
        }
        internal void PressedKey(Key key)
        {
            ClearFullSelection();
        }
        private void _DominoProject_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double ScaleX, ScaleY;
            ScaleX = e.NewSize.Width / largestX * ZoomValue;
            ScaleY = e.NewSize.Height / largestY * ZoomValue;

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
            SetColorOperation sco = new SetColorOperation(ProjectProperties, selectedIndices.ToArray(), ProjectProperties.colors.RepresentionForCalculation.ToList().IndexOf(SelectedColor.DominoColor));
            undoStack.Push(sco);
            sco.Apply();

            selectedDominoes.Clear();
            UnsavedChanges = true;
        }

        private void AddRow(bool addBelow)
        {
            if (selectedDominoes.Count == 1)
            {
                DominoInCanvas selDomino = selectedDominoes.First();
                if (ProjectProperties is IRowColumnAddableDeletable)
                {
                    AddRows addRows = new AddRows((ProjectProperties as IRowColumnAddableDeletable), selDomino.idx, 1, selDomino.domino.color, addBelow);
                    undoStack.Push(addRows);
                    addRows.Apply();
                    clearCanvas();
                    RefreshCanvas();
                }
            }
        }

        private void AddColumn(bool addRight)
        {
            if (selectedDominoes.Count == 1)
            {
                DominoInCanvas selDomino = selectedDominoes.First();
                if (ProjectProperties is IRowColumnAddableDeletable)
                {
                    AddColumns addRows = new AddColumns((ProjectProperties as IRowColumnAddableDeletable), selDomino.idx, 1, selDomino.domino.color, addRight);
                    undoStack.Push(addRows);
                    addRows.Apply();
                    clearCanvas();
                    RefreshCanvas();
                }
            }
        }

        private void RemoveSelRows()
        {
            if (selectedDominoes.Count > 0)
            {
                List<int> toRemove = new List<int>();
                foreach (DominoInCanvas selDomino in selectedDominoes)
                {
                    toRemove.Add(selDomino.idx);
                }
                DeleteRows deleteRows = new DeleteRows((ProjectProperties as IRowColumnAddableDeletable), toRemove.ToArray());
                undoStack.Push(deleteRows);
                deleteRows.Apply();
                clearCanvas();
                RefreshCanvas();
            }
        }

        private void RemoveSelColumns()
        {
            if (selectedDominoes.Count > 0)
            {
                List<int> toRemove = new List<int>();
                foreach (DominoInCanvas selDomino in selectedDominoes)
                {
                    toRemove.Add(selDomino.idx);
                }
                DeleteColumns deleteColumns = new DeleteColumns((ProjectProperties as IRowColumnAddableDeletable), toRemove.ToArray());
                undoStack.Push(deleteColumns);
                deleteColumns.Apply();
                clearCanvas();
                RefreshCanvas();
            }
        }

        private void ClearFullSelection()
        {
            foreach (DominoInCanvas dic in selectedDominoes)
                dic.isSelected = false;
            selectedDominoes.Clear();
        }
        private void RefreshCanvas()
        {
            if (DominoProject != null)
            {
                DominoProject.MouseDown -= Canvas_MouseDown;
                DominoProject.MouseMove -= Canvas_MouseMove;
                DominoProject.MouseUp -= Canvas_MouseUp;
            }
            largestX = 0;
            largestY = 0;
            DominoProject = new Canvas();
            DominoProject.MouseDown += Canvas_MouseDown;
            DominoProject.MouseMove += Canvas_MouseMove;
            DominoProject.MouseUp += Canvas_MouseUp;
            DominoProject.Background = Brushes.LightGray;
            Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
            dominoTransfer = ProjectProperties.Generate(progress);

            dominoTransfer.shapes.Count();

            for (int i = 0; i < dominoTransfer.shapes.Count(); i++)
            {
                DominoInCanvas dic = new DominoInCanvas(i, dominoTransfer[i], ProjectProperties.colors);
                dic.MouseDown += Dic_MouseDown;
                DominoProject.Children.Add(dic);
                for (int k = 0; k < 4; k++)
                {
                    if (largestX == 0 || largestX < dominoTransfer[i].GetPath().points[k].X)
                        largestX = dominoTransfer[i].GetPath().points[k].X;

                    if (largestY == 0 || largestY < dominoTransfer[i].GetPath().points[k].Y)
                        largestY = dominoTransfer[i].GetPath().points[k].Y;
                }
            }
            DominoProject.Width = largestX;
            DominoProject.Height = largestY;
        }

        private void Dic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DominoInCanvas dic = (DominoInCanvas)sender;

            if (dic.isSelected)
                selectedDominoes.Remove(dic);
            else
                selectedDominoes.Add(dic);
            dic.isSelected = !dic.isSelected;
        }

        public override bool Save()
        {
            throw new NotImplementedException();
        }

        private void OpenBuildTools()
        {
            ProtocolV protocolV = new ProtocolV();
            protocolV.DataContext = new ProtocolVM(ProjectProperties);
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
                        selectedDominoes[i].isSelected = false;
                        selectedDominoes.Remove(selectedDominoes[i]);
                        i--;
                    }
                }
            }
            else
            {
                for (int i = 0; i < DominoProject.Children.Count; i++)
                {
                    if (((DominoInCanvas)DominoProject.Children[i]).StoneColor == SelectedColor.DominoColor.mediaColor && ((DominoInCanvas)DominoProject.Children[i]).isSelected == false)
                    {
                        ((DominoInCanvas)DominoProject.Children[i]).isSelected = true;
                        selectedDominoes.Add((DominoInCanvas)DominoProject.Children[i]);
                    }
                }
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed) return;

            SelectionStartPoint = e.GetPosition(DominoProject);

            rect = new System.Windows.Shapes.Rectangle
            {
                Stroke = System.Windows.Media.Brushes.LightBlue,
                StrokeThickness = 8
            };
            Canvas.SetLeft(rect, SelectionStartPoint.X);
            Canvas.SetTop(rect, SelectionStartPoint.Y);
            rect.Visibility = System.Windows.Visibility.Hidden;
            DominoProject.Children.Add(rect);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || rect == null)
                return;

            var pos = e.GetPosition((Canvas)sender);

            var x = Math.Min(pos.X, SelectionStartPoint.X);
            var y = Math.Min(pos.Y, SelectionStartPoint.Y);

            var w = Math.Max(pos.X, SelectionStartPoint.X) - x;
            var h = Math.Max(pos.Y, SelectionStartPoint.Y) - y;

            rect.Width = w;
            rect.Height = h;

            if (w > 10 || h > 10)
                rect.Visibility = System.Windows.Visibility.Visible;
            else
                rect.Visibility = Visibility.Hidden;
            
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (rect == null || rect.Visibility != Visibility.Visible) return;
            double top = Canvas.GetTop(rect);
            double right = Canvas.GetLeft(rect) + rect.ActualWidth;
            double bottom = Canvas.GetTop(rect) + rect.ActualHeight;
            double left = Canvas.GetLeft(rect);

            for (int i = 0; i < DominoProject.Children.Count - 1; i++)
            {
                if (DominoProject.Children[i] is DominoInCanvas dic)
                {
                    if ((dic.RenderedGeometry.Bounds.Left > left && dic.RenderedGeometry.Bounds.Left < right
                        || dic.RenderedGeometry.Bounds.Right > left && dic.RenderedGeometry.Bounds.Right < right)
                        && (dic.RenderedGeometry.Bounds.Top > top && dic.RenderedGeometry.Bounds.Top < bottom
                        || dic.RenderedGeometry.Bounds.Bottom > top && dic.RenderedGeometry.Bounds.Bottom < bottom
                        || (dic.RenderedGeometry.Bounds.Top < top && dic.RenderedGeometry.Bounds.Bottom > top
                        && dic.RenderedGeometry.Bounds.Top < bottom && dic.RenderedGeometry.Bounds.Bottom > bottom)))
                    {
                        if (!((DominoInCanvas)DominoProject.Children[i]).isSelected)
                        {
                            ((DominoInCanvas)DominoProject.Children[i]).isSelected = true;
                            selectedDominoes.Add(((DominoInCanvas)DominoProject.Children[i]));
                        }
                    }
                }
            }

            rect.Visibility = Visibility.Hidden;
            DominoProject.Children.Remove(rect);
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

