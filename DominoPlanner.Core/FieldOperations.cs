using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    public class ChangeDimensionOperation<T> : Operation<T> where T : FieldParameters
    {
        public int length;
        public int width;
        public override void execute(T input)
        {
            input.length = length;
            input.height = width;
        }
        public ChangeDimensionOperation()
        {
            icon_path = "icons/stuff";
        }
    }
    public class ChangeMarginOperation<T> : Operation<T> where T : FieldParameters
    {
        public int a;
        public int b;
        public int c;
        public int d;
        public override void execute(T input)
        {
            input.a = a;
            input.b = b;
            input.c = c;
            input.d = d;
        }
    }
    public class AddRowsOperation<T> : NeedsFinalizationOperation<T> where T : FieldParameters
    {
        public int index;
        public int rows;
        public int color_new;
        public override void executeInternal(T input)
        {
            input.Generate();
            input.height = (input.height + rows);
            input.GenerateShapes();
            int[] field = new int[input.length * (input.height + rows)];
            for (int yi = 0; yi < input.height+rows; yi++)
            {
                for (int xi = 0; xi < input.length; xi++)
                {
                    if (yi < index)
                    {
                        field[input.height * xi + yi] = input.last.dominoes[input.height * xi + yi];
                    }
                    else if (yi < index + rows)
                    {
                        field[input.height * xi + yi] = color_new;
                    }
                    else
                    {
                        field[input.height * xi + yi] = input.last.dominoes[input.height * xi + (yi - rows)];
                    }
                }
            }
            input.lastValid = true;
            
        }
    }
    public class AddColumnsOperation<T> : NeedsFinalizationOperation<T> where T : FieldParameters
    {
        public int index;
        public int columns;
        public int color_new;
        public override void executeInternal(T input)
        {
            
            input.Generate();
            input.length = (input.height + columns);
            input.GenerateShapes();
            int[] field = new int[(input.length + columns) * (input.height)];
            for (int yi = 0; yi < input.height; yi++)
            {
                for (int xi = 0; xi < input.length + columns; xi++)
                {
                    if (xi < index)
                    {
                        field[input.height * xi + yi] = input.last.dominoes[input.height * xi + yi];
                    }
                    else if (xi < index + columns)
                    {
                        field[input.height * xi + yi] = color_new;
                    }
                    else
                    {
                        field[input.height * xi + yi] = input.last.dominoes[input.height * (xi - columns) + yi];
                    }
                }
            }
            input.lastValid = true;
        }
    }
    public class ReplaceColorOperation<T> : NeedsFinalizationOperation<T> where T : IDominoProvider
    {
        public int[] indices;
        public int color_old;
        public int color_new;
        public override void executeInternal(T input)
        {
            input.Generate();
            foreach (int index in indices)
            {
                if (input.last.dominoes[index] == color_old) input.last.dominoes[index] = color_new;
            }
        }
    }
    public class SetColorOperation<T> : NeedsFinalizationOperation<T> where T: IDominoProvider
    {
        public int[] indices;
        public int color_old;
        public int color_new;
        public override void executeInternal(T input)
        {
            input.Generate();
            foreach (int index in indices)
            {
                input.last.dominoes[index] = color_new;
            }
        }
    }
}
