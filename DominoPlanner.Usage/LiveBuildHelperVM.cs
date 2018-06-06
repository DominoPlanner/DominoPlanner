using DominoPlanner.Core;
using DominoPlanner.Core.ColorMine.Comparisons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage
{
    class LiveBuildHelperVM : ModelBase
    {
        #region CTOR
        public LiveBuildHelperVM(IDominoProvider pFParameters, int pBlockSize)
        {
            blockSize = pBlockSize;
            fParameters = pFParameters;
            intField = fParameters.GetBaseField();
            CountRow = intField.GetLength(1);
            stonesPerLine = intField.GetLength(0);
            CountBlock = Convert.ToInt32(Math.Ceiling(((double)stonesPerLine / blockSize)));
            SizeChanged = new RelayCommand(o => { RefreshCanvas(); });
            MouseDown = new RelayCommand(o => { currentBlock.Focus(); });
        }
        #endregion

        #region fields
        private int blockSize;
        private IDominoProvider fParameters;
        private int stonesPerLine;
        private int[,] intField;
        #endregion

        #region prope
        private Canvas _currentBlock = new Canvas();
        public Canvas currentBlock
        {
            get { return _currentBlock; }
            set
            {
                if (_currentBlock != value)
                {
                    _currentBlock = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedRow = 1;
        public int SelectedRow
        {
            get { return _SelectedRow; }
            set
            {
                if (_SelectedRow != value)
                {
                    _SelectedRow = value;
                    RaisePropertyChanged();
                    RefreshCanvas();
                    currentBlock.Focus();
                }
            }
        }

        private int _CountRow;
        public int CountRow
        {
            get { return _CountRow; }
            set
            {
                if (_CountRow != value)
                {
                    _CountRow = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedBlock = 1;
        public int SelectedBlock
        {
            get { return _SelectedBlock; }
            set
            {
                if (_SelectedBlock != value)
                {
                    _SelectedBlock = value;
                    RaisePropertyChanged();
                    RefreshCanvas();
                    currentBlock.Focus();
                }
            }
        }

        private int _CountBlock;
        public int CountBlock
        {
            get { return _CountBlock; }
            set
            {
                if (_CountBlock != value)
                {
                    _CountBlock = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region methods
        private void RefreshCanvas()
        {
            currentBlock.Children.RemoveRange(0, currentBlock.Children.Count);
            int space = 2;

            int stoneWidth = (((int)currentBlock.ActualWidth) - (2 * 2 * space) - ((blockSize - 1) * space)) / blockSize;
            int stoneHeight = 250;
            int marginHeight = (((int)currentBlock.ActualHeight) / 2);

            int firstBlockStone = blockSize * (SelectedBlock - 1);

            int lastColor = intField[(SelectedBlock - 1) * blockSize, SelectedRow - 1];
            string lastColorName = fParameters.colors[lastColor].name;
            int lastLeftMargin = 2 * space;
            int countColor = 0;

            for (int i = 0; i < blockSize; i++)
            {
                if (firstBlockStone + i < stonesPerLine)
                {
                    currentBlock.Children.Add(new DominoInCanvas(stoneWidth, stoneHeight, ((i + 2) * space) + (i * stoneWidth), marginHeight - stoneHeight, fParameters.colors[intField[firstBlockStone + i, SelectedRow - 1]].mediaColor));
                    
                    if (lastColor != intField[firstBlockStone + i, SelectedRow - 1])
                    {
                        TextBlock tb = new TextBlock();
                        tb.FontSize = 16;
                        tb.FontWeight = System.Windows.FontWeights.Bold;
                        tb.TextAlignment = System.Windows.TextAlignment.Center;
                        tb.Margin = new System.Windows.Thickness(lastLeftMargin, marginHeight + 20, 0, 0);
                        tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        tb.Text = lastColorName + Environment.NewLine + countColor;
                        tb.Width = ((i + 1) * space) + (i * stoneWidth) - lastLeftMargin;
                        currentBlock.Children.Add(tb);
                        lastColor = intField[firstBlockStone + i, SelectedRow - 1];
                        lastColorName = fParameters.colors[intField[firstBlockStone + i, SelectedRow - 1]].name;
                        lastLeftMargin = ((i + 1) * space) + (i * stoneWidth);
                        countColor = 0;
                    }

                    if (i == blockSize - 1 || firstBlockStone + i == stonesPerLine - 1)
                    {
                        countColor++;
                        TextBlock tb = new TextBlock();
                        tb.FontSize = 16;
                        tb.FontWeight = System.Windows.FontWeights.Bold;
                        tb.TextAlignment = System.Windows.TextAlignment.Center;
                        tb.Margin = new System.Windows.Thickness(lastLeftMargin, marginHeight + 20, 0, 0);
                        tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        tb.Width = ((i + 2) * space) + ((i + 1) * stoneWidth) - lastLeftMargin;
                        tb.Text = lastColorName + Environment.NewLine + countColor;
                        currentBlock.Children.Add(tb);
                        break;
                    }
                    else
                        countColor++;
                }
            }
        }

        internal void PressedKey(Key pressedKey)
        {
            switch (pressedKey)
            {
                case Key.Space:
                    if (SelectedBlock == CountBlock)
                    {
                        if (SelectedRow < CountRow)
                        {
                            SelectedBlock = 1;
                            SelectedRow++;
                        }
                    }
                    else
                    {
                        SelectedBlock++;
                    }
                    break;
                case Key.Left:
                    if (SelectedBlock == 1)
                    {
                        if (SelectedRow > 1)
                        {
                            SelectedRow--;
                            SelectedBlock = CountBlock;
                        }
                    }
                    else
                    {
                        SelectedBlock--;
                    }
                    break;
                case Key.Right:
                    if (SelectedBlock == CountBlock)
                    {
                        if (SelectedRow < CountRow)
                        {
                            SelectedBlock = 1;
                            SelectedRow++;
                        }
                    }
                    else
                        SelectedBlock++;
                    break;
                case Key.Up:
                    if (SelectedRow > 1)
                        SelectedRow--;
                    break;
                case Key.Down:
                    if (SelectedRow < CountRow)
                        SelectedRow++;
                    break;
                default:
                    break;
            }
        }
        #endregion



        private ICommand _MouseDown;
        public ICommand MouseDown { get { return _MouseDown; } set { if (value != _MouseDown) { _MouseDown = value; } } }


        private ICommand _SizeChanged;
        public ICommand SizeChanged { get { return _SizeChanged; } set { if (value != _SizeChanged) { _SizeChanged = value; } } }

    }
}
