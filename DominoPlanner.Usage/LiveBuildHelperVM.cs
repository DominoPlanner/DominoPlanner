using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia.Collections;
using static DominoPlanner.Usage.ColorControl;
using Avalonia.Data.Converters;
using System.Globalization;
using System.Timers;
using System.ComponentModel;

namespace DominoPlanner.Usage
{
    using static Localizer;
    class LiveBuildHelperVM : ModelBase
    {
        private ICommand _OpenPopup;
        public ICommand OpenPopup
        {
            get
            {
                return _OpenPopup;
            }
            set { if (value != _OpenPopup) { _OpenPopup = value; } }
        }

        #region CTOR
        public LiveBuildHelperVM(IDominoProvider pFParameters, int pBlockSize, Core.Orientation orientation, bool MirrorX, bool MirrorY)
        {
            BlockSize = pBlockSize;
            fParameters = pFParameters;
            intField = fParameters.GetBaseField(orientation, MirrorX, MirrorY);
            NextN = 500;
            CountRow = intField.GetLength(1);
            stonesPerLine = intField.GetLength(0);
            CountBlock = Convert.ToInt32(Math.Ceiling(((double)stonesPerLine / BlockSize)));
            SizeChanged = new RelayCommand(o => { RefreshCanvas(); });
            MouseDown = new RelayCommand(o => { CurrentBlock.Focus(); });
            PositionSnapshots = new ObservableCollection<PositionSnapshot>();
            MakePositionSnapshot = new RelayCommand(o => {
                PositionSnapshots.RemoveAll(x => x.Column == SelectedBlock && x.Row == SelectedRow);
                if ((bool)o == true)
                    PositionSnapshots.Insert(0, new PositionSnapshot() { Column = SelectedBlock, Row = SelectedRow });
                RaisePropertyChanged(nameof(PositionSnapshots));
            });
            GoToPositionSnapshot = new RelayCommand(o => { if (o is PositionSnapshot ps) { SelectedBlock = ps.Column; SelectedRow = ps.Row; } });
            ColumnConfig = new AvaloniaList<Column>
            {
                new Column() { DataField = "DominoColor.mediaColor", Header = "", Class = "Color" },
                new Column() { DataField = "DominoColor.name", Header = _("Name") },
                new Column() { DataField = "ProjectCount[0]", Header = GetParticularString("Number of stones available", "Total") },
                new Column() { DataField = "ProjectCount[1]", Header = GetParticularString("Remaining number of stones", "Remaining"), Class="Count" },
                new Column() { DataField = "ProjectCount[2]", Header = string.Format(GetParticularString("Dominoes of the given color within the next {0}", "Next {0}"), NextN) }
            };

            OpenPopup = new RelayCommand(x => { FillColorList(); PopupOpen = true; });

            CurrentStones = new ObservableCollection<SolidColorBrush>();
            HistStones = new ObservableCollection<SolidColorBrush>();
            ColorNames = new ObservableCollection<ColorAmount>();

            RefreshCanvas();
        }
        #endregion

        #region fields
        private readonly IDominoProvider fParameters;
        private readonly int stonesPerLine;
        private readonly int[,] intField;
        private readonly int space = 2;
        private int stoneWidth = 0;
        #endregion

