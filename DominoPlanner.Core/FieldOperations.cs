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
                target_domain[i] = (row + rowshift) + (current_width) * (col + colshift);
            }
            return target_domain;
        }

        public int[] AddRow(int position, bool below, int color, int count)
        {
            int first_inserted_row = position / current_width + (below ? 1 : 0);
            IDominoShape[] new_shapes = new IDominoShape[count * current_width * current_height];
            for (int x = 0; x < current_width; x++)
            {
                for (int y = first_inserted_row; y < first_inserted_row + height; y++)
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
            return AddRow(Enumerable.Range(first_inserted_row, count).Select(x => x * current_width).ToArray(), new_shapes);
        }
        public int[] AddRow(int[] position, IDominoShape[] shapes)
        {
            var to_add = getDistinctRows(position);
            var total_added_rows = to_add.Count(x => x);
            IDominoShape[] new_array = new IDominoShape[(current_height + total_added_rows) * current_height];
            int[] positions = new int[total_added_rows];
            int added_counter = 0;
            for (int y = 0; y < current_height; y++)
            {
                if (to_add[y])
                {
                    for (int x = 0; x < current_width; x++)
                    {
                        new_array[current_width * (y + added_counter) + x] = shapes[added_counter * current_width + x];
                    }
                    // alle shapes von diesem bis zur nächsten gelöschten Zeile nach vorne ziehen
                    int next_inserted = Array.IndexOf(to_add.Skip(y).ToArray(), true);
                    if (next_inserted != 0) // Operation wäre unnötig
                    {
                        ShiftDomainShapes(0, y, current_width, y + next_inserted, 0, added_counter);
                    }
                    positions[added_counter] = (y + added_counter) * current_width;
                    added_counter++;
                }
                else
                {
                    for (int x = 0; x < current_width; x++)
                    {
                        new_array[current_width * (y + added_counter) + x] = last[current_width * y + x];
                    }
                }
            }
            return positions.Select(x => x * current_width).ToArray();

        }
        public bool[] getDistinctRows(int[] position)
        {
            bool[] rows = new bool[current_height];
            for (int i = 0; i < position.Length; i++)
            {
                rows[position[i] / current_width] = true;
            }
            return rows;
        }
        public IDominoShape[] DeleteRow(int[] positions, out int[] remaining_positions)
        {
            var to_delete = getDistinctRows(positions);
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
                    int next_deleted = Array.IndexOf(to_delete.Skip(y).ToArray(), true);
                    if (next_deleted != 0) // Operation wäre unnötig
                    {
                        ShiftDomainShapes(0, y, current_width, y + next_deleted, 0, -deleted_counter);
                    }
                    remaining_positions[deleted_counter] = (y - deleted_counter - 1) * current_width;
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
            shapes = new_array;
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
        public void ShiftDomainShapes(int start_x, int start_y, int end_x, int end_y, int shift_x, int shift_y)
        {
            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_x; y <= end_x; y++)
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
