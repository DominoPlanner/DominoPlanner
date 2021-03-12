using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core.RTree
{
    // workaround fürs In-Place-Sortieren von Listen
    /*public class ProjectionComparer
    {
        public void Create<TSource, TKey>(Func<TSource, TKey> projection)
    }
    public class ProjectionComparer<TSource, TKey> : IComparer<TSource>
    {
        IComparer<TKey> comparer;
        Func<TSource, TKey> projection;
        public ProjectionComparer(Func<TSource, TKey> projection, IComparer<TKey> comparer)
        {
            this.comparer = comparer ?? Comparer<TKey>.Default;
            this.projection = projection;
        }
        public ProjectionComparer(Func<TSource, TKey> projection) : this(projection, null);
        public int Compare(TSource x, TSource y)
        {
            throw new NotImplementedException();
        }
    }*/

    public class RTree<T> : IGeometry where T: IGeometry
    {
        public SplitNodeAlgorithm<T> algo;
        public Node<T> root;
        public DominoRectangle boundary;
        private readonly int maxitems;
        public RTree(int maxItems, SplitNodeAlgorithm<T> algo)
        {
            root = new Leaf<T>(maxItems);
            this.algo = algo;
            this.maxitems = maxItems;
           
        }

        public DominoRectangle GetBoundingRectangle()
        {
            return boundary;
        }

        public void Insert(T toAdd)
        {
            var result = root.Insert(toAdd, algo);
            if (result is NormalInsertResult)
            {
                boundary = (result as NormalInsertResult).bounding_rectangle.ExtendRectangle(boundary);
            }
            else
            {
                var r = (result as SplitResult<T>);
                root = new NonLeaf<T>(maxitems);
                root.AddInternal(r.n1);
                root.AddInternal(r.n2);
                boundary = r.n1.GetBoundingRectangle().CommonBoundingRectangle(r.n2.GetBoundingRectangle());

            }
            
        }
        public List<T> Search(DominoRectangle domain)
        {
            List<T> result = new List<T>();
            root.Search(domain, result);
            return result;
        }
        public bool Intersects(DominoRectangle rect)
        {
            throw new NotImplementedException();
        }
        /*public Image<Rgba, byte> Draw()
        {
            var result = new Image<Rgba, byte>(1100, 1100, new Rgba(255, 255, 255, 0));
            root.Draw(result, 0, 0, 0);
            return result;
        }*/
        /*private Node<T> BuildTree(List<T> data)
        {
            var treeHeight = GetDepth(data.Count);
            var rootMaxEntries = (int)Math.Ceiling(data.Count / Math.Pow(this.maxitems, treeHeight - 1));
            return BuildNodes(data, 0, data.Count - 1, treeHeight, rootMaxEntries);
        }*/
        private int GetDepth(int numNodes) =>
            (int)Math.Ceiling(Math.Log(numNodes) / Math.Log(this.maxitems));
        /*private Node<T> BuildNodes(List<T> data, int left, int right, int height, int maxEntries)
        {
            var num = right - left + 1;
            if (num <= maxEntries)
            {
                if (height == 1)
                    return new Leaf<T>(height, data.Skip(left).Take(num).ToList());
                else
                    return new NonLeaf<T>(new List<T> { BuildNodes(data, left, right, height - 1, this.maxitems) }, height);
            }

            data.Skip(left).Take(num).OrderBy(x => x.getBoundingRectangle().x);

            var nodeSize = (num + (maxEntries - 1)) / maxEntries;
            var subSortLength = nodeSize * (int)Math.Ceiling(Math.Sqrt(maxEntries));

            var children = new List<ISpatialData>(maxEntries);
            for (int subCounter = left; subCounter <= right; subCounter += subSortLength)
            {
                var subRight = Math.Min(subCounter + subSortLength - 1, right);
                data.Sort(subCounter, subRight - subCounter + 1, CompareMinY);

                for (int nodeCounter = subCounter; nodeCounter <= subRight; nodeCounter += nodeSize)
                {
                    children.Add(
                        BuildNodes(
                            data,
                            nodeCounter,
                            Math.Min(nodeCounter + nodeSize - 1, subRight),
                            height - 1,
                            this.maxEntries));
                }
            }

            return new Node(children, height);
        }*/
    }
    public abstract class SplitNodeAlgorithm<T> where T : IGeometry
    {
        public abstract void Split<A>(List<A> elements, SplitResult<T> result) where A : IGeometry;
    }
    public struct CommonSplitResult
    {
        public List<int> indizes1;
        public List<int> indizes2;
        public DominoRectangle bounding1;
        public DominoRectangle bounding2;
    }
    public class GuttmannQuadraticSplit<T> : SplitNodeAlgorithm<T> where T : IGeometry
    {
        public override void Split<A>(List<A> elements, SplitResult<T> result)
        {
            var seeds = PickSeeds(elements);
            result.n1.AddInternal(elements[seeds.Item1]);
            elements.RemoveAt(seeds.Item1); // Item1 ist größer als Item2
            result.n2.AddInternal(elements[seeds.Item2]);
            elements.RemoveAt(seeds.Item2);
            while (elements.Count != 0)
            {
                var picked = PickNext(elements, result.n1.GetBoundingRectangle(), result.n2.GetBoundingRectangle());
                if (picked.Item2)
                {
                    result.n1.AddInternal(elements[picked.Item1]);
                }
                else result.n2.AddInternal(elements[picked.Item1]);
                elements.RemoveAt(picked.Item1);
            }
        }
        private Tuple<int, int> PickSeeds<A>(List<A> list) where A : IGeometry
        {
            double max_waste = 0;
            int entry1 = 0;
            int entry2 = 0;
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    double size = list[i].GetBoundingRectangle().SizeOfCommonBoundingRectangle(list[j].GetBoundingRectangle()) 
                        - list[i].GetBoundingRectangle().Size - list[j].GetBoundingRectangle().Size;
                    if (size > max_waste)
                    {
                        entry1 = i;
                        entry2 = j;
                        max_waste = size;
                    }
                }
            }
            return new Tuple<int, int>(entry1, entry2);
        }
        // erster Wert: Index, zweiter Wert gibt an, zu welcher Gruppe er gehören soll (false = Gruppe2, true=Gruppe1)
        private Tuple<int, bool> PickNext<A>(List<A> list, DominoRectangle bounding1, DominoRectangle bounding2) where A : IGeometry
        {
            double preference = 0;
            int preferred = 0;
            bool l = false;
            for (int i = 0; i < list.Count; i++)
            {
                var result1 = bounding1.SizeOfCommonBoundingRectangle(list[i].GetBoundingRectangle()) - bounding1.Size;
                var result2 = bounding2.SizeOfCommonBoundingRectangle(list[i].GetBoundingRectangle()) - bounding2.Size;
                var pref = result2 - result1;
                if (Math.Abs(pref) > preference)
                {
                    preferred = i;
                    preference = Math.Abs(pref);
                    l = pref > 0;
                }
            }
            return new Tuple<int, bool>(preferred, l);
        }
    }
    public abstract class Node<T> : IGeometry where T : IGeometry
    {
        public DominoRectangle boundingbox;
        public abstract List<T> Search(DominoRectangle region, List<T> result);

        public abstract InsertResult Insert(T geometry, SplitNodeAlgorithm<T> algo);

        public bool Intersects(DominoRectangle rect)
        {
            return boundingbox.Intersects(rect);
        }
        public DominoRectangle GetBoundingRectangle()
        {
            return boundingbox;
        }
        public abstract void AddInternal(IGeometry g);
        //internal abstract void Draw(Image<Rgba, byte> result, int x, int y, int ebene);

        
    }
    public class Leaf<T> : Node<T> where T : IGeometry
    {
        public T[] data;
        public List<T> data2;
        private int dataLength = 0;
        public Leaf(int maxEntries, List<T> nodes)
        {
            data = new T[maxEntries];
            foreach (var node in nodes) AddInternal(node);
        }
        public Leaf(int maxEntries)
        {
            data = new T[maxEntries];
        }

        public override void AddInternal(IGeometry g)
        {
            data[dataLength] = (T) g;
            dataLength++;
            if (boundingbox == null) boundingbox = g.GetBoundingRectangle();
            else boundingbox = boundingbox.CommonBoundingRectangle(g.GetBoundingRectangle());
        }

        public override InsertResult Insert(T geometry, SplitNodeAlgorithm<T> algo)
        {
            if (dataLength < data.Length)
            {
                data[dataLength] = geometry;
                dataLength++;
            }
            else return SplitNode(geometry,algo);
            boundingbox = geometry.GetBoundingRectangle().ExtendRectangle(boundingbox);
            return new NormalInsertResult() { bounding_rectangle = boundingbox};
        }

        public override List<T> Search(DominoRectangle region, List<T> result)
        {
            for (int i = 0; i < dataLength; i++)
            { 
                if (data[i].Intersects(region)) result.Add(data[i]);
            }
            return result;
        }
        public SplitResult<T> SplitNode(T overflowed, SplitNodeAlgorithm<T> algo)
        {
            SplitResult<T> res = new SplitResult<T>
            {
                n1 = new Leaf<T>(this.data.Length),
                n2 = new Leaf<T>(this.data.Length)
            };
            List<IGeometry> list = new List<IGeometry>();
            foreach (var e in data) list.Add(e);
            list.Add(overflowed);
            algo.Split(list, res);
            return res;
        }
        /*internal override void Draw(Image<Rgba, byte> result, int x, int y, int ebene)
        {
            if (ebene > 5) ebene = 5;
            Rgba color = new Rgba(255 * (5 - ebene) / 5.0, 0, 255 * ebene / 5.0, 255);
            var rect = new System.Drawing.Rectangle((int)boundingbox.x - x, (int)boundingbox.y - y,
                (int)boundingbox.width, (int)boundingbox.height);
            var points = new System.Drawing.Point[]
            {
                new System.Drawing.Point(rect.X, rect.Y),
                new System.Drawing.Point(rect.X + rect.Width, rect.Y),
                new System.Drawing.Point(rect.X + rect.Width, rect.Y + rect.Height),
                new System.Drawing.Point(rect.X, rect.Y + rect.Height),
            };
            // Bounding Rectangle zeichnen
            result.FillConvexPoly(points, color);
            for (int i = 0; i< dataLength; i++)
            { 
                color = new Rgba(0, 255, 0, 255);
                result.Draw(new System.Drawing.Rectangle((int)data[i].getBoundingRectangle().x - x, 
                    (int)data[i].getBoundingRectangle().y - y,
                (int)data[i].getBoundingRectangle().width, (int)data[i].getBoundingRectangle().height), color, 1) ;
            }
        }*/
        
    }
    public class NonLeaf<T> : Node<T> where T : IGeometry
    {
        public Node<T>[] children;
        public NonLeaf(int maxEntries, List<Node<T>> nodes)
        {
            children = new Node<T>[maxEntries];
            foreach (var node in nodes) AddInternal(node);
        }
        public NonLeaf(int maxEntries)
        {
            children = new Node<T>[maxEntries];
        }
        private int dataLength;
        public int ChooseLeaf(DominoRectangle newBoundingRectangle)
        {
            int best_index = -1;
            double best_size = double.PositiveInfinity ;
            double best_overlap = double.PositiveInfinity;
            for (int i = 0; i < dataLength; i++)
            {
                double current_size = children[i].boundingbox.Size;
                double new_overlap = children[i].boundingbox.SizeOfCommonBoundingRectangle(newBoundingRectangle) - current_size;
                if (new_overlap < best_overlap || new_overlap == best_overlap && current_size < best_size)
                {
                    best_index = i;
                    best_size = current_size;
                    best_overlap = new_overlap;
                }
            }
            return best_index;
        }
    

        public override InsertResult Insert(T geometry, SplitNodeAlgorithm<T> algo)
        {
            var child = ChooseLeaf(geometry.GetBoundingRectangle());
            var result = children[child].Insert(geometry, algo);
            if (result is NormalInsertResult)
            {
                ExtendBoundingRectangle(geometry.GetBoundingRectangle());
                return result;
            }
            else
            {
                var res = (result as SplitResult<T>);
                children[child] = res.n1;
                if (dataLength < children.Length)
                {
                    children[dataLength] = res.n2;
                    dataLength++;
                    ExtendBoundingRectangle(geometry.GetBoundingRectangle());
                    return new NormalInsertResult
                    { bounding_rectangle = boundingbox};
                }
                else
                {
                    return SplitNode(res.n2, algo);
                }
            }
        }
        public SplitResult<T> SplitNode(Node<T> overflowed, SplitNodeAlgorithm<T> algo)
        {
            SplitResult<T> res = new SplitResult<T>
            {
                n1 = new NonLeaf<T>(this.children.Length),
                n2 = new NonLeaf<T>(this.children.Length)
            };
            List<IGeometry> list = new List<IGeometry>();
            foreach (var e in children) list.Add(e);
            list.Add(overflowed);
            algo.Split(list, res);
            return res;
        }
        public void ExtendBoundingRectangle(DominoRectangle extension)
        {
            boundingbox = extension.ExtendRectangle(boundingbox);
        }
        public override List<T> Search(DominoRectangle region, List<T> result)
        {
            for (int i = 0; i < dataLength; i++)
            {
                if (region.Intersects(children[i].boundingbox)) children[i].Search(region, result);
            }
            return result;
        }

        public override void AddInternal(IGeometry g)
        {
            children[dataLength] = (Node<T>) g;
            dataLength++;
            if (boundingbox == null) boundingbox = g.GetBoundingRectangle();
            else boundingbox = boundingbox.CommonBoundingRectangle(g.GetBoundingRectangle());
        }
        /*internal override void Draw(Image<Rgba, byte> result, int x, int y, int ebene)
        {
            if (ebene > 5) ebene = 5;
            Rgba color = new Rgba(255 * (5 - ebene) / 5.0, 0, 255 * ebene / 5.0, 255);
            var rect = new System.Drawing.Rectangle((int)boundingbox.x - x, (int)boundingbox.y - y,
                (int)boundingbox.width, (int)boundingbox.height);
            var points = new System.Drawing.Point[]
            {
                new System.Drawing.Point(rect.X, rect.Y),
                new System.Drawing.Point(rect.X + rect.Width, rect.Y),
                new System.Drawing.Point(rect.X + rect.Width, rect.Y + rect.Height),
                new System.Drawing.Point(rect.X, rect.Y + rect.Height),
            };
            // Bounding Rectangle zeichnen
            result.FillConvexPoly(points, color);
            for(int i = 0; i < dataLength; i++)
                children[i].Draw(result, x, y, ebene + 1);
        }*/
    }
    public interface IGeometry
    {
        bool Intersects(DominoRectangle rect);

        DominoRectangle GetBoundingRectangle();
    }
    public abstract class InsertResult
    {

    }
    public class SplitResult<T> : InsertResult where T: IGeometry
    {
        public Node<T> n1;
        public Node<T> n2;
    }
    public class NormalInsertResult : InsertResult
    {
        public DominoRectangle bounding_rectangle;
    }
}
