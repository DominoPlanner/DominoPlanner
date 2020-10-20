using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.UI.UserControls.ViewModel
{
    public class RoundStructureVM : CreateStructureVM
    {
        public CircularStructure CircularStructure
        {
            get => CurrentProject as CircularStructure;
        }
        public int TangentialWidth
        {
            get => CircularStructure.DominoWidth;
            set
            {
                if (CircularStructure.DominoWidth != value)
                {
                    PropertyValueChanged(this, value);
                    CircularStructure.DominoWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int NormalWidth
        {
            get => CircularStructure.DominoLength;
            set
            {
                if (CircularStructure.DominoLength != value)
                {
                    PropertyValueChanged(this, value);
                    CircularStructure.DominoLength = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int TangentialDistance
        {
            get => CircularStructure.TangentialDistance;
            set
            {
                if (CircularStructure.TangentialDistance != value)
                {
                    PropertyValueChanged(this, value);
                    CircularStructure.TangentialDistance = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int NormalDistance
        {
            get => CircularStructure.NormalDistance;
            set
            {
                if (CircularStructure.NormalDistance != value)
                {
                    PropertyValueChanged(this, value);
                    CircularStructure.NormalDistance = value;
                    RaisePropertyChanged();
                }
            }
        }
        public RoundStructureVM(CircularStructure dn, bool? AllowRegenerate) : base(dn, AllowRegenerate)
        {

        }
    }
    public class CreateCircleVM : RoundStructureVM
    {
        public CircleParameters CircleParameters
        {
            get => CurrentProject as CircleParameters;
        }
        public int Rotations
        {
            get => CircleParameters.Circles;
            set
            {
                if (CircleParameters.Circles != value)
                {
                    PropertyValueChanged(this, value);
                    CircleParameters.Circles = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int ForceDivisibility
        {
            get => CircleParameters.ForceDivisibility;
            set
            {
                if (CircleParameters.ForceDivisibility != value)
                {
                    PropertyValueChanged(this, value);
                    CircleParameters.ForceDivisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        public double? AngleShiftFactor
        {
            get => CircleParameters.AngleShiftFactor;
            set
            {
                if (CircleParameters.AngleShiftFactor != value)
                {
                    PropertyValueChanged(this, value);
                    CircleParameters.AngleShiftFactor = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool RandomShiftFactor
        {
            get => CircleParameters.RandomShiftFactor;
            set
            {
                if (CircleParameters.RandomShiftFactor != value)
                {
                    PropertyValueChanged(this, value);
                    AngleShiftFactor = value ? null : (double?)0.05;
                    RaisePropertyChanged();
                }
            }
        }
        public CreateCircleVM(CircleParameters dn, bool? AllowRegenerate) : base(dn, AllowRegenerate)
        {
            Refresh();
            UnsavedChanges = false;
        }
    }
    public class CreateSpiralVM : RoundStructureVM
    {
        public SpiralParameters SpiralParameters
        {
            get => CurrentProject as SpiralParameters;
        }
        public int Rotations
        {
            get => (int)Math.Round(SpiralParameters.QuarterRotations / 4.0d);
            set
            {
                if (SpiralParameters.QuarterRotations != value * 4)
                {
                    PropertyValueChanged(this, value);
                    SpiralParameters.QuarterRotations = value * 4;
                    RaisePropertyChanged();
                }
            }
        }
        public double Shift
        {
            get => SpiralParameters.ShiftFactor;
            set
            {
                if (SpiralParameters.ShiftFactor != value)
                {
                    PropertyValueChanged(this, value);
                    SpiralParameters.ShiftFactor = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int Arms
        {
            get => SpiralParameters.NumberOfArms;
            set
            {
                if (SpiralParameters.NumberOfArms != value)
                {
                    PropertyValueChanged(this, value);
                    SpiralParameters.NumberOfArms = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int Groups
        {
            get => SpiralParameters.NumberOfGroups;
            set
            {
                if (SpiralParameters.NumberOfGroups != value)
                {
                    PropertyValueChanged(this, value);
                    SpiralParameters.NumberOfGroups = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int DistanceBetweenArms
        {
            get => SpiralParameters.NormalGroupDistance;
            set
            {
                if (SpiralParameters.NormalGroupDistance != value)
                {
                    PropertyValueChanged(this, value);
                    SpiralParameters.NormalGroupDistance = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool EndMode
        {
            get => SpiralParameters.CloseEnds;
            set
            {
                if (SpiralParameters.CloseEnds != value)
                {
                    PropertyValueChanged(this, value);
                    SpiralParameters.CloseEnds = value;
                    RaisePropertyChanged();
                }
            }
        }
        public CreateSpiralVM(SpiralParameters dn, bool? AllowRegenerate) : base(dn, AllowRegenerate)
        {
            Refresh();
            UnsavedChanges = false;
        }
    }
}
