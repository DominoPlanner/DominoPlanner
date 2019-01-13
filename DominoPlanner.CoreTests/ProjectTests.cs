using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DominoPlanner.CoreTests
{
    public class ProjectTests
    {
        public static AssemblyNode CreateProject()
        {
            //Workspace.Instance.root_path = 
            
            DominoAssembly main = new DominoAssembly("colors.DColor");
            main.Save("main.DProject");
            var mainnode = new AssemblyNode("main.DProject");
            main.children = new List<IDominoWrapper>();
            FieldParameters field1 = new FieldParameters("mountain.jpg", main.colorPath, 8, 8, 24, 8, 10000, Emgu.CV.CvEnum.Inter.Lanczos4, 
                new CieDe2000Comparison(), new Dithering(), new NoColorRestriction());
            field1.Save("field1.DObject");
            main.children.Add(new FieldNode("field1.DObject"));
            StreamReader sr = new StreamReader(new FileStream("Structures.xml", FileMode.Open));
            XElement xml = XElement.Parse(sr.ReadToEnd());
            StructureParameters structure = new StructureParameters("transparent.png", xml.Elements().ElementAt(1), 30000,
                main.colorPath, ColorDetectionMode.Cie1976Comparison, new Dithering(),
                AverageMode.Corner, new NoColorRestriction(), true);
            structure.Save("structure.DObject");
            main.children.Add(new StructureNode("structure.DObject"));
            DominoAssembly sub = new DominoAssembly("colors.DColor");
            sub.Save("sub.DProject");
            FieldParameters sub1 = new FieldParameters("transparent.png", main.colorPath, 8, 8, 24, 8, 10000, Emgu.CV.CvEnum.Inter.Lanczos4,
                new CieDe2000Comparison(), new Dithering(), new NoColorRestriction());
            field1.Save("field2.DObject");
            sub.children.Add(new FieldNode("field2.DObject"));
            sub.children.Add(new FieldNode("field.DObject"));
            main.children.Add(new AssemblyNode("sub.DProject"));
            foreach (AssemblyNode node in main.children.Where(child => child is AssemblyNode))
            {
                node.obj.Save(node.relativePath);
            }
            main.Save(mainnode.relativePath);
            PrintProjectStructure(mainnode, "");
            return mainnode;
        }
        public static void PrintProjectStructure(AssemblyNode project, string indentation)
        {
            Console.WriteLine(indentation + project.relativePath + ", id: " + RuntimeHelpers.GetHashCode(project));

            Console.WriteLine(indentation + "  "+ project.obj.colorPath + ", id: " + RuntimeHelpers.GetHashCode(project.obj.colors));
            foreach (IDominoWrapper obj in project.obj.children)
            {
                switch (obj)
                {
                    case DocumentNode doc:
                        Console.WriteLine(indentation  + "  " + doc.relativePath + ", id: " + RuntimeHelpers.GetHashCode(obj));
                        break;
                    case AssemblyNode assy:
                        PrintProjectStructure(assy, indentation + "  ");
                        break;
                }
            }
        }
        public static void RenameObject(AssemblyNode project, string oldpath, string newpath)
        {
            foreach (IDominoWrapper obj in project.obj.children)
            {
                switch (obj)
                {
                    case DocumentNode doc:
                        if (doc.relativePath == oldpath)
                        {
                            doc.relativePath = newpath;
                        }
                        break;
                    case AssemblyNode assy:
                        if (assy.relativePath == oldpath)
                        {
                            assy.relativePath = newpath;
                        }
                        break;
                }
            }
            string abs_old_path = Workspace.Instance.MakePathAbsolute(oldpath);
            var obj = Workspace.Instance.openedFiles.Find(x => x.Item1 == abs_old_path);
            for (int i = 0; i < Workspace.Instance.openedFiles.Count; i++)
            {
                var current = Workspace.Instance.openedFiles[i];
                if (current.Item1 == abs_old_path)
                {
                    Workspace.Instance.openedFiles[i] =
                        new Tuple<string, IWorkspaceLoadable>(Workspace.Instance.MakePathAbsolute(oldpath), current.Item2);
                }
            }
        }
    }
}
