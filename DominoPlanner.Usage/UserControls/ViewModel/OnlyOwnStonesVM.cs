using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class OnlyOwnStonesVM : ModelBase
    {
        public ObservableCollection<ColorListEntry> Colors { get; set; }

        public ColumnConfig ColorColumnConfig { get; set; } = new ColumnConfig();
        private ICommand _OpenPopup;
        public ICommand OpenPopup { get {
                return _OpenPopup; }
            set { if (value != _OpenPopup) { _OpenPopup = value; } } }
        public OnlyOwnStonesVM()
        {
            OnlyUse = false;
            Weight = 0.5;
            Iterations = 1;

        }
        public OnlyOwnStonesVM(IterationInformation res)
        {
            _onlyUse = res is IterativeColorRestriction;
            if (_onlyUse)
            {
                var r = res as IterativeColorRestriction;
                _iteration = r.maxNumberOfIterations;
                _weight = r.iterationWeight;
            }
            OpenPopup = new RelayCommand(x => PopupOpen = true);
            ColorColumnConfig = new ColumnConfig();

            var columns = new ObservableCollection<Column>();
            columns.Add(new Column() { DataField = "DominoColor.mediaColor", Header = "" });
            columns.Add(new Column() { DataField = "DominoColor.name", Header = "Name" });
            columns.Add(new Column() { DataField = "DominoColor.count", Header = "Total" });
            columns.Add(new Column() { DataField = "SumAll", Header = "Used", HighlightDataField = "DominoColor.count" });
            columns.Add(new Column() { DataField = "Weight", Header = "Weight" });
            ColorColumnConfig.Columns = columns;
        }

        private bool _onlyUse;
        public bool OnlyUse
        {
            get { return _onlyUse; }
            set
            {
                if (_onlyUse != value)
                {
                    _onlyUse = value;
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

        private int _iteration;
        public int Iterations
        {
            get { return _iteration; }
            set
            {
                if (_iteration != value)
                {
                    _iteration = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _popupOpen;

        public bool PopupOpen
        {
            get {
                return _popupOpen;
            }
            set
            {
                _popupOpen = value; RaisePropertyChanged();
            }
        }
        private bool _ColorRestrictionFulfilled;

        public bool ColorRestrictionFulfilled
        {
            get { return _ColorRestrictionFulfilled; }
            set { _ColorRestrictionFulfilled = value; RaisePropertyChanged(); }
        }


    }
    public class BoolToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return "/Icons/ok.ico";
            else
                return "/Icons/closewindow.ico";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
