using Avalonia.Media;
using DominoPlanner.Core;
using System.Collections.ObjectModel;
using System.Linq;

namespace DominoPlanner.Usage
{
    // Viewmodel for DominoColor
    public class ColorListEntry : ModelBase
    {
        public ColorListEntry()
        {
            ProjectCount = new ObservableCollection<int>();
            SumAll = 0;
        }

        private void _ProjectCount_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SumAll = ProjectCount.Sum();
        }

        #region PROPERTIES
        private IDominoColor _DominoColor;
        public IDominoColor DominoColor
        {
            get { return _DominoColor; }
            set
            {
                if (_DominoColor != value)
                {
                    _DominoColor = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return DominoColor.name; }
            set { DominoColor.name = value; RaisePropertyChanged(); }
        }


        public int Count
        {
            get { return DominoColor.count; }
            set { DominoColor.count = value; RaisePropertyChanged(); }
        }

        public Color Color
        {
            get { return DominoColor.mediaColor; }
            set { DominoColor.mediaColor = value; RaisePropertyChanged(); }
        }


        private int _SortIndex;
        public int SortIndex
        {
            get { return _SortIndex; }
            set
            {
                if (_SortIndex != value)
                {
                    _SortIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SumAll;
        public int SumAll
        {
            get { return _SumAll; }
            set
            {
                if (_SumAll != value)
                {
                    _SumAll = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _weight;
        public double Weight
        {
            get { return _weight; }
            set
            {
                if (_weight != value)
                {
                    _weight = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<int> _ProjectCount;
        public ObservableCollection<int> ProjectCount
        {
            get { return _ProjectCount; }
            set
            {
                if (_ProjectCount != value)
                {
                    if (_ProjectCount != null)
                        _ProjectCount.CollectionChanged -= _ProjectCount_CollectionChanged; 
                    _ProjectCount = value;
                    if(_ProjectCount != null)
                        _ProjectCount.CollectionChanged += _ProjectCount_CollectionChanged;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
    }
}
