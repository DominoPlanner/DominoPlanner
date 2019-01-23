using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    [ProtoContract(SkipConstructor =true)]
    [ProtoInclude(100, typeof(DocumentNode))]
    [ProtoInclude(101, typeof(AssemblyNode))]
    [ProtoInclude(102, typeof(LineNode))]
    [ProtoInclude(103, typeof(LineBlockNode))]
    [ProtoInclude(104, typeof(ArbitraryColorMatchedObject))]
    [ProtoInclude(105, typeof(FreeObject))]
    public abstract class IDominoWrapper
    {
        [ProtoMember(3, AsReference = true)]
        public DominoAssembly parent;
        public virtual int[] counts {get;}
        [ProtoMember(1)]
        List<DominoConnector> outnodes { get; set; }
        [ProtoMember(2)]
        DominoConnector innode { get; set; }
        public IDominoWrapper(DominoAssembly parent)
        {
            innode = new DominoConnector();
            innode.next = this;
            this.parent = parent;
            if (parent != null) // rootnode
                parent.children.Add(this);
        }
    }
    [ProtoContract]
    public class DominoAssembly : IWorkspaceLoadable
    {
        [ProtoMember(1, OverwriteList =true)]
        public List<IDominoWrapper> children;
        [ProtoMember(2)]
        List<Constraint> constraints;
        private string _colorPath;
        [ProtoMember(3)]
        public string colorPath
        {
            get
            {
                return _colorPath;
            }
            set
            {
                _colorPath = value;
                colors = Workspace.Load<ColorRepository>(_colorPath, this);
            }
        }
        public ColorRepository colors { get; private set; }
        public DominoAssembly()
        {
            children = new List<IDominoWrapper>();
        }

        public void Save(string relativePath = "")
        {
            Workspace.Save(this, relativePath);
        }

    }
    [ProtoContract(SkipConstructor =true)]
    [ProtoInclude(100, typeof(FieldNode))]
    [ProtoInclude(101, typeof(StructureNode))]
    [ProtoInclude(102, typeof(CircleNode))]
    [ProtoInclude(103, typeof(SpiralNode))]
    public abstract class DocumentNode : IDominoWrapper
    {
        [ProtoMember(1)]
        public string relativePath;
        private IDominoProvider _obj;
        public IDominoProvider obj
        {
            get
            {
                if (_obj == null)
                    _obj = Workspace.Load<IDominoProvider>(relativePath, parent);
                return _obj;
            }
        }
        public override int[] counts
        {
            get
            {
                if (_obj == null)
                    return Workspace.LoadColorList<IDominoProvider>(relativePath);
                return obj.counts;
            }
        }
        public DocumentNode(string relativePath, DominoAssembly parent) : base(parent)
        {
            this.relativePath = relativePath;
        }
    }
    [ProtoContract(SkipConstructor = true)]
    public class FieldNode : DocumentNode
    {
        public FieldNode(string relativePath, DominoAssembly parent) : base (relativePath, parent)
        {

        }
        // hier kommen mal so Sachen wie Feldanstoß rein
    }
    [ProtoContract(SkipConstructor = true)]
    public class StructureNode : DocumentNode
    {
        public StructureNode(string relativePath, DominoAssembly parent) : base(relativePath, parent)
        {

        }
    }
    [ProtoContract(SkipConstructor = true)]
    public class SpiralNode : DocumentNode
    {
        public SpiralNode(string relativePath, DominoAssembly parent) : base(relativePath, parent)
        {

        }
    }
    [ProtoContract(SkipConstructor = true)]
    public class CircleNode : DocumentNode
    {
        public CircleNode(string relativePath, DominoAssembly parent) : base(relativePath, parent)
        {

        }
    }
    [ProtoContract(SkipConstructor = true)]
    public class LineNode : IDominoWrapper
    {
        public LineNode(DominoAssembly parent) : base(parent)
        {

        }
    }
    [ProtoContract(SkipConstructor = true)]
    public class LineBlockNode : IDominoWrapper
    {
        public LineBlockNode(DominoAssembly parent) : base(parent)
        {

        }
    }
    [ProtoContract(SkipConstructor = true)]
    public class ArbitraryColorMatchedObject : IDominoWrapper
    {
        // so was wie Handsetting, bei dem aus einem Bild die Farben berechnet werden
        public ArbitraryColorMatchedObject(DominoAssembly parent) : base(parent)
        {

        }
    }
    [ProtoContract(SkipConstructor = true)]
    public class FreeObject : IDominoWrapper
    {
        // irgendwas, wo die Anzahlen vom Benutzer vorgegeben werden
        public FreeObject(DominoAssembly parent) : base(parent)
        {

        }
    }
    [ProtoContract(SkipConstructor =true)]
    public class AssemblyNode : IDominoWrapper
    {
        [ProtoMember(1)]
        public string Path;
        private DominoAssembly _obj;
        public DominoAssembly obj
        {
            get
            {
                if (_obj == null)
                    _obj = Workspace.Load<DominoAssembly>(Path, parent);
                return _obj;
            }
        }
        public AssemblyNode(string Path, DominoAssembly parent = null) : base(parent)
        {
            this.Path = Path;
        }
    }
    [ProtoContract]
    public class DominoConnector
    {
        [ProtoMember(1, AsReference =true)]
        public IDominoWrapper next;
    }
    [ProtoContract]
    public abstract class Constraint
    {
        [ProtoMember(1)]
        bool isUpToDate;
    }
}
