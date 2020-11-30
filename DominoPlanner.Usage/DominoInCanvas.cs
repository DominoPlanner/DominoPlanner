using System.Collections.Generic;
using System.Windows;
using DominoPlanner.Core;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia;

namespace DominoPlanner.Usage
{
    public class EditingDominoVM : ModelBase, Core.RTree.IGeometry
    {
        public int idx;
        public ColorRepository colorRepository;
        public IDominoShape domino;

        public Color StoneColor
        {
            get { return colorRepository[domino.Color].mediaColor; }
        }

        private bool _PossibleToPaste;

        public bool PossibleToPaste
        {
            get { return _PossibleToPaste; }
            set
            {
                if(_PossibleToPaste != value)
                {
                    _PossibleToPaste = value;
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                }
            }
        }
        public bool expanded;
        public Core.Point[] CanvasPoints
        {
            get
            {
                return domino.GetPath(expanded: expanded).points;
            }
        }

        public EditingDominoVM(int idx, IDominoShape domino, ColorRepository colorlist, bool expanded = false)
        {
            colorRepository = colorlist;
            this.idx = idx;
            this.domino = domino;
            this.expanded = expanded;
        }

        public bool Intersects(DominoRectangle rect)
        {
            return domino.Intersects(rect);
        }

        public DominoRectangle GetBoundingRectangle()
        {
            return domino.GetBoundingRectangle();
        }
    }
}
