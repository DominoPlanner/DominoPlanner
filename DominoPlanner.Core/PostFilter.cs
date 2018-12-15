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
        public SetColorOperation(IDominoProvider reference, int[] domain, int toReplace, int newColor)
        {
            
            this.domain = domain;
            this.new_color = newColor;
            old_colors = new int[domain.Length];
        }
    }
    public interface IRowColumnAddableDeletable
    {
        int current_width { get; set; }
        int current_height { get; set; }
        /// <summary>
        /// Fügt eine Reihe hinzu
        /// </summary>
        /// <param name="position">Index der Referenz</param>
        /// <param name="below">Gibt an, ob unter oder oder über dem aktuellen Stein eingefügt werden soll</param>
        /// <param name="color">Farbe der neu eingefügten Zeilen</param>
        /// <param name="count">Anzahl an neu eingefügten Zeilen</param>
        /// <returns></returns>
        int[] AddRow(int position, bool below, int color, int count);
        /// <summary>
        /// Fügt eine Zeile hinzu und ersetzt die neu hinzugefügten Reihen durch die angegeben Shapes
        /// </summary>
        /// <param name="position">Index der Referenz</param>
        /// <param name="count">Anzahl an neu eingefügten Zeilen</param>
        /// <param name="below">Gibt an, ob unter oder oder über dem aktuellen Stein eingefügt werden soll</param>
        /// <param name="shapes">Farben der hinzugefügten Steine</param>
        void AddRow(int position, int count, bool below, IDominoShape[] shapes);
        /// <summary>
        /// Löscht eine oder mehrere Zeilen
        /// </summary>
        /// <param name="position">Index aller Steine, von denen die sie enthaltenden Zeilen/Spalten gelöscht werden sollen</param>
        /// <param name="remaining_position">Ausgabewert: Position eines Steins, der als Referenz zum Wiedereinfügen dient</param>
        /// <param name="direction">Ausgabewert: Richtung, in der dieser Stein liegt</param>
        /// <returns>Liste der gelöschten Shapes zum Wiedereinfügen</returns>
        IDominoShape[] DeleteRow(int[] position, out int remaining_position, out bool direction);
        int[] AddColumn(int position, bool right, int color, int count);
        IDominoShape[] DeleteColumn(int[] position, out int remaining_position, out bool direction);
        void AddColumn(int position, int count, bool below, IDominoShape[] shapes);
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
            added_indizes = reference.AddRow(position, below, color, count);
        }
        public override void Undo()
        {
            bool position;
            int remaining;
            reference.DeleteRow(added_indizes, out remaining, out position);
        }
    }
    public class DeleteRow : PostFilter
    {
        IRowColumnAddableDeletable reference;
        int[] position;
        int remaining_position;
        bool direction;
        IDominoShape[] oldShapes;
        public DeleteRow(IRowColumnAddableDeletable reference, int[] position)
        {
            if (reference.current_height == 0) throw new InvalidOperationException();
            this.reference = reference;
            this.position = position;
        }
        public override void Apply()
        {
            oldShapes = reference.DeleteRow(position, out remaining_position, out direction);
        }
        public override void Undo()
        {
            reference.AddRow(remaining_position, position.Length, direction, oldShapes);
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
            added_indizes = reference.AddColumn(position, right, color, count);
        }
        public override void Undo()
        {
            bool position;
            int remaining;
            reference.DeleteColumn(added_indizes, out remaining, out position);
        }
    }
    public class DeleteColumn : PostFilter
    {
        IRowColumnAddableDeletable reference;
        int[] position;
        int remaining_position;
        bool direction;
        IDominoShape[] oldShapes;
        public DeleteColumn(IRowColumnAddableDeletable reference, int[] position)
        {
            if (reference.current_height == 0) throw new InvalidOperationException();
            this.reference = reference;
            this.position = position;
        }
        public override void Apply()
        {
            oldShapes = reference.DeleteRow(position, out remaining_position, out direction);
        }
        public override void Undo()
        {
            reference.AddRow(remaining_position, position.Length, direction, oldShapes);
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
            var field = (IDominoProvider)reference;
            for (int i = 0; i < paste_target.Length; i++)
            {
                if (paste_target[i] < field.last.length)
                {
                    original_colors[i] = field.last[paste_target[i]].color;
                    field.last[paste_target[i]].color = paste_source[i];
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
