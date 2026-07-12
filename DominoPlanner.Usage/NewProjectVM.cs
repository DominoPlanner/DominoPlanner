using DominoPlanner.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using static DominoPlanner.Usage.Localizer;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace DominoPlanner.Usage
{
    public class NewProjectVM : ModelBase
    {
        #region CTOR
        public NewProjectVM()
        {
            SelectedPath = UserSettings.Instance.StandardProjectPath;
            sPath = UserSettings.Instance.StandardColorArray;
            ProjectName = _("New Project");
            rbStandard = true;
            rbCustom = false; //damit die Labels passen
            SelectFolder = new RelayCommand(o => { SelectProjectFolder(); });
            SelectColor = new RelayCommand(o => { SelectColorArray(); });
            StartClick = new RelayCommand(o => { CreateNewProject(); });
        }
        #endregion

        #region Methods
        private async void SelectProjectFolder()
        {
            try
            {
                var app = Avalonia.Application.Current;
                if (app?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                    if (topLevel == null) return;

                    var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                    {
                        Title = _("Select Project Folder"),
                        AllowMultiple = false
                    });

                    if (folders.Count > 0)
                    {
                        SelectedPath = folders[0].Path.LocalPath;
                    }
                }
            }
            catch (Exception) { }
        }

        private async void CreateNewProject()
        {
            try
            {
                if (Directory.Exists(Path.Combine(SelectedPath, ProjectName)))
                {
                    await Errorhandler.RaiseMessageWithParent<NewProject>(_("This folder already exists. Please choose another project name."), GetParticularString("Error on project creation", "Existing Folder"), Errorhandler.MessageType.Error);
                    return;
                }

                string projectpath = Path.Combine(SelectedPath, ProjectName);
                Directory.CreateDirectory(projectpath);
                Directory.CreateDirectory(Path.Combine(projectpath, "Source Image"));
                Directory.CreateDirectory(Path.Combine(projectpath, "Planner Files"));

                DominoAssembly main = new DominoAssembly();
                var projectfilename = ProjectName + "." +  Declares.ProjectExtension;
                main.Save(Path.Combine(projectpath, projectfilename));

                if (File.Exists(sPath))
                {
                    string colorPath = Path.Combine(SelectedPath, ProjectName, "Planner Files", $"colors.{Declares.ColorExtension}");
                    File.Copy(sPath, colorPath);
                    main.ColorPath = Path.Combine("Planner Files", "colors." + Declares.ColorExtension);
                }

                main.Save();

                await Errorhandler.RaiseMessageWithParent<NewProject>(string.Format(_("The project {0} has been created"), projectfilename), _("Project created"), Errorhandler.MessageType.Info);
                Close = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(_("Project creation failed: {0}"), e.ToString());
            }
        }

        private async void SelectColorArray()
        {
            try
            {
                var app = Avalonia.Application.Current;
                if (app?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                    if (topLevel == null) return;

                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = _("Select Color File"),
                        AllowMultiple = false,
                        FileTypeFilter = new[]
                        {
                            new FilePickerFileType(_("Color files")) { Patterns = new[] { $"*.{Declares.ColorExtension}" } },
                            new FilePickerFileType(_("All files")) { Patterns = new[] { "*" } }
                        }
                    });

                    if (files.Count > 0)
                    {
                        sPath = files[0].Path.LocalPath;
                    }
                }
            }
            catch (Exception) { }
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
                    ColorVisibility = true;
                else
                    ColorVisibility = false;
            }
        }
        private bool _ColorVisibility;
        public bool ColorVisibility
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
