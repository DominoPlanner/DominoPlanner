using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    public partial class FieldParameters
    {
        int _current_width;
        public int current_width { get => _current_width; }
        public int current_height { get => last.length / _current_width; }
        public bool IsValidPastePosition(int source_position, int target_position)
        {
            return true;
        }
        public int[] GetValidPastePositions(int source_position)
        {
            return Enumerable.Range(0, last.length).ToArray();
        }
        public int[] PasteTarget(int reference, int[] source_domain, int target_reference)
        {
            int colshift = target_reference % current_width - reference % current_width;
            int rowshift = target_reference / current_width - reference / current_width;
            int[] target_domain = new int[source_domain.Length];
            for (int i = 0; i < source_domain.Length; i++)
            {
                //shapes[length * yi + xi]
                int row = source_domain[i] / current_width;
                int col = source_domain[i] % current_width;
                target_domain[i] = (row + rowshift) * (current_width) + (col + colshift);
            }
            return target_domain;
        }
        public int[] AddRow(int position, bool below, int color, int count)
        {
            int first_inserted_row = position / current_width + (below ? 1 : 0);
            IDominoShape[] new_shapes = new IDominoShape[count * current_width];
            for (int x = 0; x < current_width; x++)
            {
                for (int y = first_inserted_row; y < first_inserted_row + count; y++)
                {
                    // create new shapes with the appropriate coordinates and dimensions, assign specified color
                    new_shapes[current_width * (y - first_inserted_row) + x] = new RectangleDomino()
                    {
                        x = (b + a) * x,
                        y = (c + d) * y,
                        width = b,
                        height = c,
                        position = new ProtocolDefinition() { x = x, y = y },
                        color = color
                    };
                }
            }
            return AddRow(new int[count].Select(x => first_inserted_row * current_width).ToArray(), new_shapes);
        }
        public int[] AddRow(int[] position, IDominoShape[] shapes)
        {
            int[] counts;
            var to_add = getDistinctRows(position, out counts);
            var total_added_rows = counts.Sum();
            IDominoShape[] new_array = new IDominoShape[(current_height + total_added_rows) * current_width];
            int[] positions = new int[total_added_rows];
            int added_counter = 0;
            for (int y = 0; y <= current_height; y++)
            {
                // es wird jeweils vor der y. Reihe eingefügt
                if (to_add[y])
                {
                    for (int y2 = 0; y2 < counts[y]; y2++)
                    {
                        for (int x = 0; x < current_width; x++)
                        {
                            new_array[current_width * (y + added_counter + y2) + x] = shapes[(added_counter + y2) * current_width + x];
                        }
                        positions[added_counter + y2] = (y + added_counter + y2) * current_width;
                    }
                    // alle shapes von diesem bis zur nächsten gelöschten Zeile nach hinten schieben
                    int next_inserted = Array.IndexOf(to_add.Skip(y + 1).ToArray(), true);
                    next_inserted = next_inserted == -1 ? current_height - y : next_inserted + 1;
                    ShiftDomainShapes(0, y, current_width - 1, y + next_inserted - 1, 0, added_counter + counts[y]);
                    added_counter += counts[y];
                }
                if (y != current_height)
                {
                    for (int x = 0; x < current_width; x++)
                    {
                        new_array[current_width * (y + added_counter) + x] = last[current_width * y + x];
                    }
                }
            }
            
            last.shapes = new_array;
            return positions;
        }
        public bool[] getDistinctRows(int[] position, out int[] counts)
        {
            bool[] rows = new bool[current_height + 1];
            counts = new int[current_height + 1];
            for (int i = 0; i < position.Length; i++)
            {
                rows[position[i] / current_width] = true;
                counts[position[i] / current_width] += 1;
            }
            return rows;
        }
        public IDominoShape[] DeleteRow(int[] positions, out int[] remaining_positions)
        {
            int[] counts;
            var to_delete = getDistinctRows(positions, out counts);
            var total_deleted_rows = to_delete.Count(x => x);
            remaining_positions = new int[total_deleted_rows];
            if (total_deleted_rows == current_height) throw new InvalidOperationException("Can't delete all rows");
            IDominoShape[] new_array = new IDominoShape[(current_height - total_deleted_rows) * current_width];
            IDominoShape[] deleted = new IDominoShape[total_deleted_rows * current_width];
            int deleted_counter = 0;
            for (int y = 0; y < current_height; y++)
            {
                if (to_delete[y])
                {
                    for (int x = 0; x < current_width; x++)
                    {
                        deleted[current_width * deleted_counter + x] = last[current_width * y + x];
                    }
                    // alle shapes von diesem bis zur nächsten gelöschten Zeile nach vorne ziehen
                    int next_deleted = Array.IndexOf(to_delete.Skip(y + 1).ToArray(), true);
                    next_deleted = next_deleted == -1 ? current_height - y : next_deleted + 1;
                    ShiftDomainShapes(0, y+1, current_width - 1, y + next_deleted - 1, 0, -deleted_counter-1);
                    remaining_positions[deleted_counter] = (y - deleted_counter) * current_width;
                    deleted_counter++;
                }
                else
                {
                    for (int x = 0; x < current_width; x++)
                    {
                        new_array[current_width * (y - deleted_counter) + x] = last[current_width * y + x];
                    }
                }

            }
            last.shapes = new_array;
            return deleted;
        }
        public bool[] getDistinctColumns(int[] position, out int[] counts)
        {
            bool[] rows = new bool[current_width + 1];
            counts = new int[current_width + 1];
            for (int i = 0; i < position.Length; i++)
            {
                rows[position[i] % current_width] = true;
                counts[position[i] % current_width] += 1;
            }
            return rows;
        }
        public int[] AddColumn(int position, bool right, int color, int count)
        {
            int first_inserted_column = position % current_width + (right ? 1 : 0);
            IDominoShape[] new_shapes = new IDominoShape[count * current_height];
            for (int y = 0; y < current_height; y++)
            {
                for (int x = first_inserted_column; x < first_inserted_column + count; x++)
                {
                    // create new shapes with the appropriate coordinates and dimensions, assign specified color
                    new_shapes[y  + (x - first_inserted_column) * current_height] = new RectangleDomino()
                    {
                        x = (b + a) * x,
                        y = (c + d) * y,
                        width = b,
                        height = c,
                        position = new ProtocolDefinition() { x = x, y = y },
                        color = color
                    };
                }
            }
            return AddColumn(new int[count].Select(x => first_inserted_column).ToArray(), new_shapes);
        }

        public IDominoShape[] DeleteColumn(int[] positions, out int[] remaining_positions)
        {
            int[] counts;
            var to_delete = getDistinctColumns(positions, out counts);
            var total_deleted_cols = to_delete.Count(x => x);
            remaining_positions = new int[total_deleted_cols];
            if (total_deleted_cols == current_width) throw new InvalidOperationException("Can't delete all columns");
            IDominoShape[] new_array = new IDominoShape[current_height  * (current_width - total_deleted_cols)];
            IDominoShape[] deleted = new IDominoShape[total_deleted_cols * current_height];
            int deleted_counter = 0;
            for (int x = 0; x < current_width; x++)
            {
                if (to_delete[x])
                {
                    for (int y = 0; y < current_height; y++)
                    {
                        deleted[y + current_height * (deleted_counter)] = last[current_width * y + x];
                    }
                    // alle shapes von diesem bis zur nächsten gelöschten Zeile nach vorne ziehen
                    int next_deleted = Array.IndexOf(to_delete.Skip(x + 1).ToArray(), true);
                    next_deleted = next_deleted == -1 ? current_width - x : next_deleted + 1;
                    ShiftDomainShapes(x+1, 0, x + next_deleted - 1, current_height - 1, -deleted_counter - 1, 0);
                    remaining_positions[deleted_counter] = (x - deleted_counter);
                    deleted_counter++;
                }
                else
                {
                    for (int y = 0; y < current_height; y++)
                    {
                        new_array[(current_width - total_deleted_cols) * (y) + x - deleted_counter] = last[current_width * y + x];
                    }
                }

            }
            _current_width = current_width - total_deleted_cols;
            last.shapes = new_array;
            return deleted;
        }

        public int[] AddColumn(int[] position, IDominoShape[] shapes)
        {
            int[] counts;
            var to_add = getDistinctColumns(position, out counts);
            var total_added_columns = counts.Sum();
            IDominoShape[] new_array = new IDominoShape[current_height * (current_width + total_added_columns)];
            int[] positions = new int[total_added_columns];
            int added_counter = 0;
            for (int x = 0; x <= current_width; x++)
            {
                // es wird jeweils vor der y. Spalte eingefügt
                if (to_add[x])
                {
                    for (int x2 = 0; x2 < counts[x]; x2++)
                    {
                        for (int y = 0; y < current_height; y++)
                        {
                            new_array[(current_width + total_added_columns) * y  + x + added_counter + x2] = shapes[y +(added_counter + x2) * current_height];
                        }
                        positions[added_counter + x2] = x + added_counter + x2;
                    }
                    // alle shapes von diesem bis zur nächsten gelöschten Zeile nach hinten schieben
                    int next_inserted = Array.IndexOf(to_add.Skip(x + 1).ToArray(), true);
                    next_inserted = next_inserted == -1 ? current_width - x : next_inserted + 1;
                    ShiftDomainShapes(x, 0, x + next_inserted - 1, current_height - 1, added_counter + counts[x], 0);
                    added_counter += counts[x];
                }
                if (x != current_width)
                {
                    for (int y = 0; y < current_height; y++)
                    {
                        new_array[(current_width + total_added_columns) * y + x + added_counter] = last[current_width * y + x];
                    }
                }
            }
            _current_width = current_width + total_added_columns;
            last.shapes = new_array;
            return positions;
        }
        public void ShiftDomainShapes(int start_x, int start_y, int end_x, int end_y, int shift_x, int shift_y)
        {
            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    // move shape, set new field plan position (although not really necessary for fields)
                    last[current_width * y + x].position = new ProtocolDefinition() { x = x + shift_x, y = y + shift_y };
                    ((RectangleDomino)last[current_width * y + x]).x = (b + a) * (x + shift_x);
                    ((RectangleDomino)last[current_width * y + x]).y = (c + d) * (y + shift_y);
                }
            }
        }
    }
}
