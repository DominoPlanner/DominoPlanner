using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            
        }
        public static IDominoWrapper CreateNodeFromPath(DominoAssembly futureParent, string path)
        {
            var parentPath = Workspace.Find(futureParent);
            var relPath = Workspace.MakeRelativePath(parentPath, path);
            var deserialized = Workspace.Load<IDominoProvider>(path);
            switch (deserialized)
            {
                case FieldParameters p:
                    return new FieldNode(relPath, futureParent);
                case StructureParameters p:
                    return new StructureNode(relPath, futureParent);
                case SpiralParameters p:
                    return new SpiralNode(relPath, futureParent);
                case CircleParameters p:
                    return new CircleNode(relPath, futureParent);
                default:
                    throw new ArgumentException("Path is not loadable");
            }
        }
    }
    [ProtoContract]
    public class DominoAssembly : IWorkspaceLoadable
    {
        [ProtoMember(1, OverwriteList =true)]
        public ObservableCollection<IDominoWrapper> children;
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
                colors = Workspace.Load<ColorRepository>(Workspace.AbsolutePathFromReference(ref _colorPath, this));
            }
        }
        public ColorRepository colors { get; private set; }
        public DominoAssembly()
        {
            children = new ObservableCollection<IDominoWrapper>();
        }

        public void Save(string relativePath = "")
        {
            Workspace.Save(this, relativePath);
        }
        [ProtoAfterDeserialization]
        private void CheckIntegrity()
        {
            if (colorPath == null || colors == null)
            {
                throw new InvalidDataException("File invalid");
            }
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
        private string _relativePath;
        public string relativePath
        {
            get => _relativePath;
            set
            {
                if (value != _relativePath)
                {
                    if (_obj != null) Workspace.CloseFile(obj);
                    _relativePath = value;
                    _obj = null;
                    RelativePathChanged?.Invoke(this, null);
                    var res = obj;
                }
            }
        }
        public string AbsolutePath
        {
            get
            {
                string _oldpath = _relativePath;
                var result = Workspace.AbsolutePathFromReference(ref _relativePath, parent);
                if (_oldpath != _relativePath) RelativePathChanged?.Invoke(this, null);
                return result;
            }
        }
        private IDominoProvider _obj;
        public IDominoProvider obj
        {
            get
            {
                if (_obj == null)
                {
                    _obj = Workspace.Load<IDominoProvider>(AbsolutePath);
                }
                return _obj;
            }
        }
        public override int[] counts
        {
            get
            {
                if (_obj == null)
                    return Workspace.LoadColorList<IDominoProviderPreview>(AbsolutePath).Item2;
                return obj.Counts;
            }
        }
        public event EventHandler RelativePathChanged;
        public DocumentNode(string relativePath, DominoAssembly parent) : base(parent)
        {
            this.relativePath = relativePath;
            RelativePathChanged += (sender, args) => parent?.Save();
            if (parent != null) // rootnode
                parent.children.Add(this);
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
    [ProtoContract(SkipConstructor = true)]
    public class AssemblyNode : IDominoWrapper
    {
        [ProtoMember(1)]
        private string path;

        public string Path
        {
            get { return path; }
            set
            {
                if (_obj != null) Workspace.CloseFile(obj);
                path = value;
                RelativePathChanged?.Invoke(this, null);
                _obj = null;
                var res = obj;
            }
        }
        public string AbsolutePath
        {
            get
            {
                string _oldpath = path;
                var result = Workspace.AbsolutePathFromReference(ref path, parent);
                if (_oldpath != path)
                {
                    RelativePathChanged?.Invoke(this, null);
                }
                return result;
            }
        }

        private DominoAssembly _obj;
        public DominoAssembly obj
        {
            get
            {
                if (_obj == null)
                    _obj = Workspace.Load<DominoAssembly>(AbsolutePath);
                return _obj;
            }
        }
        public event EventHandler RelativePathChanged;

        public AssemblyNode(string Path, DominoAssembly parent = null) : base(parent)
        {
            this.Path = Path;
            RelativePathChanged += (sender, args) => parent?.Save();
            if (parent != null) // rootnode
                parent.children.Add(this);
        }
        public void Save()
        {
            Workspace.Save(obj);
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