        #region prope
        private int _BlockSize;
        public int BlockSize
        {
            get { return _BlockSize; }
            set
            {
                if (_BlockSize != value)
                {
                    _BlockSize = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _BatState;
        public string BatState
        {
            get { return _BatState; }
            set
            {
                if (_BatState != value)
                {
                    _BatState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Canvas _currentBlock = new Canvas();
        public Canvas CurrentBlock
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
                    CurrentBlock.Focus();
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
                    CurrentBlock.Focus();
                }
            }
        }

        private bool _ShowHistory;
        public bool ShowHistory
        {
            get { return _ShowHistory; }
            set
            {
                if(_ShowHistory != value)
                {
                    _ShowHistory = value;
                    RaisePropertyChanged();
                    RefreshCanvas();
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
        private bool _popupOpen;
        public bool PopupOpen
        {
            get
            {
                return _popupOpen;
            }
            set
            {
                _popupOpen = value; RaisePropertyChanged();
            }
        }

        private AvaloniaList<Column> _columnConfig;

        public AvaloniaList<Column> ColumnConfig
        {
            get { return _columnConfig; }
            set { _columnConfig = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<ColorListEntry> _colors;

        public ObservableCollection<ColorListEntry> Colors
        {
            get { return _colors; }
            set { _colors = value; RaisePropertyChanged(); }
        }
        private int _nextN;

        public int NextN
        {
            get { return _nextN; }
            set { _nextN = value; }
        }

        private ObservableCollection<SolidColorBrush> _CurrentStones;
        public ObservableCollection<SolidColorBrush> CurrentStones
        {
            get { return _CurrentStones; }
            set
            {
                if (_CurrentStones != value)
                {
                    _CurrentStones = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<SolidColorBrush> _HistStones;
        public ObservableCollection<SolidColorBrush> HistStones
        {
            get { return _HistStones; }
            set
            {
                if (_HistStones != value)
                {
                    _HistStones = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ColorAmount> _ColorNames;
        public ObservableCollection<ColorAmount> ColorNames
        {
            get { return _ColorNames; }
            set
            {
                if (_ColorNames != value)
                {
                    _ColorNames = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<PositionSnapshot> _Snapshots;

        public ObservableCollection<PositionSnapshot> PositionSnapshots
        {
            get { return _Snapshots; }
            set { _Snapshots = value; RaisePropertyChanged(); }
        }
        private int _FontSize;
        [SettingsAttribute("LiveBuildHelperVM", 18)]
        public int FontSize
        {
            get { return _FontSize; }
            set { _FontSize = value; RaisePropertyChanged(); }
        }



        #endregion

        #region methods
        private void RefreshCanvas()
        {
            CurrentStones.Clear();
            HistStones.Clear();
            ColorNames.Clear();
            CurrentBlock.Children.RemoveRange(0, CurrentBlock.Children.Count);
            
            stoneWidth = (((int)CurrentBlock.Bounds.Width) - (2 * 2 * space) - ((BlockSize - 1) * space)) / BlockSize;
            
            int firstBlockStone = BlockSize * (SelectedBlock - 1);

            for (int i = 0; i < BlockSize; i++)
            {
                if (firstBlockStone + i < stonesPerLine)
                {
                    if(ShowHistory && SelectedRow - 2 >= 0)
                    {
                        int oldStoneIndex = intField[firstBlockStone + i, SelectedRow - 2];
                        if(oldStoneIndex >= 0 && fParameters.colors.Length > oldStoneIndex) HistStones.Add(new SolidColorBrush(fParameters.colors[oldStoneIndex].mediaColor));
                    }

                    int stoneindex = intField[firstBlockStone + i, SelectedRow - 1];
                    if (stoneindex < 0) continue;
                    
                    CurrentStones.Add(new SolidColorBrush(fParameters.colors[stoneindex].mediaColor));

                    if (ColorNames.Count == 0 || !ColorNames.LastOrDefault().ColorName.Equals(fParameters.colors[stoneindex].name))
                    {
                        ColorNames.Add(new ColorAmount(fParameters.colors[stoneindex].name, 1));
                    }
                    else
                    {
                        ColorNames.LastOrDefault().Amount++;
                    }
                }
            }
        }

        private void FillColorList()
        {
            Colors = new ObservableCollection<ColorListEntry>();

            int counter = 0;

            if (fParameters.colors.RepresentionForCalculation.OfType<EmptyDomino>().Count() == 1)
            {
                Colors.Add(new ColorListEntry() { DominoColor = fParameters.colors.RepresentionForCalculation.OfType<EmptyDomino>().First(), SortIndex = -1 });
            }
            foreach (DominoColor domino in fParameters.colors.RepresentionForCalculation.OfType<DominoColor>())
            {
                Colors.Add(new ColorListEntry() { DominoColor = domino, SortIndex = fParameters.colors.Anzeigeindizes[counter] });
                counter++;
            }

            RefreshColorAmount();
        }
        private void RefreshColorAmount()
        {
            int firstBlockStone = BlockSize * (SelectedBlock - 1);
            int firstRow = SelectedRow - 1;
            int[] RemainingCount = new int[Colors.Count];
            int[] NextNCount = new int[Colors.Count];
            int counter = 0;
            for (int i = firstRow; i < intField.GetLength(1); i++)
            {
                int startj = (i == firstRow) ? firstBlockStone : 0; 
                for (int j = startj; j < intField.GetLength(0); j++)
                {
                    if (counter < NextN && intField[j, i] > 0)
                    {
                        NextNCount[intField[j, i]]++;
                        counter++;
                    }
                    if(intField[j, i] >= 0 && intField[j, i] < RemainingCount.Count()) RemainingCount[intField[j, i]]++;
                    
                }
            }
            for (int i = 0; i < Colors.Count; i++)
            {
                if (fParameters.Counts.Length > i)
                {
                    Colors[i].ProjectCount = new ObservableCollection<int>
                    {
                        fParameters.Counts[i],
                        RemainingCount[i],
                        NextNCount[i]
                    };

                }
                else
                {
                    Colors[i].ProjectCount.Add(fParameters.Counts[0]);
                }
            }
            Colors = new ObservableCollection<ColorListEntry>(Colors.Where(x => x.ProjectCount[0] > 0).OrderBy(x => x.SortIndex));
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
                case Key.P:
                    MakePositionSnapshot.Execute(true);
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

        private ICommand _GoToPositionSnapshot;

        public ICommand GoToPositionSnapshot
        {
            get { return _GoToPositionSnapshot; }
            set { _GoToPositionSnapshot = value;  RaisePropertyChanged();  }
        }
        private ICommand _MakePositionSnapshot;

        public ICommand MakePositionSnapshot
        {
            get { return _MakePositionSnapshot; }
            set { _MakePositionSnapshot = value; RaisePropertyChanged(); }
        }


    }
    public class IsSnapshottedConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count == 3 && values[0] is IEnumerable<PositionSnapshot> list && values[1] is int Row && values[2] is int Block)
            {
                return list.Where(x => (x.Column == Block && x.Row == Row)).Count() != 0;
            }
            return false;
        }
    }
    public class DateTimeDifferenceConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count == 2 && values[0] is DateTime dt1 && values[1] is DateTime dt2)
            {
                var delta = dt1 - dt2;
                if (delta.Days > 0)
                {
                    return string.Format("{0} days ago", delta.ToString("%d"));
                }
                else if (delta.Hours > 0)
                {
                    return string.Format("{0} hours ago", delta.ToString("%h"));
                }
                else if (delta.Minutes > 0)
                {
                    return string.Format("{0} minutes ago", delta.ToString("%m"));
                }
                else 
                {
                    return string.Format("{0} seconds ago", delta.ToString("%s"));
                }
            }
            return null;
        }
    }

    public class PositionSnapshot : ModelBase
    {
        public PositionSnapshot()
        {
            CreationTime = DateTime.Now;
        }
        private int row;

        public int Row
        {
            get { return row; }
            set { row = value; RaisePropertyChanged();  }
        }
        private int column;

        public int Column
        {
            get { return column; }
            set { column = value; RaisePropertyChanged(); }
        }
        private DateTime dateTime;

        public DateTime CreationTime
        {
            get { return dateTime; }
            set { dateTime = value; RaisePropertyChanged(); }
        }


    }
    public class Ticker : ModelBase
    {
        public Ticker()
        {
            Timer timer = new Timer();
            timer.Interval = 1000; // 1 second updates
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public DateTime Now
        {
            get { return DateTime.Now; }
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RaisePropertyChanged(nameof(Now));
        }
    }
    public class ColorAmount : ModelBase
    {
        public ColorAmount(string colorName, int amount)
        {
            ColorName = colorName;
            Amount = amount;
        }

        private string _ColorName;
        public string ColorName
        {
            get { return _ColorName; }
            set
            {
                if (_ColorName != value)
                {
                    _ColorName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Amount;
        public int Amount
        {
            get { return _Amount; }
            set
            {
                if (_Amount != value)
                {
                    _Amount = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override string ToString()
        {
            return $"{Amount} {ColorName}";
        }
    }
}
