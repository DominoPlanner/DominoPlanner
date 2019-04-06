using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Usage.UserControls.ViewModel
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
                    CircularStructure.DominoWidth = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    CircularStructure.DominoLength = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    CircularStructure.TangentialDistance = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    CircularStructure.NormalDistance = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    CircleParameters.Circles = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    CircleParameters.ForceDivisibility = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    CircleParameters.AngleShiftFactor = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    AngleShiftFactor = value ? null : (double?)0.05;
                    RaisePropertyChanged();
                    Refresh();
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
                    SpiralParameters.QuarterRotations = value * 4;
                    RaisePropertyChanged();
                    Refresh();
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
                    SpiralParameters.ShiftFactor = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    SpiralParameters.NumberOfArms = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    SpiralParameters.NumberOfGroups = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    SpiralParameters.NormalGroupDistance = value;
                    RaisePropertyChanged();
                    Refresh();
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
                    SpiralParameters.CloseEnds = value;
                    RaisePropertyChanged();
                    Refresh();
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
