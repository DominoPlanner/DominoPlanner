using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DominoPlanner.Core
{
    public class HistoryTree<T> where T: IDominoProvider
    {
        public string icon_path
        {
            get { return op.icon_path; }
        }
        private List<HistoryTree<T>> _children;
        private bool _isExpanded;
        private bool _isSelected;
        public Operation<T> op { get; private set; }
        private HistoryTree<T> _parent;
        public HistoryTree<T> parent {  get
            {
                return _parent;
            }
            set
            {
                this.parent = value;
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
            }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
            }
        }
        public bool IsRoot
        {
            get { return parent == null; }
        }
        public HistoryTree(Operation<T> operation)
        {
            op = operation;
            _children = new List<HistoryTree<T>>();
        }
        public HistoryTree(Operation<T> operation, HistoryTree<T> parent) : this(operation)
        {
            parent.addChild(this);
        }
        public void addChild(HistoryTree<T> child)
        {
            child.parent = this;
            _children.Add(child);
        }

        public T getState()
        {
            List<HistoryTree<T>> parents = new List<HistoryTree<T>>();
            HistoryTree<T> akt = this;
            while (!akt.op.IsKeyframe && !akt.IsRoot)
            {
                parents.Add(akt);
                akt = akt.parent;
            }
            parents.Reverse();
            T akt_object = parents[0].op.state_before;
            for (int i = 0; i < parents.Count; i++)
            {
                parents[i].op.execute(akt_object);
            }
            parents[parents.Count-1].op.finalize(akt_object);
            return akt_object;
        }
    }

    public abstract class Operation<T> where T : IDominoProvider
    {
        public string icon_path;
        public T state_before;
        public bool IsKeyframe
        {
            get { return state_before == null; }
        }
        public abstract void execute(T input);

        public void execute()
        {
            if (state_before != null)
            {
                execute(state_before);
            }
            else throw (new Exception("kein Keyframe"));
        }
        public void finalize(T obj)
        {
            if (obj is IDominoProvider)
            {
                ((IDominoProvider)obj).Generate();
            }
            else throw (new Exception("kein unterstützter Datentyp übergeben"));
        }
    }
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
    public class ChangeMarginOperation<T> : Operation<T> where T: FieldParameters
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
    public class AddRowsOperation<T> : Operation<T> where T: FieldParameters
    {
        public override void execute(T input)
        {
            // Falls noch nicht berechnet, Feld neu berechnen und als Keyframe speichern.
            if (input.lastValid)
            {
                input.Generate();
                state_before = input; // TODO: Durch Deep Copy ersetzen 
            }
            throw new NotImplementedException();
        }
       
    }
}
