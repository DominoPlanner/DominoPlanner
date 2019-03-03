using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class OnlyOwnStonesVM : ModelBase
    {
        public OnlyOwnStonesVM()
        {
            OnlyUse = false;
            Weight = 0.5;
            Iterations = 1;
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
                if(_weight != value)
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
    }
}
