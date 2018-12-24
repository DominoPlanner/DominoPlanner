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
                int color = reference.last[domain[i]].color;
                if (color == to_replace)
                {
                    old_colors[i] = color;
                    reference.last[domain[i]].color = new_color;
                }
            }
        }
        public override void Undo()
        {
            for (int i = 0; i < domain.Length; i++)
            {
                reference.last[domain[i]].color = old_colors[i];
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

                old_colors[i] = reference.last[domain[i]].color;
                reference.last[domain[i]].color = new_color;
            }
        }
        public override void Undo()
        {
            for (int i = 0; i < domain.Length; i++)
            {
                reference.last[domain[i]].color = old_colors[i];
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
        int current_width { get; set;  }
        int current_height { get; set; }

        PositionWrapper getPositionFromIndex(int v);

        int getIndexFromPosition(PositionWrapper wrapper);

        int getIndexFromPosition(int row, int col, int index, bool swap_coords = false);

        int getIndexFromPosition(int row, int col, int index, int new_width, int new_height, bool swap_coords = false);

        void copyGroup(IDominoShape[] target, int source_x, int source_y, int target_x, int target_y, int target_width, int target_height);
        IDominoShape[] getNewShapes(int current_width, int current_height);
        int[] ExtractRowColumn(bool column, int index);
        // kopiert nur die Farben
        void ReinsertRowColumn(int[] rowcolumn, bool column, int index, IDominoShape[] target, int target_width, int target_height);

        int getLengthOfTypicalRowColumn(bool column);
    }
    public struct PositionWrapper
    {
        internal int X { get; set; }
        internal int Y { get; set; }
        internal int CountInsideCell { get; set; }
    }
    public class AddRows : PostFilter
    {
        IRowColumnAddableDeletable reference;
        int[] added_indizes;
        int position;
        int color;
        int count;
        bool below;
        public AddRows(IRowColumnAddableDeletable reference, int position, int count, int color, bool below)
        {
            this.reference = reference;
            this.position = position;
            this.count = count;
            this.color = color;
            this.below = below;
        }
        public override void Apply()
        {
            // get an array 
            ((IDominoProvider) reference).last.shapes = 
                AddDeleteHelper.AddRowColumn(reference, false, position, below, count, color, out added_indizes);
        }
        public override void Undo()
        {
            ((IDominoProvider)reference).last.shapes =
                AddDeleteHelper.DeleteRowColumn(reference, false, added_indizes, out _, out _);
        }
    }
    public class DeleteRows : PostFilter
    {
        IRowColumnAddableDeletable reference;
        int[] positions;
        int[] remaining_positions;
        int[][] oldShapes;
        public DeleteRows(IRowColumnAddableDeletable reference, int[] positions)
        {
            this.reference = reference;
            this.positions = positions;
        }
        public override void Apply()
        {
            ((IDominoProvider)reference).last.shapes =
                AddDeleteHelper.DeleteRowColumn(reference, false, positions, out remaining_positions, out oldShapes);
        }
        public override void Undo()
        {
            ((IDominoProvider)reference).last.shapes =
                AddDeleteHelper.AddRowColumn(reference, false, remaining_positions, oldShapes, out _);
        }
    }
    public class AddColumns : PostFilter
    {
        IRowColumnAddableDeletable reference;
        int[] added_indizes;
        int position;
        int color;
        int count;
        bool right;
        public AddColumns(IRowColumnAddableDeletable reference, int position, int count, int color, bool right)
        {
            this.reference = reference;
            this.position = position;
            this.count = count;
            this.color = color;
            this.right = right;
        }
        public override void Apply()
        {
            // get an array 
            ((IDominoProvider)reference).last.shapes =
                AddDeleteHelper.AddRowColumn(reference, true, position, right, count, color, out added_indizes);
        }
        public override void Undo()
        {
            ((IDominoProvider)reference).last.shapes =
                AddDeleteHelper.DeleteRowColumn(reference, true, added_indizes, out _, out _);
        }
    }
    public class DeleteColumns : PostFilter
    {
        IRowColumnAddableDeletable reference;
        int[] positions;
        int[] remaining_positions;
        int[][] oldShapes;
        public DeleteColumns(IRowColumnAddableDeletable reference, int[] position)
        {
            if (reference.current_height == 0) throw new InvalidOperationException();
            this.reference = reference;
            this.positions = position;
        }
        public override void Apply()
        {
            ((IDominoProvider)reference).last.shapes =
                AddDeleteHelper.DeleteRowColumn(reference, true, positions, out remaining_positions, out oldShapes);
        }
        public override void Undo()
        {
            ((IDominoProvider)reference).last.shapes =
                AddDeleteHelper.AddRowColumn(reference, true, remaining_positions, oldShapes, out _);
        }
    }
    public static class AddDeleteHelper
    {
        
        public static int[] getDistict(bool column, IRowColumnAddableDeletable reference, int[] positions)
        {
            int max_index = column ? reference.current_width : reference.current_height; 
            int[] counts = new int[max_index + 1];
            for (int i = 0; i < positions.Length; i++)
            {
                var pos = reference.getPositionFromIndex(positions[i]);
                var current = (column ? pos.X : pos.Y);
                if (current < 0 || current > max_index) throw new InvalidOperationException("invalid position for insert");
                counts[current]++;
            }
            return counts;
        }
        public static IDominoShape[] DeleteRowColumn(IRowColumnAddableDeletable reference, bool column, int[] positions, 
            out int[] remaining_positions, out int[][] deleted_shapes)
        {
            int[] distinct = getDistict(column, reference, positions);
            int total_deleted = distinct.Sum(x => x > 0 ? 1 : 0);
            int new_width = reference.current_width - (column ? total_deleted : 0);
            int new_height = reference.current_height - (!column ? total_deleted : 0);
            IDominoShape[] new_shapes = reference.getNewShapes(new_width, new_height);
            deleted_shapes = new int[total_deleted][];
            remaining_positions = new int[total_deleted];
            int deleted_counter = 0;
            int rowcollength = deleted_shapes.Length / total_deleted;
            for (int i = -1; i < distinct.Length; i++)
            {
                if (i != -1 && i != distinct.Length - 1 && distinct[i] > 0)
                {

                    deleted_shapes[deleted_counter] = reference.ExtractRowColumn(column, i);
                    int position = reference.getIndexFromPosition(i - deleted_counter, 0, 0, new_width, new_height, column);
                    remaining_positions[deleted_counter] = position;
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
        public static IDominoShape[] AddRowColumn(IRowColumnAddableDeletable reference, bool column, int[] positions, int[][] shapes, out int[] inserted_positions)
        {
            int[] distinct = getDistict(column, reference, positions);
            int total_added = distinct.Sum();
            int new_width = reference.current_width + (column ? total_added : 0);
            int new_height = reference.current_height + (!column ? total_added : 0);
            IDominoShape[] new_shapes = reference.getNewShapes(new_width, new_height);
            inserted_positions = new int[total_added];
            int added_counter = 0;
            for (int i = -1; i < distinct.Length; i++)
            {
                if (i != -1 && distinct[i] > 0)
                {
                    for (int i2 = 0; i2 < distinct[i]; i2++)
                    {
                        reference.ReinsertRowColumn(shapes[added_counter + i2], column, 
                            i + added_counter + i2, new_shapes, new_width, new_height);
                        inserted_positions[added_counter+i2] =
                            reference.getIndexFromPosition(i + i2 + added_counter, 0, 0, new_width, new_height, column);
                    }
                    added_counter+=distinct[i];
                }
                copyColors(reference, new_shapes, -1, column ? reference.current_height : reference.current_width, i, i,
                     0, added_counter, new_width, new_height, column);
            }
            reference.current_width = new_width;
            reference.current_height = new_height;
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
            var index = southeast ? reference.getIndexFromPosition(pos.Y + (!column ? 1 : 0), pos.X + (column ? 1 : 0), pos.CountInsideCell) : position;
            return AddRowColumn(reference, column, Enumerable.Repeat(index, count).ToArray(), shapes, out inserted_positions);
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
        int[] paste_target;
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
                paste_colors[i] = field.last[paste_source[i]].color;
            }
            for (int i = 0; i < paste_target.Length; i++)
            {
                if (paste_target[i] < field.last.length)
                {
                    original_colors[i] = field.last[paste_target[i]].color;
                    field.last[paste_target[i]].color = paste_colors[i];
                }
            }
        }
        public override void Undo()
        {
            var field = (IDominoProvider)reference;
            for (int i = 0; i < paste_target.Length; i++)
            {
                if (paste_target[i] < field.last.length)
                {
                    field.last[paste_target[i]].color = original_colors[i];
                }
            }
        }
    }
}
