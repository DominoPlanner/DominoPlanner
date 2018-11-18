using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DominoPlanner.Core
{
    public abstract class HistoryTree<T> where T : IDominoProvider
    {
        private List<HistoryTree<T>> _children;
        private bool _isExpanded;
        private bool _isSelected;
        private HistoryTree<T> _parent;
        public HistoryTree<T> parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
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
        public HistoryTree()
        {
            _children = new List<HistoryTree<T>>();
        }
        public HistoryTree(HistoryTree<T> parent) 
        {
            parent.addChild(this);
        }
        public void addChild(HistoryTree<T> child)
        {
            child.parent = this;
            _children.Add(child);
        }
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

        public T getState()
        {
            List<HistoryTree<T>> parents = new List<HistoryTree<T>>();
            parents.Add(this);
            HistoryTree<T> akt = this;
            while (!akt.IsKeyframe && !akt.IsRoot)
            {
                parents.Add(akt);
                akt = akt.parent;
            }
            parents.Reverse();
            T akt_object = parents[0].state_before;
            for (int i = 0; i < parents.Count; i++)
            {
                parents[i].execute(akt_object);
            }
            parents[parents.Count - 1].finalize(akt_object);
            return akt_object;
        }
    }
    public class EmptyOperation<T> : HistoryTree<T> where T : IDominoProvider
    {
        public EmptyOperation(T input)
        {
            state_before = (T)input.Clone();
        }
        public override void execute(T input)
        {
           
        }
    }
    public abstract class NeedsFinalizationOperation<T> : HistoryTree<T> where T: IDominoProvider
    {
        public override void execute(T input)
        {
            if(!input.lastValid)
            {
                input.Generate();
                state_before = (T)input.Clone();
            }
            executeInternal(input);
        }
        public abstract void executeInternal(T input);

        public NeedsFinalizationOperation(HistoryTree<T> history) : base(history)
        {

        }
    }

}
