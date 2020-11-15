using System.Collections.Generic;
using System.Windows;
using DominoPlanner.Core;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia;

namespace DominoPlanner.Usage
{
    public class EditingDominoVM : ModelBase
    {
        public int idx;
        public ColorRepository colorRepository;
        public IDominoShape domino;

        public Color StoneColor
        {
            get { return colorRepository[domino.color].mediaColor; }
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
        public bool isSelected
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
        public Core.Point[] canvasPoints
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
    }
}
