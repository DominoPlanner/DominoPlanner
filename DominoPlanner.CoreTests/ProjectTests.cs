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
            string rootpath = Path.GetFullPath("tests/");
            
            DominoAssembly main = new DominoAssembly();
            main.Save(Path.Combine(rootpath, "main.DProject"));
            main.colorPath = "colors.DColor";
            var mainnode = new AssemblyNode(Path.Combine(rootpath, "main.DProject"));
            FieldParameters field1 = new FieldParameters(Path.GetFullPath("tests/field1.DObject"), "mountain.jpg", main.colorPath, 8, 8, 24, 8, 10000, Emgu.CV.CvEnum.Inter.Lanczos4, 
                new CieDe2000Comparison(), new Dithering(), new NoColorRestriction());
            field1.Save();
            new FieldNode("field1.DObject", main);
            StreamReader sr = new StreamReader(new FileStream("Structures.xml", FileMode.Open));
            XElement xml = XElement.Parse(sr.ReadToEnd());
            StructureParameters structure = new StructureParameters(Path.GetFullPath("tests/structure.DObject"), 
                "transparent.png", xml.Elements().ElementAt(1), 30000,
                main.colorPath, ColorDetectionMode.Cie1976Comparison, new Dithering(),
                AverageMode.Corner, new NoColorRestriction(), true);
            structure.Save();
            new StructureNode("structure.DObject", main);
            DominoAssembly sub = new DominoAssembly();
            string subpath = "sub.DProject";
            sub.Save(Workspace.AbsolutePathFromReference(ref subpath, main));
            sub.colorPath = "colors.DColor";
            new AssemblyNode("sub.DProject", main);
            FieldParameters sub1 = new FieldParameters(Path.GetFullPath("tests/field2.DObject"), "transparent.png", main.colorPath, 8, 8, 24, 8, 10000, Emgu.CV.CvEnum.Inter.Lanczos4,
                new CieDe2000Comparison(), new Dithering(), new NoColorRestriction());
            field1.Save();
            new FieldNode("field2.DObject", sub);
            new FieldNode("field1.DObject", sub);
            sub.Save();
            
            foreach (AssemblyNode node in main.children.Where(child => child is AssemblyNode))
            {
                node.obj.Save();
            }
            if (field1 == ((FieldNode)sub.children[1]).obj)
            {
                Console.WriteLine("references to field1 identical");
            }
            main.Save(mainnode.Path);
            PrintProjectStructure(mainnode, "");
            
            return mainnode;
        }
        public static void LoadProject()
        {
            Workspace.Clear();
            string rootpath = Path.GetFullPath("tests/");
            var mainnode = new AssemblyNode(Path.Combine(rootpath, "main.DProject"));
            PrintProjectStructure(mainnode, "");
            if (((FieldNode)mainnode.obj.children[0]).obj == ((FieldNode)((AssemblyNode)mainnode.obj.children[2]).obj.children[1]).obj)
            {
                Console.WriteLine("references to field1 identical");
            }
        }
        public static void PrintProjectStructure(AssemblyNode project, string indentation)
        {
            Console.WriteLine(indentation + project.Path + ", id: " + RuntimeHelpers.GetHashCode(project));

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
        /*public static void RenameObject(AssemblyNode project, string oldpath, string newpath)
        {
            foreach (IDominoWrapper obje in project.obj.children)
            {
                switch (obje)
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
        }*/
    }
}
