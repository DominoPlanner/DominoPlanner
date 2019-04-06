using DominoPlanner.Core;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class ImageTreatmentVM : ModelBase
    {
        public Action Refresh;
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
                    CurrentModel.Width = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    CurrentModel.Height = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    CurrentModel.Background = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    CurrentModel.ImageFilters = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    NRModel.Average = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    NRModel.AllowStretch = value;
                    RaisePropertyChanged();
                    Refresh();
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
        public Inter ResizeMode
        {
            get => FRModel.ResizeMode;
            set
            {
                if (FRModel.ResizeMode != value)
                {
                    FRModel.ResizeMode = value;
                    RaisePropertyChanged();
                    Refresh();
                }
            }
        }

        public FieldReadoutVM(FieldReadout model) : base(model)
        {

        }
    }
}
