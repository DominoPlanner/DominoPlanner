
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DominoPlanner.Usage.Serializer
{
    static class OpenProjectSerializer
    {
        static string path = Properties.Settings.Default.OpenProjectList;

        public static bool Create()
        {
            try
            {
                XmlDocument newDoc = new XmlDocument();
                XmlElement root = newDoc.CreateElement("Projects");
                newDoc.AppendChild(root);
                newDoc.Save(path);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public static OpenProject AddOpenProject(string projectName, string folderPath)
        {
            XmlDocument newDoc = new XmlDocument();
            newDoc.Load(path);
            if (newDoc.HasChildNodes)
            {
                XmlElement root = (XmlElement)newDoc.FirstChild;
                int curID = 0;
                if (root.HasChildNodes)
                    curID =  Int32.Parse(root.LastChild.Attributes["ID"].Value) + 1;

                XmlElement newProject = newDoc.CreateElement("Project");
                newProject.SetAttribute("ID", curID.ToString());
                newProject.SetAttribute("Projectname", projectName);
                newProject.SetAttribute("FolderPath", folderPath);
                root.AppendChild(newProject);
                newDoc.Save(path);
                return new OpenProject(curID, projectName, folderPath);
            }
            return null;
        }

        public static bool RemoveOpenProject(int id)
        {
            try
            {
                XmlDocument currDocument = new XmlDocument();
                if (File.Exists(path))
                {
                    currDocument.Load(path);
                    XmlElement root = (XmlElement)currDocument.FirstChild;
                    if (root != null && root.HasChildNodes)
                    {
                        foreach (XmlElement curElement in root.ChildNodes)
                        {
                            if(Int32.Parse(curElement.Attributes["ID"].Value) == id)
                            {
                                root.RemoveChild(curElement);
                                currDocument.Save(path);
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<OpenProject> GetOpenProjects()
        {
            try
            {
                XmlDocument currDocument = new XmlDocument();
                List<OpenProject> openProjects = new List<OpenProject>();
                if (File.Exists(path))
                {
                    currDocument.Load(path);
                    XmlElement root = (XmlElement)currDocument.FirstChild;
                    if (root != null && root.HasChildNodes)
                    {
                        foreach (XmlElement curElement in root.ChildNodes)
                            openProjects.Add(new OpenProject(Int32.Parse(curElement.Attributes["ID"].Value), curElement.Attributes["Projectname"].Value, curElement.Attributes["FolderPath"].Value));
                    }
                }
                return openProjects;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static void RenameProject(string name, string new_name)
        {
            try
            {
                XmlDocument currDocument = new XmlDocument();
                if (File.Exists(path))
                {
                    currDocument.Load(path);
                    XmlElement root = (XmlElement)currDocument.FirstChild;
                    if (root != null && root.HasChildNodes)
                    {
                        foreach (XmlElement curElement in root.ChildNodes)
                        {
                            if (string.Equals(curElement.Attributes["FolderPath"].Value, Path.GetDirectoryName(name), StringComparison.OrdinalIgnoreCase)
                                && string.Equals(curElement.Attributes["Projectname"].Value, Path.GetFileNameWithoutExtension(name), StringComparison.OrdinalIgnoreCase))
                            {
                                curElement.SetAttribute("FolderPath", Path.GetDirectoryName(new_name));
                                curElement.SetAttribute("Projectname", Path.GetFileNameWithoutExtension(new_name));
                                currDocument.Save(path);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        public static int GetProjectID(string name)
        {
            try
            {
                XmlDocument currDocument = new XmlDocument();
                if (File.Exists(path))
                {
                    currDocument.Load(path);
                    XmlElement root = (XmlElement)currDocument.FirstChild;
                    if (root != null && root.HasChildNodes)
                    {
                        foreach (XmlElement curElement in root.ChildNodes)
                        {
                            if (string.Equals(curElement.Attributes["FolderPath"].Value, Path.GetDirectoryName(name), StringComparison.OrdinalIgnoreCase)
                                && string.Equals(curElement.Attributes["Projectname"].Value, Path.GetFileNameWithoutExtension(name), StringComparison.OrdinalIgnoreCase))
                            {
                                return int.Parse(curElement.Attributes["ID"].Value);
                            }
                        }
                    }
                }
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }

    public class OpenProject
    {
        public OpenProject(int id, string name, string path)
        {
            this.id = id;
            this.name = name;
            this.path = path;
        }

        public int id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
    }
}
