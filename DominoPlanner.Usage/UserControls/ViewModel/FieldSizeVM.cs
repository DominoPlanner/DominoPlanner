using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class FieldSizeVM : ModelBase
    {
        //View.FieldSize asdf;
        #region CTOR
        public FieldSizeVM(bool EditState)
        {
            field_templates = new List<StandardSize>();
            field_templates.Add(new StandardSize("8mm", new Sizes(8, 8, 24, 8)));
            field_templates.Add(new StandardSize("Tortoise", new Sizes(0, 48, 24, 0)));
            field_templates.Add(new StandardSize("User Size", new Sizes(10, 10, 10, 10)));
            FieldSize = 5000;
            SelectedItem = field_templates[0];
            BindSize = true;
            Horizontal = true;
            this.EditState = EditState;
            if (EditState)
                Click_Binding = new RelayCommand(o => { BindSize = !BindSize; });
            else
                BindSize = false;
        }
        #endregion

        private ICommand _Click_Binding;
        public ICommand Click_Binding { get { return _Click_Binding; } set { if (value != _Click_Binding) { _Click_Binding = value; } } }


        #region prop
        private bool _EditState;
        public bool EditState
        {
            get { return _EditState; }
            set
            {
                if (_EditState != value)
                {
                    _EditState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _FieldSize;
        public int FieldSize
        {
            get { return _FieldSize; }
            set
            {
                if (_FieldSize != value)
                {
                    _FieldSize = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _Length;
        public double Length
        {
            get { return _Length; }
            set
            {
                if (_Length != value)
                {
                    _Length = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Height;
        public double Height
        {
            get { return _Height; }
            set
            {
                if (_Height != value)
                {
                    _Height = value;
                    RaisePropertyChanged();
                }
            }
        }


        private List<StandardSize> _field_templates;
        public List<StandardSize> field_templates
        {
            get { return _field_templates; }
            set
            {
                if (_field_templates != value)
                {
                    _field_templates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CanChange;
        public bool CanChange
        {
            get { return _CanChange; }
            set
            {
                if (_CanChange != value)
                {
                    _CanChange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private StandardSize _SelectedItem;
        public StandardSize SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (_SelectedItem != value)
                {
                    _SelectedItem = value;
                    if (SelectedItem.Name.Equals("User Size"))
                        CanChange = true;
                    else
                        CanChange = false;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _BindSize;
        public bool BindSize
        {
            get { return _BindSize; }
            set
            {
                if (_BindSize != value)
                {
                    _BindSize = value;
                    RaisePropertyChanged();
                    if (value)
                        BindUnbind = "/icons/lock.ico";
                    else
                        BindUnbind = "/icons/unlock.ico";
                }
            }
        }


        private string _BindUnbind;
        public string BindUnbind
        {
            get { return _BindUnbind; }
            set
            {
                if (_BindUnbind != value)
                {
                    _BindUnbind = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Horizontal;
        public bool Horizontal
        {
            get { return _Horizontal; }
            set
            {
                if (_Horizontal != value)
                {
                    _Vertical = !value;
                    _Horizontal = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Vertical;
        public bool Vertical
        {
            get { return _Vertical; }
            set
            {
                if (_Vertical != value)
                {
                    _Horizontal = !value;
                    _Vertical = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
    }

    class StandardSize : ModelBase
    {
        public StandardSize(string name, Sizes sizes)
        {
            this.Name = name;
            this.Sizes = sizes;
        }

        #region prop
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Sizes _sizes;
        public Sizes Sizes
        {
            get { return _sizes; }
            set
            {
                if (_sizes != value)
                {
                    _sizes = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public override string ToString()
        {
            return Name;
        }
    }

    class Sizes : ModelBase
    {
        public Sizes(int a, int b, int c, int d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        #region prop
        private int _a;
        public int a
        {
            get { return _a; }
            set
            {
                if (_a != value)
                {
                    _a = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _b;
        public int b
        {
            get { return _b; }
            set
            {
                if (_b != value)
                {
                    _b = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _c;
        public int c
        {
            get { return _c; }
            set
            {
                if (_c != value)
                {
                    _c = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _d;
        public int d
        {
            get { return _d; }
            set
            {
                if (_d != value)
                {
                    _d = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
    }
}