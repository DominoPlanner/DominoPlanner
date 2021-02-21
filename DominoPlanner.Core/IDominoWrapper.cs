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
        public virtual int[] Counts {get;}
        [ProtoMember(1)]
        List<DominoConnector> Outnodes { get; set; }
        [ProtoMember(2)]
        DominoConnector Innode { get; set; }
        public IDominoWrapper(DominoAssembly parent)
        {
            Innode = new DominoConnector
            {
                next = this
            };
            this.parent = parent;
            
        }
        public static IDominoWrapper CreateNodeFromPath(DominoAssembly futureParent, string path)
        {
            var parentPath = Workspace.Find(futureParent);
            var relPath = Workspace.MakeRelativePath(parentPath, path);
            var deserialized = Workspace.Load<IDominoProvider>(path);
            return deserialized switch
            {
                FieldParameters _ => new FieldNode(relPath, futureParent),
                StructureParameters _ => new StructureNode(relPath, futureParent),
                SpiralParameters _ => new SpiralNode(relPath, futureParent),
                CircleParameters _ => new CircleNode(relPath, futureParent),
                _ => throw new ArgumentException("Path is not loadable"),
            };
        }
    }
    [ProtoContract]
    public class DominoAssembly : IWorkspaceLoadable
    {
        public bool ColorListBroken;
        [ProtoMember(1, OverwriteList =true)]
        public ObservableCollection<IDominoWrapper> children;
        [ProtoMember(2)]
        List<Constraint> constraints;
        private string _colorPath;
        [ProtoMember(3)]
        public string ColorPath
        {
            get
            {
                return _colorPath;
            }
            set
            {
                if (value.Contains("\\")) value = value.Replace("\\", "/");
                _colorPath = value;
                try
                {
                    Colors = Workspace.Load<ColorRepository>(Workspace.AbsolutePathFromReference(ref _colorPath, this));
                }
                catch (ProtoException)
                {
                    ColorListBroken = true;
                }

            }
        }
        public string AbsoluteColorPath
        {
            get
            {
                return Workspace.AbsolutePathFromReference(ref _colorPath, this);
            }
        }
        public ColorRepository Colors { get; private set; }
        public DominoAssembly()
        {
            children = new ObservableCollection<IDominoWrapper>();
        }

        public void Save(string relativePath = "")
        {
            Workspace.Save(this, relativePath);
        }
        [ProtoAfterDeserialization]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Nicht verwendete private Member entfernen", Justification = "called by Protobuf.net after Deserialization")]
        private void CheckIntegrity()
        {
            if (ColorPath == null || Colors == null)
            {
                throw new InvalidDataException("File invalid");
            }
        }
        // Check for circular references
        public bool ContainsReferenceTo(DominoAssembly assembly)
        {
            bool result = false;
            foreach (var s in children.OfType<AssemblyNode>())
            {
                if (s.Obj == assembly) return true;
                else result = result && ContainsReferenceTo(assembly);
            }
            return result;
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
        public string RelativePath
        {
            get => _relativePath;
            set
            {
                if (value.Contains("\\")) value = value.Replace("\\", "/");

                if (value != _relativePath)
                {
                    if (_obj != null) Workspace.CloseFile(Obj);
                    _relativePath = value;
                    _obj = null;
                    RelativePathChanged?.Invoke(this, null);
                    _ = Obj;
                }
            }
        }
        public string AbsolutePath
        {
            get
            {
                string newrelpath = _relativePath;
                var result = Workspace.AbsolutePathFromReference(ref newrelpath, parent);
                RelativePath = newrelpath;
                return result;
            }
        }
        private IDominoProvider _obj;
        public IDominoProvider Obj
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
        public override int[] Counts
        {
            get
            {
                if (_obj == null)
                    return Workspace.LoadColorList<IDominoProviderPreview>(AbsolutePath).Item2;
                return Obj.Counts;
            }
        }
        public event EventHandler RelativePathChanged;
        public DocumentNode(string relativePath, DominoAssembly parent) : base(parent)
        {
            this.RelativePath = relativePath;
            RelativePathChanged += (sender, args) => parent?.Save();
            if (parent != null) // rootnode
                parent.children.Add(this);
        }

        public bool ColorPathMatches(AssemblyNode assy)
        {
            var counts2 = Workspace.LoadColorList<IDominoProviderPreview>(ref _relativePath, assy.Obj);
            return (Path.GetFullPath(counts2.Item1) == Path.GetFullPath(assy.Obj.AbsoluteColorPath));
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
    public class FileNotFoundEventArgs : EventArgs
    {
        public FileNotFoundException ex;
        public FileNotFoundEventArgs(FileNotFoundException ex)
        {
            this.ex = ex;
        }
    }
    [ProtoContract(SkipConstructor = true)]
    public class AssemblyNode : IDominoWrapper
    {
        
        private string path;
        [ProtoMember(1)]
        public string Path
        {
            get
            {
                // update relative path in case it changed
                _ = AbsolutePath;
                return path;
            }
            set
            {
               if (value.Contains("\\")) value = value.Replace("\\", "/");
               
               if (_obj != null) Workspace.CloseFile(Obj);

                path = value;
                RelativePathChanged?.Invoke(this, null);
                _obj = null;
            }
        }
        public string AbsolutePath
        {
            get
            {
                return Workspace.AbsolutePathFromReference(ref path, parent);
            }
        }

        private DominoAssembly _obj;
        public DominoAssembly Obj
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
            Workspace.Save(Obj);
        }

        public bool ColorPathMatches(AssemblyNode other)
        {
            if (System.IO.Path.GetFullPath(this.Obj.AbsoluteColorPath) == System.IO.Path.GetFullPath(other.Obj.AbsoluteColorPath))
                return true;
            return false;
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
