using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class CalculationVM : ModelBase
    {
        public Action Refresh;


        internal Calculation currentModel;
        public CalculationVM(Calculation model)
        {
            currentModel = model;
            
        }
        public static CalculationVM CalculationVMFactory(Calculation model)
        {
            if (model is FieldCalculation f)
            {
                return new FieldCalculationVM(f);
            }
            else if (model is UncoupledCalculation u)
            {
                return new UncoupledCalculationVM(u);
            }
            return null;
        }
    }
    public class EmptyCalculationVM : CalculationVM
    {
        public EmptyCalculationVM(EmptyCalculation model) : base(model)
        {
            
        }
    }
    public class NonEmptyCalculationVM : CalculationVM
    {
        public NonEmptyCalculationVM(NonEmptyCalculation model) : base(model)
        {
            IterationInformationVM = IterationInformationVM.IterationInformationVMFactory(model.IterationInformation);
            IterationInformationVM.Refresh = Refresh;
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
                if (value != NEModel.ColorMode)
                {
                    NEModel.ColorMode = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    NEModel.Dithering = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    NEModel.IterationInformation = value;
                    IterationInformationVM = IterationInformationVM.IterationInformationVMFactory(value);
                    IterationInformationVM.Refresh = Refresh;
                    Refresh();
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
                    NEModel.TransparencySetting = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    NEModel.ColorFilters = value;
                    RaisePropertyChanged();
                    Refresh();
                }
            }
        }
        
    }
    public class CoupledCalculationVM : NonEmptyCalculationVM
    {
        public CoupledCalculationVM(CoupledCalculation model) : base(model)
        {

        }
    }
    public class UncoupledCalculationVM : NonEmptyCalculationVM
    {
        public UncoupledCalculationVM(UncoupledCalculation model) : base(model)
        {

        }
    }
    public class FieldCalculationVM : NonEmptyCalculationVM
    {
        public FieldCalculationVM(FieldCalculation model) : base(model)
        {

        }
    }
    public class ColorRestrictionTemplateSelector : DataTemplateSelector
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
    }

}
