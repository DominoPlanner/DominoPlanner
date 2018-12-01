using Emgu.CV;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Core
{ 
    [ProtoContract]
    //[ProtoInclude()]
    public abstract class ColorFilter
    {
        public abstract void Apply(ColorRepository input);
    }
    [ProtoContract]
    public abstract class PostFilter
    {
        public abstract void Apply(DominoTransfer input);
    }
    [ProtoContract]
    [ProtoInclude(100, typeof(RemoveColorPreFilter))]
    [ProtoInclude(101, typeof(ChangeCountFilter))]
    public abstract class ImageFilter
    {
        public abstract void Apply(Mat input);
    }
    [ProtoContract]
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