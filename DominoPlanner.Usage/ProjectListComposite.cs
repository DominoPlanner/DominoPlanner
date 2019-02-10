using DominoPlanner.Core;
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

                    string picturepath = ((ProjectComposite)((MenuItem)sender).DataContext).Project.IcoPath;
                    string objectpath = ((ProjectComposite)((MenuItem)sender).DataContext).Project.FilePath;
                    
                    MessageBoxResult mbrRemoveAll = MessageBox.Show("Delete all files?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (mbrRemoveAll == MessageBoxResult.Yes)
                    {
                        try
                        {
                            //File.Delete(picturepath); jojo wenn das bild richtig ist
                            File.Delete(objectpath);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error removing projectfiles!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                MessageBox.Show(string.Format("{0} is remove!", removeName), "Removed", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not remove the project!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
