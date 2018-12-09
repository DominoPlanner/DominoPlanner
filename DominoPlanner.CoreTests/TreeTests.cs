using DominoPlanner.Core;
using DominoPlanner.Core.RTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.CoreTests
{
    class TreeTests
    {
        public static void TreeTest()
        {
            var tree = new RTree<RectangleDomino>(4, new GuttmannQuadraticSplit<RectangleDomino>());
            Random r = new Random();
            for (int i = 0; i < 100; i++)
            {
                // Rechteck mit Koordinaten zwischen 0 und 1000 und Abmessungen zwischen 0 und 100 erzeugen
                RectangleDomino dom = new RectangleDomino()
                {
                    x = r.Next(0, 1000),
                    y = r.Next(0, 1000),
                    width = r.Next(0, 100),
                    height = r.Next(0, 100)
                };
                tree.Insert(dom);
                //tree.Draw().Save("tests/tree/tree" + i + ".png");
            }
            LinearTreeTest();
        }
        public static void LinearTreeTest()
        {
            var tree = new RTree<RectangleDomino>(4, new GuttmannQuadraticSplit<RectangleDomino>());
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            int res = 500;
            var rectangles = new RectangleDomino[res * res];
            watch.Start();
            for (int i = 0; i < res; i++)
            {
                for (int j = 0; j < res; j++)
                {
                    // Rechteck mit Koordinaten zwischen 0 und 1000 und Abmessungen zwischen 0 und 100 erzeugen
                    rectangles[i * res + j] = new RectangleDomino()
                    {
                        x = i * 50,
                        y = j * 50,
                        width = 20,
                        height = 20
                    };

                }
            }
            watch.Stop();
            Console.WriteLine("Ticks for creating array with all dominoes: " + watch.ElapsedTicks);
            watch.Restart();
            for (int i = 0; i < res * res; i++)
            {
                tree.Insert(rectangles[i]);
                //tree.Draw().Save("tests/tree_lin/tree" + (i) + ".png");
            }
            watch.Stop();
            Console.WriteLine("Tree Construction: " + watch.ElapsedTicks);
            DominoRectangle search = new DominoRectangle() { x = 0, y = 0, width = 20, height = 20 };
            double sum = 0;
            Console.WriteLine("Array search");
            for (int i = 0; i < 100; i++)
            {
                watch.Restart();
                var result = SearchInArray(search, rectangles);
                watch.Stop();
                sum += watch.ElapsedTicks;
                Console.WriteLine(watch.ElapsedTicks + ", Durchschnitt: " + sum / (i + 1.0) + ", gefunden: " + result);

            }
            Console.WriteLine("Tree search");
            sum = 0;
            for (int i = 0; i < 100; i++)
            {
                watch.Restart();
                var result = SearchInTree(search, tree);
                watch.Stop();
                sum += watch.ElapsedTicks;
                Console.WriteLine(watch.ElapsedTicks + ", Durchschnitt: " + sum / (i + 1.0) + ", gefunden: " + result);
            }
        }
        public static int SearchInArray(DominoRectangle search, RectangleDomino[] rectangles)
        {
            List<RectangleDomino> result = new List<RectangleDomino>();
            for (int j = 0; j < rectangles.Length; j++)
            {
                if (rectangles[j].Intersects(search)) result.Add(rectangles[j]);
            }
            return result.Count;
        }
        public static int SearchInTree(DominoRectangle search, RTree<RectangleDomino> tree)
        {
            var result = tree.Search(search);
            return result.Count;
        }
    }
}
