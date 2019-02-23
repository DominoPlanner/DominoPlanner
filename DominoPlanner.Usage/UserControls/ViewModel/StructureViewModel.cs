using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class StructureViewModel : ModelBase
    {
        public StructureViewModel()
        {
            StandAlone = false;
        }

        private bool _StandAlone;
        public bool StandAlone
        {
            get { return _StandAlone; }
            set
            {
                if (_StandAlone != value)
                {
                    _StandAlone = value;
                    RaisePropertyChanged();
                    ShowDetailSize = StandAlone ? Visibility.Hidden : Visibility.Visible;
                }
            }
        }

        private Visibility _ShowDetailSize;
        public Visibility ShowDetailSize
        {
            get { return _ShowDetailSize; }
            set
            {
                if (_ShowDetailSize != value)
                {
                    _ShowDetailSize = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _StrucSize;
        public int StrucSize
        {
            get { return _StrucSize; }
            set
            {
                if (_StrucSize != value)
                {
                    _StrucSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        protected XElement selectedStructureElement;

        public XElement SelectedStructureElement
        {
            get
            {
                return selectedStructureElement;
            }
        }
    }
}
