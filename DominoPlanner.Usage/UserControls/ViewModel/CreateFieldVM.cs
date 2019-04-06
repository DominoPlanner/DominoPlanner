using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class CreateFieldVM : DominoProviderVM
    {
        #region CTOR
        public CreateFieldVM(FieldParameters dominoProvider, bool? AllowRegenerate) : base(dominoProvider, AllowRegenerate)
        {
            // Regeneration kurz sperren, dann wieder auf Ursprungswert setzen
            AllowRegeneration = false;
            field_templates = new List<StandardSize>();
            field_templates.Add(new StandardSize("8mm", new Sizes(8, 8, 24, 8)));
            field_templates.Add(new StandardSize("Tortoise", new Sizes(0, 48, 24, 0)));
            field_templates.Add(new StandardSize("User Size", new Sizes(10, 10, 10, 10)));
            Click_Binding = new RelayCommand((x) => BindSize = !BindSize);  
            ReloadSizes();
            Collapsible = System.Windows.Visibility.Visible;
            AllowRegeneration = AllowRegenerate;
            Refresh();
            if (fieldParameters.Counts != null) RefreshColorAmount();
            UnsavedChanges = false;
            SelectedItem.Sizes.PropertyChanged += Sizes_PropertyChanged;
        }
        #endregion

        #region fields
        Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
        FieldParameters fieldParameters
        {
            get { return CurrentProject as FieldParameters; }
            set
            {
                if (CurrentProject != value)
                {
                    CurrentProject = value;
                }
            }
        }
        #endregion

        #region prope
        public override TabItemType tabType
        {
            get
            {
                return TabItemType.CreateField;
            }
        }
        private FieldParameters FieldParameters
        {
            get => CurrentProject as FieldParameters;
        }
        private bool _EditState = true;
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
        public int Length
        {
            get { return FieldParameters.Length; }
            set
            {
                if (FieldParameters.Length != value)
                {
                    FieldParameters.Length = value;
                    if (BindSize)
                    {
                        double fieldWidth = Length * (fieldParameters.HorizontalDistance + fieldParameters.HorizontalSize);
                        double stoneHeightWidhSpace = fieldParameters.VerticalDistance + fieldParameters.VerticalSize;
                        fieldParameters.Height = (int)(fieldWidth / (double)fieldParameters.PrimaryImageTreatment.Width * fieldParameters.PrimaryImageTreatment.Height / stoneHeightWidhSpace);
                    }
                    RaisePropertyChanged();
                    Refresh();
                }
            }
        }

        public int Height
        {
            get { return FieldParameters.Height; }
            set
            {
                if (FieldParameters.Height != value)
                {
                    fieldParameters.Height = value;
                    if (BindSize)
                    {
                        double fieldHeight = Height * (fieldParameters.VerticalDistance + fieldParameters.VerticalSize);
                        double stoneWidthWidthSpace = fieldParameters.HorizontalDistance + fieldParameters.HorizontalSize;
                        fieldParameters.Length = (int)(fieldHeight / (double)fieldParameters.PrimaryImageTreatment.Height * fieldParameters.PrimaryImageTreatment.Width / stoneWidthWidthSpace);
                    }
                    RaisePropertyChanged();
                    Refresh();
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
                    SelectedItem.Sizes.PropertyChanged -= Sizes_PropertyChanged;
                    SelectedItem.Sizes.PropertyChanged += Sizes_PropertyChanged;
                    Sizes_PropertyChanged(null, null);
                    RefreshTargetSize();
                }
            }
        }

        private bool _BindSize = true;
        public bool BindSize
        {
            get { return _BindSize; }
            set
            {
                if (_BindSize != value)
                {
                    _BindSize = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool Horizontal
        {
            get { return CurrentProject.FieldPlanDirection == Core.Orientation.Horizontal; }
            set
            {
                if ((CurrentProject.FieldPlanDirection == Core.Orientation.Horizontal) != value)
                {
                    CurrentProject.FieldPlanDirection = value ? Core.Orientation.Horizontal : Core.Orientation.Vertical;
                    UpdateStoneSizes();
                    RefreshTargetSize();
                }
            }
        }
        #endregion

        #region Methods
        public override void Undo()
        {
            throw new NotImplementedException();
        }

        public override void Redo()
        {
            throw new NotImplementedException();
        }

        public override bool Save()
        {
            try
            {
                fieldParameters.Save();
                UnsavedChanges = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        protected override void PostCalculationUpdate()
        {
            TabPropertyChanged("Length", ProducesUnsavedChanges: false);
            TabPropertyChanged("Height", ProducesUnsavedChanges: false);
            TabPropertyChanged("DominoCount", ProducesUnsavedChanges: false);
        }
        private void UpdateStoneSizes()
        {
            if (!Horizontal)
            {
                fieldParameters.HorizontalDistance = SelectedItem.Sizes.d;
                fieldParameters.HorizontalSize = SelectedItem.Sizes.c;
                fieldParameters.VerticalSize = SelectedItem.Sizes.b;
                fieldParameters.VerticalDistance = SelectedItem.Sizes.a;
            }
            else
            {
                fieldParameters.HorizontalDistance = SelectedItem.Sizes.a;
                fieldParameters.HorizontalSize = SelectedItem.Sizes.b;
                fieldParameters.VerticalSize = SelectedItem.Sizes.c;
                fieldParameters.VerticalDistance = SelectedItem.Sizes.d;
            }
            ReloadSizes();
        }
        
        private void Sizes_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateStoneSizes();
            ReloadSizes();
            Refresh();
        }
        
        private void ReloadSizes()
        {
            Sizes currentSize = new Sizes(fieldParameters.HorizontalDistance, fieldParameters.HorizontalSize, fieldParameters.VerticalSize, fieldParameters.VerticalDistance);
            bool found = false;
            foreach (StandardSize sSize in field_templates)
            {
                if (Horizontal)
                {
                    if (sSize.Sizes.a == currentSize.a && sSize.Sizes.b == currentSize.b && sSize.Sizes.c == currentSize.c && sSize.Sizes.d == currentSize.d)
                    {
                        SelectedItem = sSize;
                        found = true;
                        break;
                    }
                }
                else
                {
                    if (sSize.Sizes.a == currentSize.d && sSize.Sizes.b == currentSize.c && sSize.Sizes.c == currentSize.b && sSize.Sizes.d == currentSize.a)
                    {
                        SelectedItem = sSize;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                field_templates.Last<StandardSize>().Sizes = currentSize;
                SelectedItem = field_templates.Last<StandardSize>();
            }
            else
            {
                if (SelectedItem.Name.Equals("User Size"))
                {
                    SelectedItem.Sizes.PropertyChanged -= Sizes_PropertyChanged;
                    SelectedItem.Sizes.PropertyChanged += Sizes_PropertyChanged;
                }
                else SelectedItem.Sizes.PropertyChanged -= Sizes_PropertyChanged;
            }
            UnsavedChanges = true;
        }
        #endregion

        private ICommand _Click_Binding;
        public ICommand Click_Binding { get { return _Click_Binding; } set { if (value != _Click_Binding) { _Click_Binding = value; } } }
        
        
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
