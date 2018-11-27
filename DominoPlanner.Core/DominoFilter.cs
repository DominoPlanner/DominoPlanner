using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Core
{
    public abstract class ColorFilter
    {
        public abstract void Apply(ColorRepository input);
    }
    public abstract class PostFilter
    {
        public abstract void Apply(DominoTransfer input);
    }
    public abstract class ImageFilter
    {
        public abstract void Apply(Mat input);
    }
    public class RemoveColorPreFilter : ColorFilter
    {
        private List<DominoColor> _toRemove;
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
    public class ChangeCountFilter : ColorFilter
    {
        private DominoColor _newCount;
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