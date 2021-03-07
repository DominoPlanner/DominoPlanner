using Avalonia.Media;
using DominoPlanner.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DominoPlanner.Usage
{
    // Viewmodel for DominoColor
    public class ColorListEntry : ModelBase
    {
        public Action<object, object, string, bool, Action, Action> ValueChanged;
        protected void PropertyValueChanged(object sender, object value_new, [CallerMemberName]
        string membername = "", bool producesUnsavedChanges = true, Action PostAction = null, Action PostUndoAction = null)
        {
            ValueChanged?.Invoke(sender, value_new, membername, producesUnsavedChanges, PostAction, PostUndoAction);
        }
        public ColorListEntry()
        {
            ProjectCount = new ObservableCollection<int>();
            SumAll = 0;
        }

        private void ProjectCount_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            set { PropertyValueChanged(this, value); DominoColor.name = value; RaisePropertyChanged(); }
        }


        public int Count
        {
            get { return DominoColor.count; }
            set { PropertyValueChanged(this, value); DominoColor.count = value; RaisePropertyChanged(); }
        }

        public Color Color
        {
            get { return DominoColor.mediaColor; }
            set { PropertyValueChanged(this, value); DominoColor.mediaColor = value; RaisePropertyChanged(); }
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

        public bool Deleted
        {
            get { return DominoColor.Deleted; }
            set { DominoColor.Deleted = value; RaisePropertyChanged(); }
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
                        _ProjectCount.CollectionChanged -= ProjectCount_CollectionChanged;
                    _ProjectCount = value;
                    if (_ProjectCount != null)
                        _ProjectCount.CollectionChanged += ProjectCount_CollectionChanged;
                    RaisePropertyChanged();
                }
            }
        }
        public DominoColorState GetColorState()
        {
            return GetColorState(Deleted, ProjectCount);
        }

        public static DominoColorState GetColorState(bool deleted, ObservableCollection<int> counts)
        {
            if (!deleted)
                return DominoColorState.Active;
            foreach (int count in counts)
                if (count > 0)
                    return DominoColorState.Inactive;
            return DominoColorState.Deleted;
        }
        #endregion
    }
    public enum DominoColorState
    {
        Active,
        Inactive,
        Deleted
    }
}
