using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    [ProtoContract]
    [ProtoInclude(100, typeof(DocumentNode))]
    [ProtoInclude(101, typeof(AssemblyNode))]
    [ProtoInclude(102, typeof(LineNode))]
    [ProtoInclude(103, typeof(LineBlockNode))]
    [ProtoInclude(104, typeof(ArbitraryColorMatchedObject))]
    [ProtoInclude(105, typeof(FreeObject))]
    public abstract class IDominoWrapper
    {
        public virtual int[] counts {get;}
        [ProtoMember(1)]
        List<DominoConnector> outnodes { get; set; }
        [ProtoMember(2)]
        DominoConnector innode { get; set; }

        public IDominoWrapper()
        {
            innode = new DominoConnector();
            innode.next = this;
        }
    }
    public class DominoAssembly : IWorkspaceLoadable
    {
        [ProtoMember(1)]
        List<IDominoWrapper> children;
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
                colors = Workspace.Load<ColorRepository>(_colorPath);
            }
        }
        public ColorRepository colors { get; private set; }
    }
    [ProtoContract]
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
                    _obj = Workspace.Load<IDominoProvider>(relativePath);
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
    }
    [ProtoContract]
    public class FieldNode : DocumentNode
    {
        // hier kommen mal so Sachen wie Feldanstoß rein
    }
    [ProtoContract]
    public class StructureNode : DocumentNode
    {

    }
    [ProtoContract]
    public class SpiralNode : DocumentNode
    {
        
    }
    [ProtoContract]
    public class CircleNode : DocumentNode
    {

    }
    [ProtoContract]
    public class LineNode : IDominoWrapper
    {

    }
    [ProtoContract]
    public class LineBlockNode : IDominoWrapper
    {

    }
    [ProtoContract]
    public class ArbitraryColorMatchedObject : IDominoWrapper
    {
        // so was wie Handsetting, bei dem aus einem Bild die Farben berechnet werden
    }
    [ProtoContract]
    public class FreeObject : IDominoWrapper
    {
        // irgendwas, wo die Anzahlen vom Benutzer vorgegeben werden,
    }
    [ProtoContract]
    public class AssemblyNode : IDominoWrapper
    {
        [ProtoMember(1)]
        public string relativePath;
        private DominoAssembly _obj;
        public DominoAssembly obj
        {
            get
            {
                if (_obj == null)
                    _obj = Workspace.Load<DominoAssembly>(relativePath);
                return _obj;
            }
        }
    }
    [ProtoContract]
    public class DominoConnector
    {
        [ProtoMember(1)]
        public IDominoWrapper next;
    }
    [ProtoContract]
    public abstract class Constraint
    {
        [ProtoMember(1)]
        bool isUpToDate;
    }
}
