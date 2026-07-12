using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DominoPlanner.Core;
using DominoPlanner.Usage.UserControls.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace DominoPlanner.Usage
{
    using static Localizer;
    class SetStandardVM : ModelBase
    {
        public SetStandardVM()
        {
            var StandardColorPath = UserSettings.Instance.StandardColorArray;
            SetStandardColor = new RelayCommand(o => { SetColorPath(); });
            SetStandardPath = new RelayCommand(o => { SetStandardPathOpen(); });
            ClearList = new RelayCommand(o => { ClearListMet(); });
            standardpath = UserSettings.Instance.StandardProjectPath;

            if (!File.Exists(StandardColorPath))
            {
                try
                {
                    string newPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "lamping.DColor");
                    File.Copy(newPath, StandardColorPath);
                }
                catch { }
            }

            ColorVM = new ColorListControlVM(StandardColorPath);

            Languages = Localizer.GetAllLocales().OrderBy(x => x.DisplayName).ToList();
            var Selected = Languages.Where(x => x.Name == Localizer.Language);
            if (Selected.Count() != 0)
            {
                CurrentLanguage = Selected.First();
            }
            else
            {
                CurrentLanguage = new CultureInfo("en-US");
            }
        }

        #region prop
        private ColorListControlVM _ColorVM;
        public ColorListControlVM ColorVM
        {
            get { return _ColorVM; }
            set
            {
                if (_ColorVM != value)
                {
                    _ColorVM = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<CultureInfo> Languages { get; set; }

        private CultureInfo culture;

        public CultureInfo CurrentLanguage
        {
            get { return culture; }
            set { culture = value;
                Localizer.Language = value.Name;
                Properties.Settings.Default.Save();
                RaisePropertyChanged();
            }
        }


        private string _standardpath;
        public string standardpath
        {
            get { return _standardpath; }
            set
            {
                if (_standardpath != value)
                {
                    _standardpath = value;
                    UserSettings.Instance.StandardProjectPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Method

        private async void SetStandardPathOpen()
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
                        Title = _("Select Standard Project Path"),
                        AllowMultiple = false
                    });

                    if (folders.Count > 0)
                    {
                        standardpath = folders[0].Path.LocalPath;
                    }
                }
            }
            catch (Exception) { }
        }

        private async void SetColorPath()
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
                            new FilePickerFileType(_("All color files")) { Patterns = new[] { $"*.{Declares.ColorExtension}", "*.clr", "*.farbe" } },
                            new FilePickerFileType(_("DominoPlanner 3.x color files")) { Patterns = new[] { $"*.{Declares.ColorExtension}" } },
                            new FilePickerFileType(_("DominoPlanner 2.x color files")) { Patterns = new[] { "*.clr" } },
                            new FilePickerFileType(_("Dominorechner color files")) { Patterns = new[] { "*.farbe" } }
                        }
                    });

                    if (files.Count > 0)
                    {
                        UserSettings.Instance.StandardColorArray = files[0].Path.LocalPath;
                        ColorVM = new ColorListControlVM(files[0].Path.LocalPath);
                    }
                }
            }
            catch (Exception) { }
        }

        private void ClearListMet()
        {
            ColorVM.ResetList();
        }
        #endregion

        #region Command
        private ICommand _SetStandardColor;
        public ICommand SetStandardColor { get { return _SetStandardColor; } set { if (value != _SetStandardColor) { _SetStandardColor = value; } } }

        private ICommand _SetStandardPath;
        public ICommand SetStandardPath { get { return _SetStandardPath; } set { if (value != _SetStandardPath) { _SetStandardPath = value; } } }

        private ICommand _ClearList;

        public ICommand ClearList { get { return _ClearList; } set { if (value != _ClearList) { _ClearList = value; } } }

        #endregion
    }
}