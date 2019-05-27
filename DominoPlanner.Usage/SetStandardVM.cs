using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using DominoPlanner.Usage.UserControls.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DominoPlanner.Usage
{
    class SetStandardVM : ModelBase
    {
        public SetStandardVM()
        {
            SetStandardColor = new RelayCommand(o => { SetColorPath(); });
            SetStandardPath = new RelayCommand(o => { SetStandardPathOpen(); });
            SaveStandardPath = new RelayCommand(o => { SaveStandard(); });
            ClearList = new RelayCommand(o => { ClearListMet(); });
            standardpath = Properties.Settings.Default.StandardProjectPath;

            if (!File.Exists(Properties.Settings.Default.StandardColorArray))
            {
                try
                {
                    File.Copy(@".\Resources\lamping.DColor", Properties.Settings.Default.StandardColorArray);
                }
                catch { }
            }

            ColorVM = new ColorListControlVM(Properties.Settings.Default.StandardColorArray);
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

        private string _standardpath;
        public string standardpath
        {
            get { return _standardpath; }
            set
            {
                if (_standardpath != value)
                {
                    _standardpath = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Method
        private void SaveStandard()
        {
            Properties.Settings.Default.StandardProjectPath = standardpath;
            Properties.Settings.Default.Save();
        }

        private void SetStandardPathOpen()
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                standardpath = fbd.SelectedPath;
            }
        }

        private void SetColorPath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            try
            {
                openFileDialog.InitialDirectory = ColorVM.FilePath;
                openFileDialog.Filter = $"All color files |*{Properties.Resources.ColorExtension};*.clr;*.farbe|" +
                    $"DominoPlanner 3.x color files (*{Properties.Resources.ColorExtension})|*{Properties.Resources.ColorExtension}|" +
                    "DominoPlanner 2.x color files (*.clr)|*.clr|" +
                    "Dominorechner color files (*.farbe)|*.farbe|" +
                    "All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Resources");
            }
            catch (Exception) { }
            
            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    ColorRepository colorList;
                    int colorListVersion = 0;
                    try
                    {
                         colorList = Workspace.Load<ColorRepository>(openFileDialog.FileName);
                        colorListVersion = 3;
                    }
                    catch
                    {
                        // Colorlist version 1 or 2
                        try
                        {
                            colorList = new ColorRepository(openFileDialog.FileName);
                            colorListVersion = 1;
                        }
                        catch
                        {
                            // file not readable
                            Errorhandler.RaiseMessage("Color repository file is invalid", "Error", Errorhandler.MessageType.Error);
                            return;
                        }
                    }
                    File.Delete(Properties.Settings.Default.StandardColorArray);
                    if (colorListVersion == 3)
                    {
                        File.Copy(openFileDialog.FileName, Properties.Settings.Default.StandardColorArray);
                    }
                    else if (colorListVersion != 0)
                    {
                        colorList.Save(Properties.Settings.Default.StandardColorArray);
                    }
                }
                Workspace.CloseFile(Properties.Settings.Default.StandardColorArray);
                ColorVM.Reload(Properties.Settings.Default.StandardColorArray);
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

        private ICommand _SaveStandardPath;
        public ICommand SaveStandardPath { get { return _SaveStandardPath; } set { if (value != _SaveStandardPath) { _SaveStandardPath = value; } } }

        private ICommand _ClearList;
        public ICommand ClearList { get { return _ClearList; } set { if (value != _ClearList) { _ClearList = value; } } }

        #endregion
    }
}
