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
                int[] row_counts = Enumerable.Range(0, 3).Select(x => cells[0, x].Count + current_width * cells[1, x].Count + cells[2, x].Count).ToArray();
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
            int[] row_counts = Enumerable.Range(0, 2).Select(x => cells[0, x].Count + current_width * cells[1, x].Count + cells[2, x].Count).ToArray();
            int reihe = (index < row_counts[0]) ? -1 : 
                ((index < row_counts[0] + row_counts[1] * current_height) ? (index - row_counts[0]) / row_counts[1] : current_height);
            int steine_vor_reihe = (reihe == -1 ? 0 : row_counts[0] + reihe * row_counts[1]);
            int reihentyp = (reihe == -1 ? 0 : (reihe == current_height ? 2 : 1));
            int index_in_reihe = index - steine_vor_reihe;
            int spalte = (index_in_reihe < cells[0, reihentyp].Count) ? -1 :
                ((index_in_reihe < cells[0, reihentyp].Count + cells[1, reihentyp].Count * current_width) 
                    ? (index_in_reihe - cells[0, reihentyp].Count) / cells[1, reihentyp].Count : current_width);
            int steine_vor_zelle = (spalte == -1 ? 0 : cells[0, reihentyp].Count + spalte * cells[1, reihentyp].Count);
            int index_in_zelle = index_in_reihe - steine_vor_zelle;
            return new PositionWrapper() { X = spalte, Y = reihe, CountInsideCell = index_in_zelle };
        }
        private int getIndexFromPosition(int reihe, int spalte, int index_in_zelle)
        {
            int[] row_counts = Enumerable.Range(0, 2).Select(x => cells[0, x].Count + current_width * cells[1, x].Count + cells[2, x].Count).ToArray();
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
            int first_inserted_row = getPositionFromIndex(position).Y  + (below ? 1 : 0);
            IDominoShape[] new_shapes = new IDominoShape[count * getShapesInRow(0)];
            for (int x = -1; x <= current_width; x++)
            {
                for (int y = first_inserted_row; y < first_inserted_row + count; y++)
                {
                    // create new shapes with the appropriate coordinates and dimensions, assign specified color
                    var new_cells = (
                        cells[(x == -1) ? 0 : ((x == current_width) ? 2 : 1), 
                        (y == -1) ? 0 : ((y == current_height) ? 2 : 1)]
                        .TransformDefinition(
                            (x == -1) ? 0 : (cells[1, 1].width * (x) + cells[0, 0].width),
                            (y == -1) ? 0 : (cells[1, 1].height * (y) + cells[0, 0].height),
                            (x), (y), current_width, current_height + count))
                        .dominoes;
                    var index = getIndexFromPosition(y - first_inserted_row, x, 0) - getShapesInRow(-1);
                    for (int i = 0; i < new_cells.Length; i++)
                    {
                        new_cells[i].color = color;
                        new_shapes[i + index] = new_cells[i];
                    }
                }
            }
            return AddRow(new int[count].Select(x => getIndexFromPosition(first_inserted_row, 0, 0)).ToArray(), new_shapes);
        }
        public bool[] getDistinctRows(int[] position, out int[] counts)
        {
            bool[] rows = new bool[current_height + 1];
            counts = new int[current_height + 1];
            for (int i = 0; i < position.Length; i++)
            {
                var row = getPositionFromIndex(position[i]).Y;
                if (row == -1 || row > current_height) throw new InvalidOperationException();
                rows[row] = true;
                counts[row]++;
            }
            return rows;
        }
        public int getShapesInRow(int reihe)
        {
            int reihentyp = (reihe == -1 ? 0 : (reihe == current_height ? 2 : 1));
            return cells[0, reihentyp].Count + current_width * cells[1, reihentyp].Count + cells[2, reihentyp].Count;
        }
        public int[] AddRow(int[] position, IDominoShape[] shapes)
        {
            int[] counts;
            var to_add = getDistinctRows(position, out counts);
            var total_added_rows = counts.Sum();
            int top_row = getShapesInRow(-1);
            int bottom_row = getShapesInRow(current_height);
            int center_row = getShapesInRow(0);
            IDominoShape[] new_array = new IDominoShape[(current_height + total_added_rows) * center_row 
                + top_row + bottom_row];
            int[] positions = new int[total_added_rows];
            int added_counter = 0;
            int array_pos = 0;
            for (int y = -1; y <= current_height; y++)
            {
                int steine_in_reihe = getShapesInRow(y);
                // es wird jeweils vor der y. Reihe eingefügt
                if (y != -1 && to_add[y])
                {
                    for (int y2 = 0; y2 < counts[y]; y2++)
                    {
                        for (int x = 0; x < steine_in_reihe; x++)
                        {
                            new_array[array_pos + (added_counter + y2) * steine_in_reihe + x] 
                                = shapes[(added_counter + y2) * steine_in_reihe + x];
                        }
                        positions[added_counter + y2] = steine_in_reihe * (added_counter + y2) + array_pos;
                    }
                    // alle shapes von diesem bis zur nächsten gelöschten Zeile nach hinten schieben
                    int next_inserted = Array.IndexOf(to_add.Skip(y+1).ToArray(), true);
                    next_inserted = next_inserted == -1 ? current_height - y : next_inserted + 1;

                    ShiftDomainShapes(-1, y, current_width, y + next_inserted, 0, added_counter + counts[y], current_width, current_height + total_added_rows);
                    added_counter += counts[y];
                }
                if (true)
                {
                    for (int x = 0; x < steine_in_reihe; x++)
                    {
                        new_array[array_pos + center_row * added_counter + x] = last[array_pos + x];
                    }
                }
                array_pos += steine_in_reihe;
            }
            last.shapes = new_array;
            return positions;
        }

        public IDominoShape[] DeleteRow(int[] positions, out int[] remaining_positions)
        {
            int[] counts;
            var to_delete = getDistinctRows(positions, out counts);
            to_delete[current_height] = false;
            var total_deleted_rows = to_delete.Count(x => x);
            remaining_positions = new int[total_deleted_rows];
            if (total_deleted_rows == current_height) throw new InvalidOperationException("Can't delete all rows");
            int top_row = getShapesInRow(-1);
            int bottom_row = getShapesInRow(current_height);
            int center_row = getShapesInRow(0);
            IDominoShape[] new_array = new IDominoShape[(current_height - total_deleted_rows) * center_row
                + top_row + bottom_row];
            IDominoShape[] deleted = new IDominoShape[total_deleted_rows * center_row];
            int deleted_counter = 0;
            int position = 0;
            for (int y = -1; y <= current_height; y++)
            {
                int steine_in_reihe = getShapesInRow(y);
                if (y != -1 && y != current_height && to_delete[y])
                {
                    for (int x = 0; x < steine_in_reihe; x++)
                    {
                        deleted[steine_in_reihe * deleted_counter + x] = last[position + x];
                    }
                    // alle shapes von diesem bis zur nächsten gelöschten Zeile nach vorne ziehen
                    int next_deleted = Array.IndexOf(to_delete.Skip(y+1).ToArray(), true);
                    // wenn keine Zeile danach mehr gelöscht werden soll, alles bis zur letzen vorziehen
                    next_deleted = next_deleted == -1 ? current_height - y : next_deleted;
                    ShiftDomainShapes(-1, y+1, current_width, y + next_deleted, 0, -deleted_counter-1, current_width, current_height - total_deleted_rows);
                    remaining_positions[deleted_counter] = (y - deleted_counter) * steine_in_reihe + top_row;
                    deleted_counter++;
                }
                else
                {
                    for (int x = 0; x < steine_in_reihe; x++)
                    {
                        new_array[position - deleted_counter * center_row + x] = last[position + x];
                    }
                }
                position += steine_in_reihe;
            }
            last.shapes = new_array;
            return deleted;
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
        public void ShiftDomainShapes(int start_x, int start_y, int end_x, int end_y, int shift_x, int shift_y, int new_width, int new_height)
        {
            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_x; y <= end_y; y++)
                {
                    var new_cells = (cells[(x == -1) ? 0 : ((x == current_width) ? 2 : 1), (y == -1) ? 0 : ((y == current_height) ? 2 : 1)]
                        .TransformDefinition(
                            (x == -1) ? 0 : (cells[1, 1].width * (x + shift_x) + cells[0, 0].width),
                            (y == -1) ? 0 : (cells[1, 1].height * (y + shift_y) + cells[0, 0].height),
                            (x + shift_x), (y + shift_y), new_width, new_height))
                        .dominoes;
                    var index = getIndexFromPosition(y, x, 0);
                    for (int i = 0; i < new_cells.Length; i++)
                    {
                        new_cells[i].color = last[index + i].color;
                        new_cells[i].originalColor = last[index + i].originalColor;
                        last.shapes[index + i] = new_cells[i];
                    }
                }
            }
        }
    }
    struct PositionWrapper
    {
        internal int X { get; set; }
        internal int Y { get; set; }
        internal int CountInsideCell { get; set; }
    }
}
