using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DominoPlanner.Usage
{
    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            var propertyInfo = this.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var attributes = propertyInfo.GetCustomAttributes(typeof(SettingsAttribute), true);
                if (attributes.Any())
                {
                    var attribute = (SettingsAttribute)attributes[0];
                    WriteSetting<object>(attribute.ClassName, attribute.PropertyName, propertyInfo.GetValue(this));
                }
            }
           
        }

        public ModelBase Self
        {
            get { return this; }
        }

        /*public BitmapImage ToBitmapSource(Bitmap source)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                source.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }*/
        public ModelBase()
        {
            var propertyInfos = this.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                var attributes = propertyInfo.GetCustomAttributes(typeof(SettingsAttribute), true).FirstOrDefault();
                if (attributes is SettingsAttribute settingsAttribute)
                {
                    object value = ReadSetting(settingsAttribute.ClassName, settingsAttribute.PropertyName);
                    if (value != null)
                    {
                        propertyInfo.SetValue(this, value);
                    }
                    else
                    {
                        if (settingsAttribute.DefaultValue.GetType() == propertyInfo.PropertyType)
                        {
                            propertyInfo.SetValue(this, settingsAttribute.DefaultValue);
                        }
                    }
                }
            }
        }
        public object ReadSetting(string vmname, string propertyname)
        {
            return UserSettingsSerializer.Instance.GetPropertyValue(vmname, propertyname);
        }
        public void WriteSetting<T>(string vmname, string propertyname, T value)
        {
            UserSettingsSerializer.Instance.AddPropertyValue(vmname, propertyname, value);
        }


    }
    public class SettingsAttribute : Attribute
    {
        public string ClassName{ get; set; }
        public string PropertyName { get; set; }
        public object DefaultValue { get; set; }

        public SettingsAttribute(string className, object defaultValue, [CallerMemberName] string propertyName = null)
        {
            ClassName = className;
            PropertyName = propertyName;
            DefaultValue = defaultValue;
        }
    }

    public class RelayCommand : ICommand
    {
        #region Properties

        private readonly Action<object> ExecuteAction;
        private readonly Predicate<object> CanExecuteAction;

        #endregion

        public RelayCommand(Action<object> execute) : this(execute, _ => true)
        {
        }
        public RelayCommand(Action<object> action, Predicate<object> canExecute)
        {
            ExecuteAction = action;
            CanExecuteAction = canExecute;
        }

        #region Methods

        public bool CanExecute(object parameter)
        {
            return CanExecuteAction(parameter);
        }
        public event EventHandler CanExecuteChanged;

        /*public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }*/

        public void Execute(object parameter)
        {
            ExecuteAction(parameter);
        }

        #endregion
    }
    
    public class ContextMenuEntry
    {
        public static MenuItem GenerateMenuItem(ContextMenuAttribute attr, MethodInfo mi, object reference)
        {
            MenuItem MI = new MenuItem();
            bool Activated = true;
            bool isMethod = !bool.TryParse(attr.Activated, out Activated);
            if (isMethod)
            {
                try
                {
                    var mipred = reference.GetType().GetRuntimeMethod(attr.Activated, new Type[] { });
                    var micoll = reference.GetType().GetRuntimeMethods();
                    Activated = (bool)mipred.Invoke(reference, new object[] { });
                }
                catch
                {
                    Activated = false;
                }
            }
            MI.Command = new RelayCommand(o => mi.Invoke(reference, new object[] { }));
            MI.Header = attr.Header;
            if (!string.IsNullOrEmpty(attr.ImageSource))
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                MI.Icon = new Image
                {
                    Source = new Bitmap(assets.Open(new Uri("avares://DominoPlanner.Usage/" + attr.ImageSource, UriKind.Absolute)))
            };
            }
            MI.IsVisible = attr.IsVisible;
            MI.IsEnabled = Activated;
            return MI;
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ContextMenuAttribute : Attribute
    {
        public string Activated { get; set; }

        public bool IsVisible { get; set; }

        public string ImageSource { get; set; }

        public string Header { get; set; }

        public int Index { get; set; }

        //public ICommand Command { get; set; }
        public ContextMenuAttribute(string header, string imageSource, 
            string activated, bool isVisible = true, int index = 0)
        {
            Header = Localizer._(header);
            ImageSource = imageSource;
            Activated = activated;
            IsVisible = isVisible;
            Index = index;
        }
        public ContextMenuAttribute(string header, string imageSource,
            bool activated = true, bool isVisible = true, int index = 0)
        {
            Header = Localizer._(header);
            ImageSource = imageSource;
            Activated = activated.ToString();
            IsVisible = isVisible;
            Index = index;
        }

    }
}
