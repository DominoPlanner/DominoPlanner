using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DominoPlanner.Usage
{
    public class UserSettings : ModelBase
    {
        #region CTOR
        private UserSettings()
        {

        }
        #endregion

        #region Singleton
        private static UserSettings _Instance;
        public static UserSettings Instance
        {
            get 
            {
                if(_Instance == null)
                {
                    _Instance = new UserSettings();
                }
                return _Instance;
            }
        }

        #endregion

        #region PROPERTIES
        private static string _AppDataPath = string.Empty;
        public static string AppDataPath
        {
            get 
            {
                if(string.IsNullOrWhiteSpace(_AppDataPath))
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _AppDataPath = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        _AppDataPath = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local", "share");
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        _AppDataPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Library", "Application Support");
                    }
                    else
                    {
                        throw new NotImplementedException("Unknown OS Platform");
                    }
                    _AppDataPath = Path.Combine(_AppDataPath, "DominoPlanner");
                }
                try
                {
                    if (!Directory.Exists(_AppDataPath))
                    {
                        Directory.CreateDirectory(_AppDataPath);
                    }
                }catch(Exception ex) { }
                return _AppDataPath; 
            }
        }

        public static string UserSettingsPath
        {
            get
            {
                return Path.Combine(AppDataPath, "UserSettings.xml");
            }
        }

        private string _StandardProjectPath;

        [SettingsAttribute("UserSettings")]
        public string StandardProjectPath
        {
            get { return _StandardProjectPath; }
            set
            {
                if (_StandardProjectPath != value)
                {
                    _StandardProjectPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StandardColorArray;
        [SettingsAttribute("UserSettings")]
        public string StandardColorArray
        {
            get { return _StandardColorArray; }
            set
            {
                if (_StandardColorArray != value)
                {
                    _StandardColorArray = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StructureTemplates;
        [SettingsAttribute("UserSettings")]
        public string StructureTemplates
        {
            get { return _StructureTemplates; }
            set
            {
                if (_StructureTemplates != value)
                {
                    _StructureTemplates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _OpenProjectList;
        [SettingsAttribute("UserSettings")]
        public string OpenProjectList
        {
            get { return _OpenProjectList; }
            set
            {
                if (_OpenProjectList != value)
                {
                    _OpenProjectList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
    }
}
