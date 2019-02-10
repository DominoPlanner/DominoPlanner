using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class RoundSizeVM : StructureViewModel
    {
        #region CTOR
        public RoundSizeVM()
        {
            list = new List<String>();
            list.Add("Spiral");
            list.Add("Circle Bomb");
            TypeSelected = "Spiral";
            PossibleTypeChange = false;
        }
        #endregion

        #region Prop
        private bool _PossibleTypeChange;
        public bool PossibleTypeChange
        {
            get { return _PossibleTypeChange; }
            set
            {
                if (_PossibleTypeChange != value)
                {
                    _PossibleTypeChange = value;
                    RaisePropertyChanged();
                }
            }
        }


        private List<string> _list;
        public List<string> list
        {
            get { return _list; }
            set
            {
                if (_list != value)
                {
                    _list = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TypeSelected;
        public string TypeSelected
        {
            get { return _TypeSelected; }
            set
            {
                if (_TypeSelected != value)
                {
                    _TypeSelected = value;
                    if (value.CompareTo("Spiral") == 0)
                    {
                        lType = "Quarter rotations:";
                    }else if(value.CompareTo("Circle Bomb") == 0)
                    {
                        lType = "Rounds";
                    }
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



        private string _lType;
        public string lType
        {
            get { return _lType; }
            set
            {
                if (_lType != value)
                {
                    _lType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _dWidth = 8;
        public int dWidth
        {
            get { return _dWidth; }
            set
            {
                if (_dWidth != value)
                {
                    _dWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _dHeight = 24;
        public int dHeight
        {
            get { return _dHeight; }
            set
            {
                if (_dHeight != value)
                {
                    _dHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _beLines = 8;
        public int beLines
        {
            get { return _beLines; }
            set
            {
                if (_beLines != value)
                {
                    _beLines = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _beDominoes = 8;
        public int beDominoes
        {
            get { return _beDominoes; }
            set
            {
                if (_beDominoes != value)
                {
                    _beDominoes = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
    }
}
