using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    partial class StructureParameters : IRowColumnAddableDeletable, ICopyPasteable
    {
        private int _current_width;
        public int current_width => _current_width;

        public int current_height
        {
            get
            {
                int[] row_counts = Enumerable.Range(0, 2).Select(x => cells[0, x].Count + cells[1, x].Count + cells[2, x].Count).ToArray();
                return (last.length - row_counts[0] - row_counts[2]) / row_counts[1];
            }
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public bool IsValidPastePosition(int source_position, int target_position)
        {
            var source =getPositionFromIndex(source_position);
            var target = getPositionFromIndex(target_position);
            if (source.CountInsideCell == target.CountInsideCell)
            {
                if (source.X != -1 && source.Y != -1 && source.X != current_width && source.Y != current_height)
                {
                    if (target.X != -1 && target.Y != -1 && target.X != current_width && target.Y != current_height)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int[] GetValidPastePositions(int source_position)
        {
            var positions = new int[current_width * current_height];
            var source = getPositionFromIndex(source_position);
            if (!(source.X != -1 && source.Y != -1 && source.X != current_width && source.Y != current_height))
            {
                throw new InvalidOperationException("Can't copy from here");
            }
            for (int x = 0; x < current_width; x++)
            {
                for (int y = 0; y < current_height; y++)
                {
                    positions[x * current_height + y] = getIndexFromPosition(y, x, source.CountInsideCell);
                }
            }
            return positions;
        }

        public int[] PasteTarget(int reference, int[] source_domain, int target_reference)
        {
            int[] target_domain = new int[source_domain.Length];
            if (!IsValidPastePosition(reference, target_reference)) throw new InvalidOperationException("Can't paste here");
            var source = getPositionFromIndex(reference);
            var target = getPositionFromIndex(target_reference);
            var rowshift = target.Y - source.Y;
            var colshift = target.X - source.X;
            for (int i = 0; i < source_domain.Length; i++)
            {
                var current = getPositionFromIndex(source_domain[i]);
                var target_index = getIndexFromPosition(current.Y + rowshift, current.X + colshift, current.CountInsideCell);
                // nur im "mittleren" Bereich darf gepastet werden
                target_domain[i] = current.Y + rowshift < current_height && current.X + colshift < current_width ? target_index : last.length;
            }
            return target_domain;
        }
        private PositionWrapper getPositionFromIndex(int index)
        {
            int[] row_counts = Enumerable.Range(0, 2).Select(x => cells[0, x].Count + cells[1, x].Count + cells[2, x].Count).ToArray();
            int reihe = (index < row_counts[0]) ? -1 : 
                ((index < row_counts[0] + row_counts[1] * current_height) ? (index - row_counts[0]) / row_counts[1] : current_height);
            int steine_vor_reihe = (reihe == -1 ? 0 : row_counts[0] + reihe * row_counts[1]);
            int reihentyp = (reihe == -1 ? 0 : (reihe == current_height ? 2 : 1));
            int index_in_reihe = reihe - steine_vor_reihe;
            int spalte = (index < cells[0, reihentyp].Count) ? -1 :
                ((index_in_reihe < cells[0, reihentyp].Count + cells[1, reihentyp].Count * current_width) 
                    ? (index_in_reihe - cells[0, reihentyp].Count) / cells[1, reihentyp].Count : current_width);
            int steine_vor_zelle = (spalte == -1 ? 0 : cells[0, reihentyp].Count + spalte * cells[1, reihentyp].Count);
            int index_in_zelle = index_in_reihe - steine_vor_zelle;
            return new PositionWrapper() { X = spalte, Y = reihe, CountInsideCell = index_in_zelle };
        }
        private int getIndexFromPosition(int reihe, int spalte, int index_in_zelle)
        {
            int[] row_counts = Enumerable.Range(0, 2).Select(x => cells[0, x].Count + cells[1, x].Count + cells[2, x].Count).ToArray();
            int summe_reihen_davor = (reihe == -1 ? 0 : row_counts[1] * reihe + row_counts[0]);
            int reihentyp = (reihe == -1 ? 0 : (reihe == current_height ? 2 : 1));
            int summe_spalten_davor = (spalte == -1 ? 0 : cells[0, reihentyp].Count + spalte * cells[1, reihentyp].Count);
            return summe_reihen_davor + summe_spalten_davor + index_in_zelle;
        }
        private int getIndexFromPosition(PositionWrapper wrapper)
        {
            return getIndexFromPosition(wrapper.Y, wrapper.X, wrapper.CountInsideCell);
        }
        public int[] AddRow(int position, bool below, int color, int count)
        {
            throw new NotImplementedException();
        }

        public int[] AddRow(int[] position, IDominoShape[] shapes)
        {
            throw new NotImplementedException();
        }

        public IDominoShape[] DeleteRow(int[] positions, out int[] remaining_positions)
        {
            throw new NotImplementedException();
        }

        public int[] AddColumn(int position, bool right, int color, int count)
        {
            throw new NotImplementedException();
        }

        public IDominoShape[] DeleteColumn(int[] positions, out int[] remaining_positions)
        {
            throw new NotImplementedException();
        }

        public int[] AddColumn(int[] position, IDominoShape[] shapes)
        {
            throw new NotImplementedException();
        }
    }
    struct PositionWrapper
    {
        internal int X { get; set; }
        internal int Y { get; set; }
        internal int CountInsideCell { get; set; }
    }
}
