using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using DominoPlanner.Usage.Serializer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DominoPlanner.Usage
{
    class ProjectListComposite : ProjectComposite
    {
        #region CTOR
        public ProjectListComposite(int id, string Name, string path, ProjectElement dominoAssembly) : base(dominoAssembly)
        {
            this.OwnID = id;
            this.Name = Name;
            _Children = new ObservableCollection<ProjectComposite>();
            Img = "/Icons/folder_txt.ico";
        }
        #endregion

        #region Prop
        public delegate bool CloseTabDelegate(ProjectComposite comp);
        public CloseTabDelegate closeTabDelegate;
        public delegate void OpenTabDelegate(ProjectComposite comp);
        public OpenTabDelegate openTabDelegate;

        private ObservableCollection<ProjectComposite> _Children;
        public ObservableCollection<ProjectComposite> Children
        {
            get { return _Children; }
            set
            {
                if (_Children != value)
                {
                    _Children = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ProjectComposite AddProject(ProjectComposite addObject)
        {
            _Children.Add(addObject);
            addObject.ParentProjectID = this.OwnID;
            return addObject;
        }
        #endregion

        #region Methods
        #region overrides
        public override string ToString()
        {
            return Name;
        }
        #endregion
        internal void RemoveMI_Object_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var proj = ((ProjectComposite)((MenuItem)sender).DataContext);
                string removeName = proj.Name;
                if (closeTabDelegate.Invoke(proj) &&
                    MessageBox.Show($"Remove file {removeName} from project {this.Name}?\nThe file won't be permanently deleted.", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DocumentNode dn = (DocumentNode)proj.Project.documentNode;
                    Workspace.CloseFile(proj.FilePath);
                    dn.parent.children.Remove(dn);
                    dn.parent.Save();
                    this.Children.Remove(proj);
                    Errorhandler.RaiseMessage(string.Format("{0} has been removed!", removeName), "Removed", Errorhandler.MessageType.Error);
                }
            }
            catch (Exception)
            {
                Errorhandler.RaiseMessage("Could not remove the project!", "Error", Errorhandler.MessageType.Error);
            }
        }

        internal void RenameMI_Object_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var proj = ((ProjectComposite)((MenuItem)sender).DataContext);
                DocumentNode dn = (DocumentNode)proj.Project.documentNode;
                RenameObject ro = new RenameObject(Path.GetFileName(dn.relativePath));
                if (closeTabDelegate.Invoke(proj) && ro.ShowDialog() == true)
                {
                    Workspace.CloseFile(proj.FilePath);
                    dn.relativePath = Path.Combine(Path.GetDirectoryName(dn.relativePath), ((RenameObjectVM)ro.DataContext).NewName);
                    proj.Name = Path.GetFileNameWithoutExtension(((RenameObjectVM)ro.DataContext).NewName);
                    string old_path = proj.FilePath;
                    proj.FilePath = Path.Combine(Path.GetDirectoryName(proj.FilePath), ((RenameObjectVM)ro.DataContext).NewName);
                    dn.parent.Save();
                    File.Move(old_path, proj.FilePath);
                    openTabDelegate(proj);
                }
            }
            catch 
            {
                Errorhandler.RaiseMessage("Renaming object failed!", "Error", Errorhandler.MessageType.Error);
            }
        }
        #endregion
    }
}
