using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    [ProtoContract]
    [ProtoInclude(100, typeof(NoColorRestriction))]
    [ProtoInclude(101, typeof(IterativeColorRestriction))]
    public abstract class IterationInformation : INotifyPropertyChanged, ICloneable
    {
        public int numberofiterations;
        public double[] weights;
        public bool? colorRestrictionsFulfilled;
        [ProtoMember(1)]
        public virtual int maxNumberOfIterations { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnNotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public virtual void EvaluateSolution(IDominoColor[] palette, IDominoShape[] field)
        {

        }

        public object Clone()
        {
            IterationInformation res = (IterationInformation)this.MemberwiseClone();
            this.weights.CopyTo(res.weights, 0);
            return res;
        }
    }
    public class NoColorRestriction : IterationInformation
    {
        public NoColorRestriction()
        {
            maxNumberOfIterations = 1;
            colorRestrictionsFulfilled = null;
        }
        public override int maxNumberOfIterations { get => 1; set => base.maxNumberOfIterations = 1; }

    }
    public class IterativeColorRestriction : IterationInformation
    {

        private int _maxnumberofiterations;

        public override int maxNumberOfIterations
        {
            get
            {
                return _maxnumberofiterations;
            }
            set
            {
                _maxnumberofiterations = value;
                OnNotifyPropertyChanged("numberofiterations");
            }
        }

        private double _iterationWeight;
        [ProtoMember(1)]
        public double iterationWeight
        {
            get
            {
                return _iterationWeight;
            }
            set
            {
                _iterationWeight = value;
                OnNotifyPropertyChanged("iterationWeight");
            }
        }
        public IterativeColorRestriction(int nit, double iterationWeight)
        {
            maxNumberOfIterations = nit;
            this.iterationWeight = iterationWeight;
        }
        public override void EvaluateSolution(IDominoColor[] palette, IDominoShape[] field)
        {
            int[] counts = new int[palette.Length];
            for (int j = field.Length - 1; j >= 0; j--)
            {
                counts[field[j].color]++;
            }
            this.colorRestrictionsFulfilled = true;
            for (int j = 0; j < counts.Length; j++)
            {
                if (counts[j] > palette[j].count) colorRestrictionsFulfilled = false;
                weights[j] = weights[j] * (1 + Math.Max(0.0, 1.0 * (counts[j] - palette[j].count) / palette[j].count * iterationWeight));
                Console.WriteLine($"Farbe: {palette[j].name}, vorhanden: {palette[j].count}, verwendet: {counts[j]}, neues Gewicht: {weights[j]}");
            }
        }
    }
}
