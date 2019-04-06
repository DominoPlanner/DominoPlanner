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
    
    public class IterativeColorRestrictionVM : IterationInformationVM
    {
        public IterativeColorRestrictionVM(IterativeColorRestriction model) : base(model)
        {

        }
        private IterativeColorRestriction IterativeColorRestriction
        {
            get => currentModel as IterativeColorRestriction;
        }
        public double Weight
        {
            get { return IterativeColorRestriction.iterationWeight; }
            set
            {
                if (IterativeColorRestriction.iterationWeight != value)
                {
                    IterativeColorRestriction.iterationWeight = value;
                    RaisePropertyChanged();
                    Refresh();
                }
            }
        }
        public int Iterations
        {
            get { return IterativeColorRestriction.maxNumberOfIterations; }
            set
            {
                if (IterativeColorRestriction.maxNumberOfIterations != value)
                {
                    IterativeColorRestriction.maxNumberOfIterations = value;
                    RaisePropertyChanged();
                    Refresh();
                }
            }
        }
    }
    public class NoColorRestrictionVM : IterationInformationVM
    {
        public NoColorRestrictionVM(NoColorRestriction model) : base(model)
        {

        }
    }
   public  class IterationInformationVM : ModelBase
    {
        public Action Refresh;

        public IterationInformation currentModel;
        public IterationInformationVM(IterationInformation model)
        {
            currentModel = model;
        }
        public static IterationInformationVM IterationInformationVMFactory(IterationInformation model)
        {
            if (model is NoColorRestriction nc)
                return new NoColorRestrictionVM(nc);
            else if (model is IterativeColorRestriction ic)
                return new IterativeColorRestrictionVM(ic);
            return null;
        }
    }
    
}
