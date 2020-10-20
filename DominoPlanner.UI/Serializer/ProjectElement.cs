using System.IO;
using DominoPlanner.Core;

namespace DominoPlanner.UI.Serializer
{
    public class ProjectElement
    {
        public ProjectElement(string FilePath, string IcoPath, IDominoWrapper documentNode)
        {
            this.documentNode = documentNode;
            this.FilePath = FilePath;
            this.IcoPath = IcoPath;
            //switch (Path.GetExtension(this.FilePath).ToUpper())
            //{
            //    case ".DCOLOR":
            //        CurrType = NodeType.ColorListNode;
            //        break;
            //    case ".DOBJECT":
            //        CurrType = NodeType.ProjectNode;
            //        break;
            //    default:
            //        break;
            //}
        }

        public IDominoWrapper documentNode;
        public int Id;
        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FilePath);
            }
        }
        public string FilePath { get; set; }
        public string IcoPath;
        //public NodeType CurrType;
    }
}
