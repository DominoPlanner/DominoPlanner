using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace DominoPlanner.Core
{
    partial class StructureParameters
    {
        public String name;
        internal CellDefinition[,] cells;
        private double PreviewScaleFactor(int TargetDimension)
        {
            double largest = 0;
            // get largest dimension
            foreach (CellDefinition c in cells)
            {
                if (c.width > largest) largest = c.width;
                if (c.height > largest) largest = c.height;
            }
            return TargetDimension / largest;
        }
        private int GetIndex(string Position)
        {
            if (Position == "Center") return 1;
            else if (Position == "Left" || Position == "Right")
            {
                return (Position == "Left") ? 0 : 2;
            }
            else
            {
                return (Position == "Top") ? 0 : 2;
            }
        }
        public GenStructHelper GenerateStructure(int sWidth, int sHeight)
        {
            GenStructHelper g = new GenStructHelper() // Initialize GenStructHelper with final size.
            {
                width = cells[0, 0].width + cells[1, 1].width * sWidth + cells[2, 2].width,
                height = cells[0, 0].height + cells[1, 1].height * sHeight + cells[2, 2].height
            };
            g.dominoes = getNewShapes(sWidth, sHeight);
            g.HasProtocolDefinition = hasProcotolDefinition;
            int a = g.dominoes.Max(s => s.GetContainer().x2);
            return g;
        }
        public WriteableBitmap DrawPreview(int col, int row, int targetDimension)
        {
            CellDefinition cell = cells[col, row];
            double scalingFactor = PreviewScaleFactor(targetDimension); // scale everything to the same size
            WriteableBitmap b = BitmapFactory.New((int)(cell.width * scalingFactor + 2), (int)(cell.height * scalingFactor + 2));
            for (int colc = (col == 2) ? 1 : 0; colc <= ((col == 0) ? 1 : 2); colc++)
            {
                for (int rowc = (row == 2) ? 1 : 0; rowc <= ((row == 0) ? 1 : 2); rowc++) // only use the cells next to the specified (so top left uses 4 top center, center left and center center).
                {
                    CellDefinition current = cells[colc, rowc];
                    int xOffsetMultiplier = colc - col; // for moving the cells
                    int yOffsetMultiplier = rowc - row;
                    for (int i = 0; i < current.dominoes.Length; i++)
                    {
                        IDominoShape transformed = current.dominoes[i].TransformDomino(
                            xOffsetMultiplier * ((xOffsetMultiplier > 0) ? cell.width : current.width),
                            yOffsetMultiplier * ((yOffsetMultiplier > 0) ? cell.height : current.height), 0, 0, 0, 0); // move the dominoes
                        DominoRectangle container = transformed.GetContainer(); // get containing rectangle
                        if (container.x >= cells[col, row].width || container.x + container.width <= 0 || container.y >= cells[col, row].height || container.y + container.height <= 0) continue; // check if rectangle is out of drawing area
                        b.FillPolygon(transformed.GetPath(scalingFactor).getOffsetRectangle((int)Math.Ceiling(scalingFactor)).getWBXPath(), System.Windows.Media.Colors.Black); // outline
                        b.FillPolygon(transformed.GetPath(scalingFactor).getOffsetRectangle(-(int)Math.Ceiling(scalingFactor)).getWBXPath(), System.Windows.Media.Colors.LightGray); // inner line
                    }
                }
            }
            return b;
        }
    }
    public struct GenStructHelper
    {
        public double width;
        public double height;
        public IDominoShape[] dominoes;
        public bool HasProtocolDefinition;
    }
    internal class CellDefinition
    {
        public double width;
        public double height;
        public IDominoShape[] dominoes;
        public CellDefinition() { }
        public CellDefinition(XElement part)
        {
            width = float.Parse(part.Attribute("Width").Value, CultureInfo.InvariantCulture);
            height = float.Parse(part.Attribute("Height").Value, CultureInfo.InvariantCulture);
            dominoes = part.Elements().Select(x => IDominoShape.LoadDefinition(x)).ToArray();
        }
        public CellDefinition TransformDefinition(double moveX, double moveY, int i, int j, int width, int height)
        {
            return new CellDefinition()
            {
                width = this.width,
                height = this.height,
                dominoes = this.dominoes.Select(d => d.TransformDomino(moveX, moveY, i, j, width, height)).ToArray()
            };
        }
        public int Count => dominoes.Length;
    }
}
