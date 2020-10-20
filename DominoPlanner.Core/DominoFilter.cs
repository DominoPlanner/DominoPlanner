using Emgu.CV;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace DominoPlanner.Core
{ 
    [ProtoContract]
    [ProtoInclude(100, typeof(ChangeCountColorFilter))]
    [ProtoInclude(101, typeof(ChangeRGBColorFilter))]
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
        public abstract void Apply();

        public abstract void Undo();

    }
    [ProtoContract]
    [ProtoInclude(100, typeof(BlendImageFilter))]
    [ProtoInclude(101, typeof(ContrastLightFilter))]
    [ProtoInclude(102, typeof(GammaCorrectFilter))]
    [ProtoInclude(103, typeof(GaussianBlurFilter))]
    [ProtoInclude(104, typeof(ReplaceColorFilter))]
    public abstract class ImageFilter : INotifyPropertyChanged
    {
        public IDominoProvider parent;
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
    public class ChangeCountColorFilter : ColorFilter
    {
        private int _index;
        [ProtoMember(1)]
        public int Index { get => _index; set { SetField(ref _index, value); } }
        private int _newcount;
        [ProtoMember(2)]
        public int NewCount { get => _newcount; set { SetField(ref _newcount, value); } }
        public override void Apply(ColorRepository input)
        {
            input.RepresentionForCalculation[Index].count = NewCount;
        }
    }
    [ProtoContract]
    public class ChangeRGBColorFilter : ColorFilter
    {
        private int _index;
        [ProtoMember(1)]
        public int Index { get => _index; set { SetField(ref _index, value); } }
        private Color _color;
        public Color Color { get => _color; set { SetField(ref _color, value); } }
        [ProtoMember(2)]
        private String ColorSerialized
        {
            get { return Color.ToString(); }
            set { Color = Color.Parse(value); }
        }
        public override void Apply(ColorRepository input)
        {
            input.RepresentionForCalculation[Index].mediaColor = Color;
        }
    }
    
}