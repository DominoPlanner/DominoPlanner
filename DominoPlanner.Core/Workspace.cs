using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    public class PathResolution
    {
        public string RelativePath;
        public IWorkspaceLoadable reference;
        public string AbsolutePath;
        public bool isResolved;
        // only used if there is no IWorkspaceloadable that can be loaded as a reference (analogous to fileInWork)
        public string ParentPath;
    }
    public class Workspace : INotifyPropertyChanged
    {
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private ObservableCollection<Tuple<string, IWorkspaceLoadable>> _openedFiles;

        public ObservableCollection<Tuple<string, IWorkspaceLoadable>> openedFiles
        {
            get { return _openedFiles; }
            set { _openedFiles = value; RaisePropertyChanged(); }
        }
        public ObservableCollection<PathResolution> resolvedPaths;
        
        // threadsicheres Singleton
        private static readonly Lazy<Workspace> _mySingleton = new Lazy<Workspace>(() => new Workspace());

        private string FileInWork = "";
        private bool ReferenceReplaced = false;
        private Workspace() {
            openedFiles = new ObservableCollection<Tuple<string, IWorkspaceLoadable>>();
            resolvedPaths = new ObservableCollection<PathResolution>();
        }
        public delegate string FileReplacementDelegate(string filename, string caller);

        public static FileReplacementDelegate del;

        public event PropertyChangedEventHandler PropertyChanged;

        public static Workspace Instance
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
        public static string AbsolutePathFromReferenceLoseUpdate(string relativePath, IWorkspaceLoadable reference)
        {
            return AbsolutePathFromReference(ref relativePath, reference);
        }
        public static PathResolution FindResolution(string relativePath, IWorkspaceLoadable reference)
        {
            if (!string.IsNullOrEmpty(Instance.FileInWork))
            {
                var result = Instance.resolvedPaths.Where(x => x.ParentPath == Instance.FileInWork && x.RelativePath == relativePath).FirstOrDefault();
                if (result != null)
                    return result;
            }
            return Instance.resolvedPaths.Where(x => x.reference == reference && x.RelativePath == relativePath).FirstOrDefault();
        }
        public static string AbsolutePathFromReference(ref string relativePath, IWorkspaceLoadable reference)
        {
            if(relativePath.Contains("\\")) relativePath = relativePath.Replace("\\", "/");
            
            bool resolved = false;
            bool createResolution = false;
            string absolutePath = "";
            
            var referenceTuple = Instance.openedFiles.Where(x => x.Item2 == reference).FirstOrDefault();
            if (Path.IsPathRooted(relativePath))
            {
                absolutePath = relativePath;
                resolved = true;
            }
            else if (reference != null)
            {
                string basepath = "";
                if (referenceTuple != null)
                {
                     basepath = referenceTuple.Item1;
                }
                else if (!String.IsNullOrEmpty(Instance.FileInWork))
                {
                    basepath = Instance.FileInWork;
                }
                if (basepath != "")
                {
                    // try primitive resolution
                    string directoryofreference = Path.GetDirectoryName(basepath);
                    if (relativePath != null)
                    {
                        absolutePath = Path.GetFullPath(Path.Combine(directoryofreference, relativePath));
                        resolved = true;
                    }
                    if (relativePath == null || !File.Exists(absolutePath))
                    {
                        resolved = false;
                    }
                    var resolution = FindResolution(relativePath, reference);
                    // if the resolution was already computed, return that instead.
                    if (resolution != null)
                    {
                        if (!string.IsNullOrEmpty(resolution.AbsolutePath) && File.Exists(resolution.AbsolutePath))
                        {
                            relativePath = Workspace.MakeRelativePath(basepath, resolution.AbsolutePath);

                            resolution.isResolved = true;
                            return resolution.AbsolutePath;
                        }
                        else if (resolution.isResolved)
                        {
                            // resolution is marked as resolved, but file doesn't exist
                            resolution.isResolved = false;
                        }
                        if (resolved)
                        {
                            // primitive search was successful. Update the PathResolution
                            // this should only be called if a file was moved to its original filename while the program was running
                            resolution.AbsolutePath = absolutePath;
                            resolution.isResolved = resolved;
                        }
                    }
                    else
                    {
                        // no resolution exists yet - create a new one
                        createResolution = true;
                    }
                    if (createResolution)
                    {
                        Instance.resolvedPaths.Add(new PathResolution() { AbsolutePath = absolutePath, isResolved = resolved, reference = reference, RelativePath = relativePath, ParentPath = basepath });
                    }
                }
            }
            
            else
            {
                throw new IOException("When not providing a reference, the path must be absolute");
            }
            if (!resolved)
                throw new FileNotFoundException("File not found, update failed", absolutePath);
            return absolutePath;
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
            bool old_referenceReplaced = Instance.ReferenceReplaced;
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
                        if (!preview) Add(absolutePath, resultLoaded);
                    }
                    if (!preview && Instance.ReferenceReplaced)
                    {
                        Console.WriteLine($"Reference in file {absolutePath} replaced");
                        Save(resultLoaded, absolutePath);
                    }
                    return func.Invoke(resultLoaded);
                }
                Console.WriteLine("Datei bereits geöffnet, aus Workspace nehmen");
                
                return func2.Invoke(result);
            }
            finally
            {
                Instance.ReferenceReplaced = old_referenceReplaced;
                Instance.FileInWork = old_file_in_work;
            }
        }
        private static Out LoadInternal<FullType, LoadType, Out>
            (ref string relativePath, IWorkspaceLoadable reference, Func<LoadType, Out> func, Func<FullType, Out> func2) where FullType : IWorkspaceLoadable where LoadType : IWorkspaceLoadable
        {
            var absPath = AbsolutePathFromReference(ref relativePath, reference);
            return LoadInternal(absPath, func, func2);
        }
        public static T Load<T>(ref string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadable
        {
            return LoadInternal<T, T, T>(ref relativePath, reference, a => a, a => a);
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
                if (!Path.IsPathRooted(filepath))
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
            return LoadInternal<T, IDominoProviderImageFilter, ObservableCollection<ImageFilter>>(absolutePath, a => a.PrimaryImageTreatment?.ImageFilters, a => a.PrimaryImageTreatment?.ImageFilters);
        }
        public static ObservableCollection<ImageFilter> LoadImageFilters<T>(ref string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadImageFilter
        {
            return LoadImageFilters<T>(AbsolutePathFromReference(ref relativePath, reference));
        }
        public static Tuple<string, int[]> LoadColorList<T>(string absolutePath) where T: IWorkspaceLoadColorList
        {
            return LoadInternal<IDominoProvider, IDominoProviderPreview, Tuple<string, int[]>>(absolutePath,
                a => new Tuple<string, int[]>(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(absolutePath), a.ColorPath)), a.Counts),
                a => new Tuple<string, int[]>(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(absolutePath), a.ColorPath)), a.Counts)
                );
        } 
        public static Tuple<string, int[]> LoadColorList<T>(ref string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadColorList
        {
            return LoadColorList<T>(AbsolutePathFromReference(ref relativePath, reference));
        }
        public static bool LoadEditingState<T>(string absolutePath) where T : IWorkspaceLoadColorList
        {
            return LoadInternal<IDominoProvider, IDominoProviderPreview, bool>(absolutePath, a => a.Editing, a => a.Editing);
        }
        public static bool LoadEditingState<T>(ref string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadColorList
        {
            return LoadEditingState<T>(AbsolutePathFromReference(ref relativePath, reference));
        }
        public static bool LoadHasProtocolDefinition<T>(string absolutePath) where T : IWorkspaceLoadColorList
        {
            return LoadInternal<IDominoProvider, IDominoProviderPreview, bool>(absolutePath, a => a.HasProtocolDefinition, 
                a => a.HasProtocolDefinition);
        }
        public static bool LoadHasProtocolDefinition<T>(ref string relativePath, IWorkspaceLoadable reference) where T : IWorkspaceLoadColorList
        {
            return LoadHasProtocolDefinition<T>(AbsolutePathFromReference(ref relativePath, reference));
        }
        public static byte[] LoadThumbnailFromStream(Stream stream)
        {
            return Serializer.Deserialize<IDominoProviderThumbnail>(stream).Thumbnail;
        }
        public static void CloseFile(string path)
        {
            if (Path.IsPathRooted(path))
            {
                Instance.openedFiles.RemoveAll(x => x.Item1 == path);
            }
        }
        public static void CloseFile(string relativePath, IWorkspaceLoadable reference)
        {
            CloseFile(AbsolutePathFromReference(ref 
                relativePath, reference));
        }
        public static void CloseFile(IWorkspaceLoadable reference)
        {
            Instance.openedFiles.RemoveAll(x => x.Item2 == reference);
        }
        public static void Clear()
        {
            Instance.openedFiles = new ObservableCollection<Tuple<string, IWorkspaceLoadable>>();
        }
        public static object Find<T>(string AbsolutePath)
        {
            if (string.IsNullOrEmpty(AbsolutePath))
                throw new FileNotFoundException(AbsolutePath);
            var result = Instance.openedFiles.Where(x => Path.GetFullPath(x.Item1).Equals(Path.GetFullPath(AbsolutePath), StringComparison.OrdinalIgnoreCase) && x.Item2 is T);
            if (result.Count() == 0) return null;
            return result.First().Item2;
        }
        public static string Find(IWorkspaceLoadable obj) 
        {
            var result = Instance.openedFiles.Where(x => x.Item2 == obj);
            if (result.Count() == 0) return null;
            return result.First().Item1;
        }
        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath)) return toPath;
            if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");
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
    public static class ObservableCollectionExtensions
    {
        public static int RemoveAll<T>(
        this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }
        public static int FindIndex<T>(
        this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            for (int i = 0; i < coll.Count; i++)
            {
                if (condition.Invoke(coll[i])) return i;
            }
            return -1;
        }
    }
}
