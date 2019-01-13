using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    public class Workspace
    {
        // der Root-Path muss absolut sein
        private string _root_path;
        public string root_path
        {
            get
            {
                return _root_path;
            }
            set
            {
                Uri test = new Uri(value, UriKind.RelativeOrAbsolute);
                if (test.IsAbsoluteUri) _root_path = value;
                else throw new InvalidOperationException("Der Roodpfad muss absolut sein");
                openedFiles = new List<Tuple<string, IWorkspaceLoadable>>();
            }
        }
        public List<Tuple<String, IWorkspaceLoadable>> openedFiles;
        // threadsicheres Singleton
        private static readonly Lazy<Workspace> _mySingleton = new Lazy<Workspace>(() => new Workspace());

        private Workspace() {
            openedFiles = new List<Tuple<string, IWorkspaceLoadable>>();
        }

        public static Workspace Instance
        {
            get
            {
                return _mySingleton.Value;
            }
        }
        public static T Load<T>(string path) where T : IWorkspaceLoadable
        {
            path = Workspace.Instance.MakePathAbsolute(path);
            var result = (T)Workspace.Instance.Find<T>(path);
            Console.WriteLine("Datei " + path + " öffnen");
            if (result == null)
            {
                Console.WriteLine("Datei noch nicht geöffnet, deserialisieren");
                using (var file = File.OpenRead(path))
                {
                    result = Serializer.Deserialize<T>(file);
                }
                Workspace.Instance.AddToWorkspace(path, result);
            }
            return result;
        }
        public static void Save(IWorkspaceLoadable obj, string filepath)
        {
            filepath = Workspace.Instance.MakePathAbsolute(filepath);
            using (FileStream stream = new FileStream(filepath, FileMode.Create))
            {
                Serializer.Serialize(stream, obj);
                if (Workspace.Instance.openedFiles.FindIndex(ob => ob.Item1 == filepath) == -1)
                {
                    Workspace.Instance.openedFiles.Add(new Tuple<string, IWorkspaceLoadable>(filepath, obj));
                }
            }
        }
        public static int[] LoadColorList<T>(string path) where T : IWorkspaceLoadColorList
        {
            path = Workspace.Instance.MakePathAbsolute(path);
            var result = (T)Workspace.Instance.Find<T>(path);
            Console.WriteLine("Datei " + path + " als Vorschau öffnen");
            if (result == null)
            {
                Console.WriteLine("Datei noch nicht geöffnet, deserialisieren");
                using (var file = File.OpenRead(path))
                {
                    return Serializer.Deserialize<IDominoProviderPreview>(file).counts;
                }
            }
            return result.counts;
        }
        public static bool LoadEditingState<T>(string path) where T : IWorkspaceLoadColorList
        {
            path = Workspace.Instance.MakePathAbsolute(path);
            var result = (T)Workspace.Instance.Find<T>(path);
            Console.WriteLine("Datei " + path + " als Vorschau öffnen für Editing State");
            if (result == null)
            {
                Console.WriteLine("Datei noch nicht geöffnet, deserialisieren");
                using (var file = File.OpenRead(path))
                {
                    return Serializer.Deserialize<IDominoProviderPreview>(file).Editing;
                }
            }
            return result.Editing;
        }
        public object Find<T>(string path)
        {
            var result = openedFiles.Where(x => x.Item1 == MakePathAbsolute(path) && x.Item2 is T);
            if (result.Count() == 0) return null;
            return result.First().Item2;
        }
        public void AddToWorkspace(string path, IWorkspaceLoadable obj)
        {
            openedFiles.Add(new Tuple<string, IWorkspaceLoadable>(MakePathAbsolute(path), obj));
        }
        public string MakePathAbsolute(string path)
        {
            Uri path2 = new Uri(path, UriKind.RelativeOrAbsolute);
            if (!path2.IsAbsoluteUri) path2 = new Uri(System.IO.Path.Combine(root_path, path));
            return path2.OriginalString;
        }
    }
}
