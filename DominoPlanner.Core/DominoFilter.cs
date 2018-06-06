using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Core
{
    public abstract class PreFilter
    {
        public abstract void Apply(List<DominoColor> input);
    }
    public abstract class PostFilter
    {
        public abstract void Apply(DominoTransfer input);
    }
    public abstract class ImageFilter
    {
        public abstract void Apply(RenderTargetBitmap input);
    }
    public class RemoveColorPreFilter : PreFilter
    {
        private List<DominoColor> _toRemove;
        public List<DominoColor> toRemove
        {
            get { return _toRemove; }
            set { _toRemove = value; }
        }
        public override void Apply(List<DominoColor> input)
        {
            foreach (DominoColor c in toRemove)
            {
                input = (List<DominoColor>) input.Where(t => t.Equals(c));
            }
        }
    }
    public class ChangeCountFilter : PreFilter
    {
        private DominoColor _newCount;
        public DominoColor newCount
        {
            get { return _newCount; }
            set { _newCount = value; }
        }

        public override void Apply(List<DominoColor> input)
        {
            input = (List<DominoColor>)input.Select(x => (x.Equals(newCount)) ? newCount : x);
        }
    }
}