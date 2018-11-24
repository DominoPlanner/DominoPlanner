using System;
using System.Collections.Generic;
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
            }
        }
        public List<Tuple<String, object>> openedFiles;
        // threadsicheres Singleton
        private static readonly Lazy<Workspace> _mySingleton = new Lazy<Workspace>(() => new Workspace());

        private Workspace() {
            openedFiles = new List<Tuple<string, object>>();
        }

        public static Workspace Instance
        {
            get
            {
                return _mySingleton.Value;
            }
        }
        public object Find(string path)
        {
            var result = openedFiles.Where(x => x.Item1 == MakePathAbsolute(path));
            if (result.Count() == 0) return null;
            return result.First().Item2;
        }
        public void AddToWorkspace(string path, object obj)
        {
            openedFiles.Add(new Tuple<string, object>(MakePathAbsolute(path), obj));
        }
        public string MakePathAbsolute(string path)
        {
            Uri path2 = new Uri(path, UriKind.RelativeOrAbsolute);
            if (!path2.IsAbsoluteUri) path2 = new Uri(System.IO.Path.Combine(root_path, path));
            return path2.OriginalString;
        }
    }
}
