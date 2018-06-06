using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class StructureViewModel : ModelBase
    {
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
    }
}
