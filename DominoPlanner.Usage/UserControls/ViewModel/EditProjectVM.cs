using DominoPlanner.Core;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            ImageSource = @"C:\Users\johan\Pictures\Screenshots\Screenshot (5).png";
            BitmapImage b = new BitmapImage(new Uri(ImageSource, UriKind.RelativeOrAbsolute));
            WriteableBitmap wb = new WriteableBitmap(b);
            //ProjectProperties = new FieldParameters(wb, new List<DominoColor>(), 8, 8, 24, 8, 1000, BitmapScalingMode.NearestNeighbor, DitherMode.NoDithering, ColorDetectionMode.Cie94Comparison);
            BitmapImage bi = new BitmapImage(new Uri("./NewField.jpg", UriKind.RelativeOrAbsolute));
            //ProjectProperties = new FieldParameters(mat, @"C:\Users\johan\Desktop\colors.DColor", 8, 8, 24, 8, 1500, Emgu.CV.CvEnum.Inter.Lanczos4, new Core.Dithering.Dithering(), ColorDetectionMode.CieDe2000Comparison, new NoColorRestriction());
            ProjectProperties = new FieldParameters(ImageSource, @"C:\Users\johan\Desktop\colors.DColor", 8, 8, 24, 8, 6, Emgu.CV.CvEnum.Inter.Lanczos4, new Core.Dithering.Dithering(), ColorDetectionMode.CieDe2000Comparison, new NoColorRestriction());

            DominoList = new ObservableCollection<DominoColor>(ProjectProperties.colors.colors);
            
            SaveField = new RelayCommand(o => { Save(); });
            RestoreBasicSettings = new RelayCommand(o => { MessageBox.Show("asdf"); });
            BuildtoolsClick = new RelayCommand(o => { OpenBuildTools(); });
            SelectColor = new RelayCommand(o => { SelectAllStonesWithColor(); });
            MouseClickCommand = new RelayCommand(o => { ChangeColor(); });
            ClearSelection = new RelayCommand(o => { ClearFullSelection(); });
            CopyCom = new RelayCommand(o => { Copy(); });
            PasteCom = new RelayCommand(o => { Paste(); });

            AddRowAbove = new RelayCommand(o => { System.Diagnostics.Debug.WriteLine("asdf"); });
            AddRowBelow = new RelayCommand(o => { System.Diagnostics.Debug.WriteLine("asdf"); });
            AddColumnRight = new RelayCommand(o => { System.Diagnostics.Debug.WriteLine("asdf"); });
            AddColumnLeft = new RelayCommand(o => { System.Diagnostics.Debug.WriteLine("asdf"); });
            RemoveRowAbove = new RelayCommand(o => { System.Diagnostics.Debug.WriteLine("asdf"); });
            RemoveRowBelow = new RelayCommand(o => { System.Diagnostics.Debug.WriteLine("asdf"); });
            RemoveColumnRight = new RelayCommand(o => { System.Diagnostics.Debug.WriteLine("asdf"); });
            RemoveColumnLeft = new RelayCommand(o => { System.Diagnostics.Debug.WriteLine("asdf"); });
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
        private DominoInCanvas[] copyedDominoes;
        private int startindex;
        private System.Windows.Point SelectionStartPoint;
        private System.Windows.Shapes.Rectangle rect;
        private DominoTransfer dominoTransfer;
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

        private ObservableCollection<DominoColor> _DominoList;
        public ObservableCollection<DominoColor> DominoList
        {
            get { return _DominoList; }
            set
            {
                if (_DominoList != value)
                {
                    _DominoList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DominoColor _SelectedColor;
        public DominoColor SelectedColor
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
            catch(Exception)
            {
                
            }
        }

        private void Copy()
        {
            if (selectedDominoes.Count < 0)
            {
                MessageBox.Show("gibt nichts");
                return;
            }
            copyedDominoes = new DominoInCanvas[selectedDominoes.Count];
            selectedDominoes.CopyTo(copyedDominoes);
            startindex = DominoProject.Children.Count - 1;
            foreach(DominoInCanvas dic in selectedDominoes)
            {
                if (startindex > dic.idx)
                    startindex = dic.idx;
                dic.isSelected = false;
            }
            //hier gucken welcher oben links ist und davon den idx abspeichern und dann daran verschieben;) vielleicht klappt es ja ein bisschen
            /*foreach (DominoInCanvas dic in selectedDominoes)
            {
                if (dic.canvasPoints[0].X < ((DominoInCanvas)DominoProject.Children[0]).canvasPoints[0].X)
                {
                    startindex = dic.idx;
                }
                else if (dic.canvasPoints[0].X == ((DominoInCanvas)DominoProject.Children[0]).canvasPoints[0].X)
                {
                    //HierarchicalDataTemplate nochmal auf Y kontrollieren
                }
                dic.isSelected = false;
            }*/

            selectedDominoes = new List<DominoInCanvas>();
        }

        private void Paste()
        {
            if(selectedDominoes.Count != 1)
            {
                MessageBox.Show("Please select one stone!");
                return;
            }
            int difference = selectedDominoes[0].idx - startindex;

            foreach(DominoInCanvas dic in copyedDominoes)
            {
                if(dic.idx + difference < DominoProject.Children.Count)
                {
                    ((DominoInCanvas)DominoProject.Children[dic.idx + difference]).StoneColor = dic.StoneColor;
                }
            }

            selectedDominoes[0].isSelected = false;
            selectedDominoes.Clear();
        }
        public override void Undo()
        {
            throw new NotImplementedException();
        }

        public override void Redo()
        {
            throw new NotImplementedException();
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
            foreach (DominoInCanvas dic in selectedDominoes)
            {
                dic.StoneColor = SelectedColor.mediaColor;
                dic.isSelected = false;
            }
            selectedDominoes.Clear();
            UnsavedChanges = true;
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
            DominoProject = new Canvas();
            DominoProject.MouseDown += Canvas_MouseDown;
            DominoProject.MouseMove += Canvas_MouseMove;
            DominoProject.MouseUp += Canvas_MouseUp;
            DominoProject.Background = Brushes.LightGray;
            Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
            dominoTransfer = ProjectProperties.Generate(progress);
            dominoTransfer.dominoes.Count();

            for (int i = 0; i < dominoTransfer.dominoes.Count(); i++)
            {
                DominoInCanvas dic = new DominoInCanvas(i, dominoTransfer[i].Item1.GetPath(), dominoTransfer[i].Item2);
                dic.MouseDown += Dic_MouseDown;
                DominoProject.Children.Add(dic);
                for (int k = 0; k < 4; k++)
                {
                    if (largestX == 0 || largestX < dominoTransfer[i].Item1.GetPath().points[k].X)
                        largestX = dominoTransfer[i].Item1.GetPath().points[k].X;

                    if (largestY == 0 || largestY < dominoTransfer[i].Item1.GetPath().points[k].Y)
                        largestY = dominoTransfer[i].Item1.GetPath().points[k].Y;
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
                    if (selectedDominoes[i].StoneColor != SelectedColor.mediaColor)
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
                    if (((DominoInCanvas)DominoProject.Children[i]).StoneColor == SelectedColor.mediaColor && ((DominoInCanvas)DominoProject.Children[i]).isSelected == false)
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

            if (w > 6 || h > 6)
                rect.Visibility = System.Windows.Visibility.Visible;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            double top = Canvas.GetTop(rect);
            double right = Canvas.GetLeft(rect) + rect.ActualWidth;
            double bottom = Canvas.GetTop(rect) + rect.ActualHeight;
            double left = Canvas.GetLeft(rect);

            for (int i = 0; i < DominoProject.Children.Count - 1; i++)
            {
                DominoInCanvas dic = (DominoInCanvas)DominoProject.Children[i];
                if ((dic.RenderedGeometry.Bounds.Left > left && dic.RenderedGeometry.Bounds.Left < right
                    || dic.RenderedGeometry.Bounds.Right > left && dic.RenderedGeometry.Bounds.Right < right)
                    && (dic.RenderedGeometry.Bounds.Top > top && dic.RenderedGeometry.Bounds.Top < bottom
                    || dic.RenderedGeometry.Bounds.Bottom > top && dic.RenderedGeometry.Bounds.Bottom < bottom))
                {
                    if (!((DominoInCanvas)DominoProject.Children[i]).isSelected)
                    {
                        ((DominoInCanvas)DominoProject.Children[i]).isSelected = true;
                        selectedDominoes.Add(((DominoInCanvas)DominoProject.Children[i]));
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

        private ICommand _RemoveRowAbove;
        public ICommand RemoveRowAbove { get { return _RemoveRowAbove; } set { if (value != _RemoveRowAbove) { _RemoveRowAbove = value; } } }

        private ICommand _RemoveRowBelow;
        public ICommand RemoveRowBelow { get { return _RemoveRowBelow; } set { if (value != _RemoveRowBelow) { _RemoveRowBelow = value; } } }

        private ICommand _RemoveColumnRight;
        public ICommand RemoveColumnRight { get { return _RemoveColumnRight; } set { if (value != _RemoveColumnRight) { _RemoveColumnRight = value; } } }

        private ICommand _RemoveColumnLeft;
        public ICommand RemoveColumnLeft { get { return _RemoveColumnLeft; } set { if (value != _RemoveColumnLeft) { _RemoveColumnLeft = value; } } }

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

