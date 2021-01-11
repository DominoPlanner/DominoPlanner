using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    public class ReplaceColorOperation : PostFilter
    {
        IDominoProvider reference;
        int[] domain;
        int to_replace;
        int new_color;
        int[] old_colors;
        public override void Apply()
        {
            for (int i = 0; i < domain.Length; i++)
            {
                int color = reference.Last[domain[i]].Color;
                if (color == to_replace)
                {
                    old_colors[i] = color;
                    reference.Last[domain[i]].Color = new_color;
                }
            }
        }
        public override void Undo()
        {
            for (int i = 0; i < domain.Length; i++)
            {
                reference.Last[domain[i]].Color = old_colors[i];
            }
        }
        public ReplaceColorOperation(IDominoProvider reference, int[] domain, int toReplace, int newColor)
        {
            this.reference = reference;
            this.domain = domain;
            this.to_replace = toReplace;
            this.new_color = newColor;
            old_colors = new int[domain.Length];
        }
    }
    public class SetColorOperation : PostFilter
    {
        IDominoProvider reference;
        int[] domain;
        int new_color;
        int[] old_colors;
        public override void Apply()
        {
            for (int i = 0; i < domain.Length; i++)
            {

                old_colors[i] = reference.Last[domain[i]].Color;
                reference.Last[domain[i]].Color = new_color;
            }
        }
        public override void Undo()
        {
            for (int i = 0; i < domain.Length; i++)
            {
                reference.Last[domain[i]].Color = old_colors[i];
            }
        }
        public SetColorOperation(IDominoProvider reference, int[] domain, int newColor)
        {
            this.reference = reference;
            this.domain = domain;
            this.new_color = newColor;
            old_colors = new int[domain.Length];
        }
    }
    public interface IRowColumnAddableDeletable
    {
        void ResetSize();
        int current_width { get; set;  }
        int current_height { get; set; }

        PositionWrapper getPositionFromIndex(int v);
        PositionWrapper getPositionFromIndex(int v, int new_width, int new_height);

        int getIndexFromPosition(PositionWrapper wrapper);

        int getIndexFromPosition(int row, int col, int index, bool swap_coords = false);

        int getIndexFromPosition(int row, int col, int index, int new_width, int new_height, bool swap_coords = false);

        void copyGroup(IDominoShape[] target, int source_x, int source_y, int target_x, int target_y, int target_width, int target_height);
        IDominoShape[] getNewShapes(int current_width, int current_height);
        int[] ExtractRowColumn(bool column, int index);
        // kopiert nur die Farben
        void ReinsertRowColumn(int[] rowcolumn, bool column, int index, IDominoShape[] target, int target_width, int target_height);

        int getLengthOfTypicalRowColumn(bool column);

        (int startindex, int endindex) getIndicesOfCell(int row, int col, int target_width, int target_height);
    }
    public struct PositionWrapper
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int CountInsideCell { get; set; }
    }
    public abstract class AddRowColumn : PostFilter
    {
        protected IRowColumnAddableDeletable reference;
        public int[] added_indizes;
        protected int position;
        protected int color;
        protected int count;
        public AddRowColumn(IRowColumnAddableDeletable reference, int position, int count, int color)
        {
            this.reference = reference;
            this.position = position;
            this.count = count;
            this.color = color;
        }
    }
    public class AddRows : AddRowColumn
    {
        bool below;
        public AddRows(IRowColumnAddableDeletable reference, int position, int count, int color, bool below) : base(reference, position, count, color)
        {
            this.below = below;
        }
        public override void Apply()
        {
            // get an array 
            ((IDominoProvider) reference).Last.shapes = 
                AddDeleteHelper.AddRowColumn(reference, false, position, below, count, color, out added_indizes);
        }
        public override void Undo()
        {
            ((IDominoProvider)reference).Last.shapes =
                AddDeleteHelper.DeleteRowColumn(reference, false, AddDeleteHelper.IndicesToPositions(reference, added_indizes), out _, out _);
        }
    }
    public class DeleteRows : PostFilter
    {
        IRowColumnAddableDeletable reference;
        int[] positions;
        PositionWrapper[] remaining_positions;
        int[][] oldShapes;
        public DeleteRows(IRowColumnAddableDeletable reference, int[] positions)
        {
            this.reference = reference;
            this.positions = positions;
        }
        public override void Apply()
        {
            ((IDominoProvider)reference).Last.shapes =
                AddDeleteHelper.DeleteRowColumn(reference, false, AddDeleteHelper.IndicesToPositions(reference, positions), 
                out remaining_positions, out oldShapes);
        }
        public override void Undo()
        {
            ((IDominoProvider)reference).Last.shapes =
                AddDeleteHelper.AddRowColumn(reference, false, remaining_positions, oldShapes, out _);
        }
    }
    public class AddColumns : AddRowColumn
    {
        bool right;
        public AddColumns(IRowColumnAddableDeletable reference, int position, int count, int color, bool right) : base(reference, position, count, color)
        {
            this.right = right;
        }
        public override void Apply()
        {
            // get an array 
            ((IDominoProvider)reference).Last.shapes =
                AddDeleteHelper.AddRowColumn(reference, true, position, right, count, color, out added_indizes);
        }
        public override void Undo()
        {
            ((IDominoProvider)reference).Last.shapes =
                AddDeleteHelper.DeleteRowColumn(reference, true, AddDeleteHelper.IndicesToPositions(reference, added_indizes), out _, out _);
        }
    }
    public class DeleteColumns : PostFilter
    {
        IRowColumnAddableDeletable reference;
        int[] positions;
        PositionWrapper[] remaining_positions;
        int[][] oldShapes;
        public DeleteColumns(IRowColumnAddableDeletable reference, int[] position)
        {
            if (reference.current_height == 0) throw new InvalidOperationException();
            this.reference = reference;
            this.positions = position;
        }
        public override void Apply()
        {
            ((IDominoProvider)reference).Last.shapes =
                AddDeleteHelper.DeleteRowColumn(reference, true, AddDeleteHelper.IndicesToPositions(reference, positions), out remaining_positions, out oldShapes);
        }
        public override void Undo()
        {
            ((IDominoProvider)reference).Last.shapes =
                AddDeleteHelper.AddRowColumn(reference, true, remaining_positions, oldShapes, out _);
        }
    }
    public static class AddDeleteHelper
    {
        public static PositionWrapper[] IndicesToPositions(IRowColumnAddableDeletable reference, int[] positions)
        {
            PositionWrapper[] wrapper = new PositionWrapper[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                wrapper[i] = reference.getPositionFromIndex(positions[i]);
            }
            return wrapper;
        }
        public static int[] getDistinct(bool column, IRowColumnAddableDeletable reference, PositionWrapper[] positions)
        {
            int max_index = column ? reference.current_width : reference.current_height; 
            int[] counts = new int[max_index + 1];
            for (int i = 0; i < positions.Length; i++)
            {
                var current = (column ? positions[i].X : positions[i].Y);
                if (current < 0 || current > max_index) continue;
                counts[current]++;
            }
            return counts;
        }
        public static IDominoShape[] DeleteRowColumn(IRowColumnAddableDeletable reference, bool column, PositionWrapper[] positions, 
            out PositionWrapper[] remaining_positions, out int[][] deleted_shapes)
        {
            int[] distinct = getDistinct(column, reference, positions);
            distinct[distinct.Length - 1] = 0;
            int total_deleted = distinct.Sum(x => x > 0 ? 1 : 0);
            if (total_deleted == 0)
                throw new InvalidOperationException("Nothing to delete. Borders can't be deleted.");
            int new_width = reference.current_width - (column ? total_deleted : 0);
            int new_height = reference.current_height - (!column ? total_deleted : 0);
            IDominoShape[] new_shapes = reference.getNewShapes(new_width, new_height);
            deleted_shapes = new int[total_deleted][];
            remaining_positions = new PositionWrapper[total_deleted];
            int deleted_counter = 0;
            int rowcollength = deleted_shapes.Length / total_deleted;
            for (int i = -1; i < distinct.Length; i++)
            {
                if (i != -1 && i != distinct.Length - 1 && distinct[i] > 0)
                {

                    deleted_shapes[deleted_counter] = reference.ExtractRowColumn(column, i);
                    remaining_positions[deleted_counter] = new PositionWrapper() {
                    X = column ? (i - deleted_counter) : 0,
                    Y = column ? 0 : (i - deleted_counter),
                    CountInsideCell = 0};
                    deleted_counter++;
                }
                else
                {
                    copyColors(reference, new_shapes, -1, column ? reference.current_height : reference.current_width, i, i,
                         0, -deleted_counter, new_width, new_height, column);
                }
            }
            reference.current_width = new_width;
            reference.current_height = new_height;
            return new_shapes;
        }
        public static IDominoShape[] AddRowColumn(IRowColumnAddableDeletable reference, bool column, 
            PositionWrapper[] positions, int[][] shapes, out int[] inserted_positions)
        {
            int total_added = positions.Length;
            int new_width = reference.current_width + (column ? total_added : 0);
            int new_height = reference.current_height + (!column ? total_added : 0);
            int[] distinct = getDistinct(column, reference, positions);
            total_added = distinct.Sum();
            IDominoShape[] new_shapes = reference.getNewShapes(new_width, new_height);
            int typicallength = reference.getLengthOfTypicalRowColumn(column);
            List<int> insertedPositions = new List<int>();
            int added_counter = 0;
            for (int i = -1; i < distinct.Length; i++)
            {
                if (i != -1 && distinct[i] > 0)
                {
                    for (int i2 = 0; i2 < distinct[i]; i2++)
                    {
                        reference.ReinsertRowColumn(shapes[added_counter + i2], column, 
                            i + added_counter + i2, new_shapes, new_width, new_height);
                        insertedPositions.AddRange(getAllIndicesInRowColumn(reference, i + i2 + added_counter, column, new_shapes.Length, new_width, new_height));
                    }
                    added_counter+=distinct[i];
                }
                copyColors(reference, new_shapes, -1, column ? reference.current_height : reference.current_width, i, i,
                     0, added_counter, new_width, new_height, column);
            }
            reference.current_width = new_width;
            reference.current_height = new_height;
            inserted_positions = insertedPositions.ToArray();
            return new_shapes;
        }
        public static IDominoShape[] AddRowColumn(IRowColumnAddableDeletable reference,
            bool column, int position, bool southeast, int count, int color, out int[] inserted_positions)
        {
            int[][] shapes = new int[count][];
            for (int i = 0; i < count; i++)
            {
                shapes[i] = new int[reference.getLengthOfTypicalRowColumn(column)];
                for (int j = 0; j < shapes[i].Length; j++)
                {
                    shapes[i][j] = color;
                }
            }
            var pos = reference.getPositionFromIndex(position);
            var res_y = southeast ? (pos.Y + (!column ? 1 : 0)) : pos.Y;
            var res_x = southeast ? (pos.X + (column ? 1 : 0)) : pos.X;
            if (!column && (res_y == -1 || res_y == reference.current_height + (column ? 0 : count))
                || column && (res_x == -1 || res_x == reference.current_width + (column ? count : 0)))
                throw new InvalidOperationException("Can't insert row/column here, borders must remain borders");
            return AddRowColumn(reference, column, Enumerable.Repeat(new PositionWrapper() { X = res_x, Y = res_y, CountInsideCell = 0}, 
                count).ToArray(), shapes, out inserted_positions);
        }
            public static void copyColors(IRowColumnAddableDeletable reference, IDominoShape[] target, 
            int startX, int endX, int startY, int endY, int shiftX, int shiftY, int target_width, int target_height, bool swapCoords = false)
        {

            if (swapCoords)
            {
                int temp;
                temp = startX; startX = startY; startY = temp;
                temp = endX; endX = endY; endY = temp;
                temp = shiftX; shiftX= shiftY; shiftY = temp;
            }
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    reference.copyGroup(target, x, y, x + shiftX, y + shiftY, target_width, target_height);
                }
            }
        }
        public static int[] getAllIndicesInRowColumn(IRowColumnAddableDeletable reference ,int rowColumn,  bool column, int maxindex, int current_width, int current_height)
        {
            int[] result = new int[reference.getLengthOfTypicalRowColumn(column)];
            int position = 0;
            for (int i = -1; i <= (column ? reference.current_height : reference.current_width); i++)
            {
                var (a, b) = reference.getIndicesOfCell(column ?  i : rowColumn, column ? rowColumn : i, current_width, current_height);
                for (int j = a; j <= b; j++)
                {
                    if (j >= 0 && j < maxindex && position < result.Length )
                    {
                        result[position] = j;
                        position++;
                    }
                    else
                    {

                    }
                }
            }
            return result;
        }
        public static List<InsertionHelper> GetInsertionPositions(IRowColumnAddableDeletable reference, bool column, bool expanded = false)
        {
            if (reference is IDominoProvider p)
            {
                List<InsertionHelper> result = new List<InsertionHelper>();
                int[] current_column;
                int[] last_column = null;
                var dim = column ? reference.current_width : reference.current_height;
                for (int i = 0; i <= dim; i++)
                {
                    double lastend = 0;
                    if (i != 0)
                    {
                        lastend = column ? last_column.Max(x => p.Last[x].GetContainer(expanded: expanded).x2) : last_column.Max(x => p.Last[x].GetContainer(expanded: expanded).y2);
                    }
                    if (i < dim)
                    {
                        current_column = getAllIndicesInRowColumn(reference, i, column, p.Last.Length, reference.current_width, reference.current_height);
                        var curstart = column ? current_column.Min(x => p.Last[x].GetContainer(expanded: expanded).x) : current_column.Min(x => p.Last[x].GetContainer(expanded: expanded).y);
                        if (i == 0)
                        {
                            result.Add(new InsertionHelper() { DrawPosition = curstart, Before = true, Index = current_column[0] });
                        }
                        else if (i != dim && last_column != null)
                        {
                            result.Add(new InsertionHelper()
                            {
                                DrawPosition = 0.5d * (lastend + curstart),
                                Before = true,
                                Index = current_column[0]
                            });
                        }
                        last_column = current_column;
                    }
                    else
                    {
                        result.Add(new InsertionHelper() { DrawPosition = lastend, Before = false, Index = last_column[0] });
                    }
                    
                }
                return result;
            }
            throw new ArgumentException("Needs an IDominoProvider as argument");
        }
    }
    public struct InsertionHelper
    {
        public double DrawPosition;
        public int Index;
        public bool Before;
    }
    public interface ICopyPasteable
    {
        bool IsValidPastePosition(int source_position, int target_position);

        int[] GetValidPastePositions(int source_position);

        int[] PasteTarget(int reference, int[] source_domain, int target_reference);
    }
    public class PasteFilter : PostFilter
    {
        ICopyPasteable reference;
        int position_source;
        int position_target;
        int[] paste_source;
        public int[] paste_target;
        int[] original_colors;
        public PasteFilter(ICopyPasteable reference, int position_source, int[] source_domain, int position_target)
        {
            this.reference = reference;
            this.position_source = position_source;
            this.paste_source = source_domain;
            this.position_target = position_target;
            if (!reference.IsValidPastePosition(position_source, position_target)) throw new InvalidOperationException("Can't paste here");
            original_colors = new int[source_domain.Length];
        }
        public override void Apply()
        {
            paste_target = reference.PasteTarget(position_source, paste_source, position_target);
            int[] paste_colors = new int[paste_source.Length];
            var field = (IDominoProvider)reference;
            // Quellfarben müssen separat ausgelesen werden
            for (int i = 0; i < paste_target.Length; i++)
            {
                paste_colors[i] = field.Last[paste_source[i]].Color;
            }
            for (int i = 0; i < paste_target.Length; i++)
            {
                if (paste_target[i] < field.Last.Length)
                {
                    original_colors[i] = field.Last[paste_target[i]].Color;
                    field.Last[paste_target[i]].Color = paste_colors[i];
                }
            }
        }
        public override void Undo()
        {
            var field = (IDominoProvider)reference;
            for (int i = 0; i < paste_target.Length; i++)
            {
                if (paste_target[i] < field.Last.Length)
                {
                    field.Last[paste_target[i]].Color = original_colors[i];
                }
            }
        }
    }
}
