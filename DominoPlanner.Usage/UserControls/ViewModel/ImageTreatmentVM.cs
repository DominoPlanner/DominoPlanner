using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class ImageTreatmentVM : ModelBase
    {
        public Action<object, object, string, bool, Action, bool, Action> ValueChanged;
        protected void PropertyValueChanged(object sender, object value_new, [CallerMemberName]
        string membername = "", bool producesUnsavedChanges = true, Action PostAction = null, bool ChangesSize = false, Action PostUndoAction = null)
        {
            ValueChanged(sender, value_new, membername, producesUnsavedChanges, PostAction, ChangesSize, PostUndoAction);
        }
        protected ImageTreatment CurrentModel;
        public ImageTreatmentVM(ImageTreatment model)
        {
            CurrentModel = model;
        }
        public static ImageTreatmentVM ImageTreatmentVMFactory(ImageTreatment model)
        {
            if (model is FieldReadout f)
            {
                return new FieldReadoutVM(f);
            }
            else if (model is NormalReadout n)
            {
                return new NormalReadoutVM(n);
            }
            return null;
        }
        public int Width
        {
            get => CurrentModel.Width;
            set
            {
                if (CurrentModel.Width != value)
                {
                    PropertyValueChanged(this, value);
                    CurrentModel.Width = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int Height
        {
            get => CurrentModel.Height;
            set
            {
                if (CurrentModel.Height != value)
                {
                    PropertyValueChanged(this, value);
                    CurrentModel.Height = value;
                    RaisePropertyChanged();
                }
            }
        }
        public Color Background
        {
            get => CurrentModel.Background;
            set
            {
                if (CurrentModel.Background != value)
                {
                    PropertyValueChanged(this, value);
                    CurrentModel.Background = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<ImageFilter> ImageFilters
        {
            get => CurrentModel.ImageFilters;
            set
            {
                if (CurrentModel.ImageFilters != value)
                {
                    PropertyValueChanged(this, value);
                    CurrentModel.ImageFilters = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
    public class NormalReadoutVM : ImageTreatmentVM
    {
        public NormalReadoutVM(NormalReadout model) : base(model)
        {

        }
        NormalReadout NRModel
        {
            get => CurrentModel as NormalReadout;
        }
        public AverageMode AverageMode
        {
            get => NRModel.Average;
            set
            {
                if (NRModel.Average != value)
                {
                    PropertyValueChanged(this, value);
                    NRModel.Average = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool AllowStretch
        {
            get => NRModel.AllowStretch;
            set
            {
                if (NRModel.AllowStretch != value)
                {
                    PropertyValueChanged(this, value);
                    NRModel.AllowStretch = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
    public class FieldReadoutVM : ImageTreatmentVM
    {
        private FieldReadout FRModel
        {
            get => CurrentModel as FieldReadout;
        }
        public SkiaSharp.SKFilterQuality ResizeQuality
        {
            get => FRModel.ResizeQuality;
            set
            {
                if (FRModel.ResizeQuality != value)
                {
                    PropertyValueChanged(this, value);
                    FRModel.ResizeQuality = value;
                    RaisePropertyChanged();
                }
            }
        }

        public FieldReadoutVM(FieldReadout model) : base(model)
        {

        }
    }
}
