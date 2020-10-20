using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DominoPlanner.UI.UserControls.ViewModel
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
                    PropertyValueChanged(this, value);
                    IterativeColorRestriction.iterationWeight = value;
                    RaisePropertyChanged();
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
                    PropertyValueChanged(this, value);
                    IterativeColorRestriction.maxNumberOfIterations = value;
                    RaisePropertyChanged();
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
        public Action<object, object, string, bool, Action, bool, Action> ValueChanged;
        protected void PropertyValueChanged(object sender, object value_new, [CallerMemberName]
        string membername = "", bool producesUnsavedChanges = true, Action PostAction = null, bool ChangesSize = false, Action PostUndoAction = null)
        {
            ValueChanged(sender, value_new, membername, producesUnsavedChanges, PostAction, ChangesSize, PostUndoAction);
        }
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
