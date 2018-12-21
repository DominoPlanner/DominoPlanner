using DominoPlanner.Usage.Serializer;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace DominoPlanner.Usage
{
    public class NewProjectVM : ModelBase
    {
        #region CTOR
        public NewProjectVM()
        {
            SelectedPath = Properties.Settings.Default.StandardProjectPath;
            sPath = Properties.Settings.Default.StandardColorArray;
            ProjectName = "New Project";
            rbStandard = true;
            rbCustom = false; //damit die Labels passen
            SelectFolder = new RelayCommand(o => { SelectProjectFolder(); });
            SelectColor = new RelayCommand(o => { SelectColorArray(); });
            StartClick = new RelayCommand(o => { CreateNewProject(); });
        }
        #endregion

        #region Methods
        private void SelectProjectFolder()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedPath = fbd.SelectedPath;
            }
        }

        private void CreateNewProject()
        {
            try
            {
                if (Directory.Exists(Path.Combine(SelectedPath, ProjectName)))
                {
                    MessageBox.Show("This Folder already exists. Please choose another Project-Name.", "Existing Folder", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Directory.CreateDirectory(Path.Combine(SelectedPath, ProjectName));
                Directory.CreateDirectory(Path.Combine(SelectedPath, ProjectName, "Source Image"));
                Directory.CreateDirectory(Path.Combine(SelectedPath, ProjectName, "Planner Files"));
                bool create = ProjectSerializer.CreateProject(Path.Combine(SelectedPath, ProjectName), ProjectName);
                if (create)
                {
                    if(ProjectSerializer.AddProject(Path.Combine(SelectedPath, ProjectName), "colors.dpcol", @".\Icons\colorLine.ico") == -1)
                    {
                        create = false;
                    }
                }
                if (File.Exists(sPath))
                    File.Copy(sPath, Path.Combine(SelectedPath, ProjectName, "Planner Files", "colors.dpcol"));
                if (create)
                {
                    MessageBox.Show("Create new project", "Created", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    Close = true;
                }
                else
                {
                    MessageBox.Show("Could not create the new project.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }

        private void SelectColorArray()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            try
            {
                openFileDialog.InitialDirectory = sPath;
                openFileDialog.Filter = "domino color files (*.DColor)|*.DColor|All files (*.*)|*.*";
            }
            catch (Exception) { }

            if (openFileDialog.ShowDialog() == true)
            {
                sPath = openFileDialog.FileName;
            }
        }
        #endregion

        #region Prop
        private bool _Close;
        public bool Close
        {
            get { return _Close; }
            set
            {
                if (_Close != value)
                {
                    _Close = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _sPath;
        public string sPath
        {
            get { return _sPath; }
            set
            {
                if (_sPath != value)
                {
                    _sPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SelectedPath;
        public string SelectedPath
        {
            get { return _SelectedPath; }
            set
            {
                if (_SelectedPath != value)
                {
                    _SelectedPath = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _ProjectName;
        public string ProjectName
        {
            get { return _ProjectName; }
            set
            {
                if (_ProjectName != value)
                {
                    _ProjectName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _rbStandard;
        public bool rbStandard
        {
            get { return _rbStandard; }
            set
            {
                if (_rbStandard != value)
                {
                    _rbStandard = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _rbCustom;
        public bool rbCustom
        {
            get { return _rbCustom; }
            set
            {
                if (_rbCustom != value)
                {
                    _rbCustom = value;
                    RaisePropertyChanged();
                }
                if (value)
                    ColorVisibility = Visibility.Visible;
                else
                    ColorVisibility = Visibility.Hidden;
            }
        }
        private Visibility _ColorVisibility;
        public Visibility ColorVisibility
        {
            get { return _ColorVisibility; }
            set
            {
                if (_ColorVisibility != value)
                {
                    _ColorVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Command
        private ICommand _SelectFolder;
        public ICommand SelectFolder { get { return _SelectFolder; } set { if (value != _SelectFolder) { _SelectFolder = value; } } }

        private ICommand _SelectColor;
        public ICommand SelectColor { get { return _SelectColor; } set { if (value != _SelectColor) { _SelectColor = value; } } }

        private ICommand _StartClick;
        public ICommand StartClick { get { return _StartClick; } set { if (value != _StartClick) { _StartClick = value; } } }
        #endregion
    }
}
