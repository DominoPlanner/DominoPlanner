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
                string removeName = ((ProjectComposite)((MenuItem)sender).DataContext).Name;
                MessageBoxResult mbr = MessageBox.Show(string.Format("Delete {0}?", removeName), "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (mbr == MessageBoxResult.Yes)
                {
                    DocumentNode dn = (DocumentNode)((ProjectComposite)((MenuItem)sender).DataContext).Project.documentNode;
                    dn.parent.children.Remove(dn);
                    dn.parent.Save();
                    this.Children.Remove((ProjectComposite)((MenuItem)sender).DataContext);
                }
                Errorhandler.RaiseMessage(string.Format("{0} has been removed!", removeName), "Removed", Errorhandler.MessageType.Error);
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
                var proj = ((ProjectComposite)((MenuItem)sender).DataContext).Project;
                DocumentNode dn = (DocumentNode)proj.documentNode;
                RenameObject ro = new RenameObject(Path.GetFileName(dn.relativePath));
                if (ro.ShowDialog() == true)
                {
                    Workspace.CloseFile(proj.FilePath);
                    // jojo Tabs schließen
                    dn.relativePath = Path.Combine(Path.GetDirectoryName(dn.relativePath), ((RenameObjectVM)ro.DataContext).NewName);
                    ((ProjectComposite)((MenuItem)sender).DataContext).Name = Path.GetFileNameWithoutExtension(((RenameObjectVM)ro.DataContext).NewName);
                    dn.parent.Save();
                    File.Move(proj.FilePath, Path.Combine(Path.GetDirectoryName(proj.FilePath), ((RenameObjectVM)ro.DataContext).NewName));
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
