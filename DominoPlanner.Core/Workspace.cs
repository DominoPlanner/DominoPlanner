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
    public class Workspace
    {
        public List<Tuple<String, IWorkspaceLoadable>> openedFiles;
        // threadsicheres Singleton
        private static readonly Lazy<Workspace> _mySingleton = new Lazy<Workspace>(() => new Workspace());

        private string FileInWork = "";
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
        public static string AbsolutePathFromReference(string relativePath, IWorkspaceLoadable reference)
        {
            var referenceTuple = Instance.openedFiles.Find(x => x.Item2 == reference);
            if (referenceTuple != null && reference != null)
            {
                string directoryofreference = Path.GetDirectoryName(referenceTuple.Item1);
                relativePath = Path.GetFullPath(Path.Combine(directoryofreference, relativePath));
            }
            else if (reference != null)
            {
                string directoryofreference = Path.GetDirectoryName(Instance.FileInWork);
                relativePath = Path.GetFullPath(Path.Combine(directoryofreference, relativePath));
            }
            else if (new Uri(relativePath, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
            {
                // Path passt schon
            }
            
            else
            {
                throw new IOException("When not providing a reference, the path must be absolute");
            }
            return relativePath;
        }
        public static T Load<T>(string absolutePath) where T : IWorkspaceLoadable
        {
            return Load<T>(absolutePath, null);
        }
        public static T Load<T>(string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadable
        {
            var absPath = AbsolutePathFromReference(relativePath, reference);
            string old_file_in_work = Instance.FileInWork;
            try
            {
                Instance.FileInWork = absPath;
                var result = (T)Workspace.Instance.Find<T>(absPath);
                Console.WriteLine("Datei " + absPath + " öffnen");
                if (result == null)
                {
                    Console.WriteLine("Datei noch nicht geöffnet, deserialisieren");
                    using (var file = File.OpenRead(absPath))
                    {
                        result = Serializer.Deserialize<T>(file);
                    }
                    Instance.openedFiles.Add(new Tuple<string, IWorkspaceLoadable>(absPath, result));
                }
                return result;
            }
            finally
            {
                Instance.FileInWork = old_file_in_work;
            }

        }
        public static void Save(IWorkspaceLoadable obj, string filepath = "")
        {
            // prüfen, ob Datei schon offen
            var index = Instance.openedFiles.FindIndex(x => x.Item2 == obj);
            bool addToList = false;
            if (index != -1)
            {
                // Datei schon offen, soll sie unter einem anderen Namen gespeichert werden? 
                if (filepath != Instance.openedFiles[index].Item1 && filepath != "")
                {
                    obj = Serializer.DeepClone(obj);
                    addToList = true;
                }
                filepath = Instance.openedFiles[index].Item1;
            }
            else
            {
                // Datei neu erstellt
                if (!new Uri(filepath, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
                {
                    throw new IOException("If file will be newly created, save needs an absolute path");
                }
                addToList = true;
            }
            using (FileStream stream = new FileStream(filepath, FileMode.Create))
            {
                Serializer.Serialize(stream, obj);
                if (addToList)
                {
                    Instance.openedFiles.Add(new Tuple<string, IWorkspaceLoadable>(filepath, obj));
                }
            }
        }
        public static ObservableCollection<ImageFilter> LoadImageFilters<T>(string absolutePath) where T : IWorkspaceLoadImageFilter
        {
            return LoadImageFilters<T>(absolutePath, null);
        }
        public static ObservableCollection<ImageFilter> LoadImageFilters<T>(string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadImageFilter
        {
            relativePath = AbsolutePathFromReference(relativePath, reference);
            var result = (T)Workspace.Instance.Find<T>(relativePath);
            Console.WriteLine("Datei " + relativePath + " als Vorschau öffnen für ImageFilter");
            if (result == null)
            {
                Console.WriteLine("Datei noch nicht geöffnet, deserialisieren");
                using (var file = File.OpenRead(relativePath))
                {
                    return Serializer.Deserialize<IDominoProviderImageFilter>(file).ImageFilters;
                }
            }
            return result.ImageFilters;
        }
        public static int[] LoadColorList<T>(string absolutePath) where T: IWorkspaceLoadColorList
        {
            return LoadColorList<T>(absolutePath, null);
        }
        public static int[] LoadColorList<T>(string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadColorList
        {
            relativePath = AbsolutePathFromReference(relativePath, reference);
            var result = (T)Workspace.Instance.Find<T>(relativePath);
            Console.WriteLine("Datei " + relativePath + " als Vorschau öffnen für Farbenliste");
            if (result == null)
            {
                Console.WriteLine("Datei noch nicht geöffnet, deserialisieren");
                using (var file = File.OpenRead(relativePath))
                {
                    return Serializer.Deserialize<IDominoProviderPreview>(file).counts;
                }
            }
            return result.counts;
        }
        public static bool LoadEditingState<T>(string absolutePath) where T : IWorkspaceLoadColorList
        {
            return LoadEditingState<T>(absolutePath, null);
        }
        public static bool LoadEditingState<T>(string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadColorList
        {
            relativePath = AbsolutePathFromReference(relativePath, reference);
            var result = (T)Workspace.Instance.Find<T>(relativePath);
            Console.WriteLine("Datei " + relativePath + " als Vorschau öffnen für Editing State");
            if (result == null)
            {
                Console.WriteLine("Datei noch nicht geöffnet, deserialisieren");
                using (var file = File.OpenRead(relativePath))
                {
                    return Serializer.Deserialize<IDominoProviderPreview>(file).Editing;
                }
            }
            return result.Editing;
        }
        public object Find<T>(string AbsolutePath)
        {
            var result = openedFiles.Where(x => x.Item1 == AbsolutePath && x.Item2 is T);
            if (result.Count() == 0) return null;
            return result.First().Item2;
        }
    }
}
