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
        private static Workspace Instance
        {
            get
            {
                return _mySingleton.Value;
            }
        }
        private static void Add(string absolutePath, IWorkspaceLoadable obj)
        {
            if (Find<IWorkspaceLoadable>(absolutePath) == null)
            {
                Instance.openedFiles.Add(new Tuple<string, IWorkspaceLoadable>(absolutePath, obj));
                Console.WriteLine($"File {absolutePath} added to workspace");
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
            /*return Load<T>(absolutePath, null);*/
            return LoadInternal<T, T, T>(absolutePath, a => a, a => a);
        }
        private static Out LoadInternal<FullType, LoadType, Out>
            (string absolutePath, Func<LoadType, Out> func, Func<FullType, Out> func2) where FullType : IWorkspaceLoadable where LoadType : IWorkspaceLoadable
        {
            bool preview = typeof(LoadType) != typeof(FullType);
            var result = (FullType)Find<FullType>(absolutePath);
            string old_file_in_work = Instance.FileInWork;
            Console.WriteLine($"Datei {absolutePath} {(preview ? "als Vorschau" : "vollwertig")} öffnen");
            try
            {
                if (!preview) Instance.FileInWork = absolutePath;
                if (result == null)
                {
                    LoadType resultLoaded;
                    Console.WriteLine($"Datei noch nicht geöffnet, als {typeof(LoadType)} deserialisieren");
                    using (var file = File.OpenRead(absolutePath))
                    {
                        resultLoaded = Serializer.Deserialize<LoadType>(file);
                    }
                    if (!preview) Add(absolutePath, resultLoaded);
                    return func.Invoke(resultLoaded);
                }
                Console.WriteLine("Datei bereits geöffnet, aus Workspace nehmen");
                return func2.Invoke(result);
            }
            finally
            {
                Instance.FileInWork = old_file_in_work;
            }
        }
        private static Out LoadInternal<FullType, LoadType, Out>
            (string relativePath, IWorkspaceLoadable reference, Func<LoadType, Out> func, Func<FullType, Out> func2) where FullType : IWorkspaceLoadable where LoadType : IWorkspaceLoadable
        {
            var absPath = AbsolutePathFromReference(relativePath, reference);
            return LoadInternal(absPath, func, func2);
        }
        public static T Load<T>(string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadable
        {
            return LoadInternal<T, T, T>(relativePath, reference, a => a, a => a);
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
                    Add(filepath, obj);
                }
            }
        }
        public static ObservableCollection<ImageFilter> LoadImageFilters<T>(string absolutePath) where T : IWorkspaceLoadImageFilter
        {
            return LoadInternal<T, IDominoProviderImageFilter, ObservableCollection<ImageFilter>>(absolutePath, a => a.ImageFilters, a => a.ImageFilters);
        }
        public static ObservableCollection<ImageFilter> LoadImageFilters<T>(string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadImageFilter
        {
            return LoadImageFilters<T>(AbsolutePathFromReference(relativePath, reference));
        }
        public static Tuple<string, int[]> LoadColorList<T>(string absolutePath) where T: IWorkspaceLoadColorList
        {
            return LoadInternal<IDominoProvider, IDominoProviderPreview, Tuple<string, int[]>>(absolutePath,
                a => new Tuple<string, int[]>(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(absolutePath), a.ColorPath)), a.counts),
                a => new Tuple<string, int[]>(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(absolutePath), a.ColorPath)), a.counts)
                );
        }
        public static Tuple<string, int[]> LoadColorList<T>(string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadColorList
        {
            return LoadColorList<T>(AbsolutePathFromReference(relativePath, reference));
        }
        public static bool LoadEditingState<T>(string absolutePath) where T : IWorkspaceLoadColorList
        {
            return LoadInternal<IDominoProvider, IDominoProviderPreview, bool>(absolutePath, a => a.Editing, a => a.Editing);
        }
        public static bool LoadEditingState<T>(string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadColorList
        {
            return LoadEditingState<T>(AbsolutePathFromReference(relativePath, reference));
        }
        public static bool LoadHasProtocolDefinition<T>(string absolutePath) where T : IWorkspaceLoadColorList
        {
            return LoadInternal<IDominoProvider, IDominoProviderPreview, bool>(absolutePath, a => a.hasProtocolDefinition, 
                a => a.hasProcotolDefinition);
        }
        public static bool LoadHasProtocolDefinition<T>(string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadColorList
        {
            return LoadHasProtocolDefinition<T>(AbsolutePathFromReference(relativePath, reference));
        }
        public static void CloseFile(string path)
        {
            if (new Uri(path, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
            {
                Instance.openedFiles.RemoveAll(x => x.Item1 == path);
            }
        }
        public static void CloseFile(string relativePath, IWorkspaceLoadable reference)
        {
            CloseFile(AbsolutePathFromReference(relativePath, reference));
        }
        public static void CloseFile(IWorkspaceLoadable reference)
        {
            Instance.openedFiles.RemoveAll(x => x.Item2 == reference);
        }
        public static void Clear()
        {
            Instance.openedFiles = new List<Tuple<string, IWorkspaceLoadable>>();
        }
        public static object Find<T>(string AbsolutePath)
        {
            var result = Instance.openedFiles.Where(x => x.Item1 == AbsolutePath && x.Item2 is T);
            if (result.Count() == 0) return null;
            return result.First().Item2;
        }
        public static string Find(IWorkspaceLoadable obj) 
        {
            var result = Instance.openedFiles.Where(x => x.Item2 == obj);
            if (result.Count() == 0) return null;
            return result.First().Item1;
        }
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
