using System.Collections.Generic;
using System.Windows;
using DominoPlanner.Core;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia;
using System;

namespace DominoPlanner.Usage
{
    [Flags]
    public enum EditingDominoStates
    {
        Default = 0,
        Selected = 1,
        PasteHighlight = 2,
        DeletionHighlight = 4
    }
    public class EditingDominoVM : ModelBase, Core.RTree.IGeometry
    {
        public int idx;
        public ColorRepository colorRepository;
        public IDominoShape domino;

        public Color StoneColor
        {
            get { return colorRepository[domino.Color].mediaColor; }
        }

        private EditingDominoStates _state;
        public EditingDominoStates State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
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
