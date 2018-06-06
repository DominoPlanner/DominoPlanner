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
        public ProjectListComposite(int id, string Name, string path) : base(id, 0, Name, "", path, NodeType.ProjectNode)
        {
            _Children = new ObservableCollection<ProjectComposite>();
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

        internal void RemoveMI_Object_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                string removeName = ((ProjectComposite)((MenuItem)sender).DataContext).Name;
                MessageBoxResult mbr = MessageBox.Show(string.Format("Delete {0}?", removeName), "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (mbr == MessageBoxResult.Yes)
                {
                    string picturepath = ((ProjectComposite)((MenuItem)sender).DataContext).PicturePath;
                    string objectpath = ((ProjectComposite)((MenuItem)sender).DataContext).FilePath;
                    if (ProjectSerializer.RemoveProject(FilePath, ((ProjectComposite)((MenuItem)sender).DataContext).OwnID))
                        Children.Remove((ProjectComposite)((MenuItem)sender).DataContext);
                    MessageBoxResult mbrRemoveAll = MessageBox.Show("Delete all files?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if(mbrRemoveAll == MessageBoxResult.Yes)
                    {
                        try
                        {
                            File.Delete(picturepath);
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
