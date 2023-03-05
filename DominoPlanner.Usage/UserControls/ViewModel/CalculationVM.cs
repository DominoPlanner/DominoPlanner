using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using System.Windows.Input;
using Avalonia.Collections;
using static DominoPlanner.Usage.ColorControl;
using static OfficeOpenXml.ExcelErrorValue;
using System.Diagnostics;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class CalculationVM : ModelBase
    {
        public Action<object, object, string, bool, Action, bool, Action> ValueChanged;
        protected void PropertyValueChanged(object sender, object value_new, [CallerMemberName]
        string membername = "", bool producesUnsavedChanges = true, Action PostAction = null, bool ChangesSize = false, Action PostUndoAction = null)
        {
            ValueChanged(sender, value_new, membername, producesUnsavedChanges, PostAction, ChangesSize, PostUndoAction);
        }

        internal Calculation currentModel;
        internal ColorRepository colorRepository;
        public CalculationVM(Calculation model, ColorRepository colorRepository)
        {
            currentModel = model;
            this.colorRepository = colorRepository;
            
        }
        public static CalculationVM CalculationVMFactory(Calculation model, ColorRepository colorRepository)
        {
            if (model is FieldCalculation f)
            {
                return new FieldCalculationVM(f, colorRepository);
            }
            else if (model is UncoupledCalculation u)
            {
                return new UncoupledCalculationVM(u, colorRepository);
            }
            return null;
        }
    }
    public class EmptyCalculationVM : CalculationVM
    {
        public EmptyCalculationVM(EmptyCalculation model, ColorRepository colorRepository) : base(model, colorRepository)
        {
            
        }
    }
    public class NonEmptyCalculationVM : CalculationVM
    {
        public NonEmptyCalculationVM(NonEmptyCalculation model, ColorRepository colorRepository) : base(model, colorRepository)
        {
            ColorColumnConfig.Add(new Column() { DataField = "DominoColor.mediaColor", Header = "", Class = "Color" });
            ColorColumnConfig.Add(new Column() { DataField = "DominoColor.name", Header = "Name", Width = new GridLength(100), CanResize = true });
            ColorColumnConfig.Add(new Column() { DataField = "SumAll", Header = "Used", HighlightDataField = "DominoColor.count" });
            
            IterationInformationVM = IterationInformationVM.IterationInformationVMFactory(model.IterationInformation);
            IterationInformationVM.ValueChanged = PropertyValueChanged;
            IterationInformationChanged = new RelayCommand( (o) => {if (o is bool b) ChangeIterationInformation(b); });
        }

        private void ChangeIterationInformation(bool IsChecked)
        {
            if (!IsChecked && IterationInformation is NoColorRestriction)
            {
                IterationInformation = new IterativeColorRestriction(2, 0.1);
            }
            else if (IsChecked && IterationInformation is IterativeColorRestriction)
            {
                IterationInformation = new NoColorRestriction();
            }
        }

        public bool updatingImage = false;

        private ColorListEntry _SelectedColor;
        public ColorListEntry SelectedColor
        {
            get
            {
                return _SelectedColor;
            }
            set
            {
                if (_SelectedColor != value)
                {
                    _SelectedColor = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("IsColorUsed");

                    if(SelectedColor != null)
                    {
                        if(ShouldUseForceMode && !updatingImage)
                        {
                            ChangeColorFilter(ForcedValue);
                            PropertyValueChanged(this, value);
                        }

                        if(ColorFilters == null)
                        {
                            IsColorUsed = true;
                        }
                        else
                        {
                            if (ColorFilters.OfType<ChangeCountColorFilter>().Any(x => x.Index == CalcIndex(SelectedColor.DominoColor as DominoColor)))
                            {
                                IsColorUsed = false;
                            }
                            else
                            {
                                IsColorUsed = true;
                            }
                        }
                    }
                }
            }
        }

        public AvaloniaList<Column> ColorColumnConfig { get; set; } = new AvaloniaList<Column>();

        private bool _ShouldUseForceMode;
        public bool ShouldUseForceMode
        {
            get
            {
                return _ShouldUseForceMode;
            }
            set
            {
                if(_ShouldUseForceMode != value)
                {
                    _ShouldUseForceMode = value;
                    RaisePropertyChanged("ShouldUseForceMode");
                }
            }
        }

        private bool _ForcedValue;
        public bool ForcedValue
        {
            get
            {
                return _ForcedValue;
            }
            set
            {
                if(_ForcedValue != value)
                {
                    _ForcedValue = value;
                    RaisePropertyChanged("ForcedValue");
                }
            }
        }

        private bool _IsColorUsed;

        public bool IsColorUsed
        {
            get
            {
                return _IsColorUsed;
            }
            set
            {
                if (_IsColorUsed != value)
                {
                    _IsColorUsed = value;
                    RaisePropertyChanged("IsColorUsed");

                    

                    ChangeColorFilter(IsColorUsed);

                    PropertyValueChanged(this, value);
                }
            }
        }

        private void ChangeColorFilter(bool activate)
        {
            if (SelectedColor != null)
            {
                if (ColorFilters == null)
                {
                    ColorFilters = new ObservableCollection<ColorFilter>();
                }

                ChangeCountColorFilter foundFilter = ColorFilters.OfType<ChangeCountColorFilter>().FirstOrDefault(x => x.Index == CalcIndex(SelectedColor.DominoColor as DominoColor));
                if (activate)
                {
                    if (foundFilter != null)
                    {
                        ColorFilters.Remove(foundFilter);
                    }
                }
                else if (foundFilter == null && SelectedColor.DominoColor is DominoColor dColor && colorRepository.colors.Contains(dColor))
                {
                    ChangeCountColorFilter changeCountColorFilter = new ChangeCountColorFilter();
                    changeCountColorFilter.NewCount = 0;
                    changeCountColorFilter.Index = CalcIndex(dColor);
                    ColorFilters.Add(changeCountColorFilter);
                }
            }
        }

        private int CalcIndex(DominoColor dominoColor)
        {
            if(dominoColor != null)
            {
                if(colorRepository?.colors != null && colorRepository.colors.Contains(dominoColor))
                {
                    return colorRepository.colors.IndexOf(dominoColor) + 1;
                }
            }
            return -1;
        }


        private NonEmptyCalculation NEModel
        {
            get => currentModel as NonEmptyCalculation;
        }
        public IColorComparison ColorMode
        {
            get
            {
                return NEModel.ColorMode;
            }
            set
            {
                if (!value.GetType().Equals(NEModel.ColorMode.GetType()))
                {
                    PropertyValueChanged(this, value);
                    NEModel.ColorMode = value;
                    RaisePropertyChanged();
                }
            }
        }
        public Dithering Dithering
        {
            get => NEModel.Dithering;
            set
            {
                if (value != NEModel.Dithering)
                {
                    PropertyValueChanged(this, value);
                    NEModel.Dithering = value;
                    RaisePropertyChanged();
                }
            }
        }
        public IterationInformation IterationInformation
        {
            get => NEModel.IterationInformation;
            set
            {
                if (value != NEModel.IterationInformation)
                {
                    PropertyValueChanged(this, value);
                    NEModel.IterationInformation = value;
                    IterationInformationVM = IterationInformationVM.IterationInformationVMFactory(value);
                    IterationInformationVM.ValueChanged = PropertyValueChanged;
                    RaisePropertyChanged();
                }
            }
        }
        public void TestMethod() { }
        // könnte man auch durch Converter ersetzen
        private IterationInformationVM _iterationinformationVM;
        public IterationInformationVM IterationInformationVM
        {
            get => _iterationinformationVM;
            set
            {
                if (_iterationinformationVM != value)
                {
                    _iterationinformationVM = value;
                    RaisePropertyChanged();
                }
            }
        }
        public byte TransparencySetting
        {
            get => NEModel.TransparencySetting;
            set
            {
                if (value != NEModel.TransparencySetting)
                {
                    PropertyValueChanged(this, value);
                    NEModel.TransparencySetting = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<ColorFilter> ColorFilters
        {
            get => NEModel.ColorFilters;
            set
            {
                if (value != NEModel.ColorFilters)
                {
                    PropertyValueChanged(this, value);
                    NEModel.ColorFilters = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ICommand _IterationInformationChanged;
        public ICommand IterationInformationChanged
        {
            get { return _IterationInformationChanged; }
            set { _IterationInformationChanged = value; RaisePropertyChanged(); }
        }
        
    }
    public class CoupledCalculationVM : NonEmptyCalculationVM
    {
        public CoupledCalculationVM(CoupledCalculation model, ColorRepository colorRepository) : base(model, colorRepository)
        {

        }
    }
    public class UncoupledCalculationVM : NonEmptyCalculationVM
    {
        public UncoupledCalculationVM(UncoupledCalculation model, ColorRepository colorRepository) : base(model, colorRepository)
        {

        }
    }
    public class FieldCalculationVM : NonEmptyCalculationVM
    {
        public FieldCalculationVM(FieldCalculation model, ColorRepository colorRepository) : base(model, colorRepository)
        {

        }
    }
    /*public class ColorRestrictionTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return null;

            switch (item)
            {
                case NoColorRestriction a: return NoColorRestrictionTemplate;
                case IterativeColorRestriction a: return IterativeColorRestrictionTemplate;
                default: return null;
            }
        }
        public DataTemplate NoColorRestrictionTemplate { get; set; }

        public DataTemplate IterativeColorRestrictionTemplate { get; set; }
    }*/

}
