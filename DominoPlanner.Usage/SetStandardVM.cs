using Avalonia.Controls;
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
                    var share_path = MainWindowViewModel.ShareDirectory;
                    File.Copy(Path.Combine(share_path, "Resources", "lamping.DColor"), StandardColorPath);
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
            OpenFolderDialog ofd = new OpenFolderDialog { Directory = standardpath };
            var result = await ofd.ShowAsyncWithParent<SetStandardV>();
            if (result != null && !string.IsNullOrEmpty(result))
            {
                standardpath = result;
            }
        }

        private async void SetColorPath()
        {
            var StandardColorPath = UserSettings.Instance.StandardColorArray;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            try
            {
                openFileDialog.Filters = new System.Collections.Generic.List<FileDialogFilter>
                {
                    new FileDialogFilter() { Extensions = new System.Collections.Generic.List<string> { Declares.ColorExtension,  "clr", "farbe"}, Name = _("All color files")},
                    new FileDialogFilter() { Extensions = new System.Collections.Generic.List<string> { Declares.ColorExtension }, Name = _("DominoPlanner 3.x color files")},
                    new FileDialogFilter() { Extensions = new System.Collections.Generic.List<string> {"clr"}, Name = _("DominoPlanner 2.x color files")},
                    new FileDialogFilter() { Extensions = new System.Collections.Generic.List<string> {"farbe"}, Name = _("Dominorechner color files")},
                };
                openFileDialog.Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "lamping.DColor");
            }
            catch (Exception) { }
            var result = await openFileDialog.ShowAsyncWithParent<SetStandardV>();
            if (result != null && result.Length != 0)
            {
                var filename = result[0];
                if (File.Exists(filename))
                {
                    ColorRepository colorList;
                    int colorListVersion;
                    try
                    {
                        colorList = Workspace.Load<ColorRepository>(filename);
                        colorListVersion = 3;
                    }
                    catch
                    {
                        // Colorlist version 1 or 2
                        try
                        {
                            colorList = new ColorRepository(filename);
                            colorListVersion = 1;
                        }
                        catch
                        {
                            // file not readable
                            await Errorhandler.RaiseMessage(GetParticularString("When importing color list fails", "Color repository file is invalid"), _("Error"), Errorhandler.MessageType.Error);
                            return;
                        }
                    }
                    File.Delete(StandardColorPath);
                    if (colorListVersion == 3)
                    {
                        File.Copy(filename, StandardColorPath);
                    }
                    else if (colorListVersion != 0)
                    {
                        colorList.Save(StandardColorPath);
                    }
                }
                Workspace.CloseFile(StandardColorPath);
                ColorVM.Reload(StandardColorPath);
            }
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