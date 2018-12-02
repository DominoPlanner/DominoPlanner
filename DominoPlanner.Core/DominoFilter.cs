using Emgu.CV;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Core
{ 
    [ProtoContract]
    //[ProtoInclude()]
    public abstract class ColorFilter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        public abstract void Apply(ColorRepository input);
        public void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
    [ProtoContract]
    public abstract class PostFilter
    {
        public abstract void Apply(DominoTransfer input);

    }
    [ProtoContract]
    [ProtoInclude(100, typeof(BlendImageFilter))]
    [ProtoInclude(101, typeof(ContrastLightFilter))]
    [ProtoInclude(102, typeof(GammaCorrectFilter))]
    [ProtoInclude(103, typeof(GaussianBlurFilter))]
    [ProtoInclude(104, typeof(ReplaceColorFilter))]
    public abstract class ImageFilter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        public abstract void Apply(Image<Emgu.CV.Structure.Bgra, byte> input);
        public void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        }
        internal byte Saturate(double input)
        {
            if (input < 0) return 0;
            if (input > 255) return 255;
            return (byte)input;
        }
    }
    [ProtoContract]
    [ProtoInclude(100, typeof(RemoveColorPreFilter))]
    [ProtoInclude(101, typeof(ChangeCountFilter))]
    public class RemoveColorPreFilter : ColorFilter
    {
        private List<DominoColor> _toRemove;
        [ProtoMember(1, AsReference =true)]
        public List<DominoColor> toRemove
        {
            get { return _toRemove; }
            set { _toRemove = value; }
        }
        public override void Apply(ColorRepository input)
        {
            foreach (DominoColor c in toRemove)
            {
                throw new NotImplementedException();
            }
        }
    }
    [ProtoContract]
    public class ChangeCountFilter : ColorFilter
    {

        private DominoColor _newCount;
        [ProtoMember(1, AsReference =true)]
        public DominoColor newCount
        {
            get { return _newCount; }
            set { _newCount = value; }
        }

        public override void Apply(ColorRepository input)
        {
            throw new NotImplementedException();
        }
    }
}