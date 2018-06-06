using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace DominoPlanner.Usage.Serializer
{
    static class ProjectSerializer
    {
        public static bool CreateProject(string projectPath, string objectName)
        {
            try
            {
                XmlDocument newDoc = new XmlDocument();
                XmlElement root = newDoc.CreateElement("Projects");
                root.SetAttribute("Projectname", objectName);
                newDoc.AppendChild(root);
                newDoc.Save(Path.Combine(projectPath, "project.dpp"));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int AddProject(string projectPath, string nameWithExtension, string pictureName)
        {
            int curID = 0;
            try
            {
                XmlDocument newDoc = new XmlDocument();
                newDoc.Load(Path.Combine(projectPath, "project.dpp"));
                if (newDoc.HasChildNodes)
                {
                    XmlElement root = (XmlElement)newDoc.FirstChild;
                    if (root.HasChildNodes)
                        curID = Int32.Parse(root.LastChild.Attributes["ID"].Value) + 1;

                    XmlElement newProject = newDoc.CreateElement("Object");
                    newProject.SetAttribute("ID", curID.ToString());
                    newProject.SetAttribute("NameWithExtension", nameWithExtension);
                    newProject.SetAttribute("PictureName", pictureName);
                    root.AppendChild(newProject);
                }
                newDoc.Save(Path.Combine(projectPath, "project.dpp"));
                return curID;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static bool RemoveProject(string projectPath, int id)
        {
            try
            {
                XmlDocument currentDocument = new XmlDocument();
                currentDocument.Load(Path.Combine(projectPath, "project.dpp"));
                if (currentDocument.ChildNodes.Count == 1)
                {
                    XmlElement root = (XmlElement)currentDocument.ChildNodes[0];
                    for(int i = 0; i < root.ChildNodes.Count; i++)
                    {
                        XmlElement element = (XmlElement)root.ChildNodes[i];
                        if(element.GetAttribute("ID").Equals(id.ToString()))
                        {
                            root.RemoveChild(element);
                            currentDocument.Save(Path.Combine(projectPath, "project.dpp"));
                            return true;
                        }
                    }
                }
                else
                    return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<ProjectTransfer> GetProjects(string projectPath)
        {
            List<ProjectTransfer> returnList = new List<ProjectTransfer>();
            using (XmlReader reader = XmlReader.Create(Path.Combine(projectPath, "project.dpp")))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name.Equals("Object"))
                        {
                            string path = Path.Combine(projectPath, "Planner Files", reader.GetAttribute("NameWithExtension"));
                            string icopath = Path.Combine(projectPath, "Source Image", reader.GetAttribute("PictureName"));
                            returnList.Add(new ProjectTransfer(Convert.ToInt32(reader.GetAttribute("ID")), reader.GetAttribute("NameWithExtension"), path, icopath));
                        }
                    }
                }
            }
            return returnList;
        }
    }

    internal class ProjectTransfer
    {
        public ProjectTransfer(int id, string Name, string FilePath, string IcoPath)
        {
            this.Id = id;
            this.Name = Name;
            this.FilePath = FilePath;
            this.IcoPath = IcoPath;
            switch (Path.GetExtension(this.FilePath))
            {
                case ".dpcol":
                    CurrType = NodeType.ColorListNode;
                    break;
                case ".dpfd":
                    CurrType = NodeType.FieldNode;
                    break;
                case ".dpffd":
                    CurrType = NodeType.FreeHandFieldNode;
                    break;
                case ".dpst":
                    CurrType = NodeType.StructureNode;
                    break;
                default:
                    break;
            }
        }

        public int Id;
        public string Name;
        public string FilePath;
        public string IcoPath;
        public NodeType CurrType;
    }
}
