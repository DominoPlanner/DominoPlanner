using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
