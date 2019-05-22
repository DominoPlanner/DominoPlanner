using DominoPlanner.Usage.UserControls.ViewModel;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage
{
    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public ModelBase Self
        {
            get { return this; }
        }

        public BitmapImage ToBitmapSource(Bitmap source)
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

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            ExecuteAction(parameter);
        }

        #endregion
    }
    public class ContextMenuEntry : MenuItem
    {

        public ContextMenuEntry(ContextMenuAttribute attr, MethodInfo mi, object reference)
        {
            bool Activated = true;
            bool isMethod = !bool.TryParse(attr.Activated, out Activated);
            if (isMethod)
            {
                var mipred = reference.GetType().GetRuntimeMethod(attr.Activated, new Type[] { });
                var micoll = reference.GetType().GetRuntimeMethods();
                Activated = (bool)mipred.Invoke(reference, new object[] { });
            }
            Command = new RelayCommand(o => mi.Invoke(reference, new object[] { }));
            Header = attr.Header;
            if (!string.IsNullOrEmpty(attr.ImageSource))
                Icon = new System.Windows.Controls.Image {
                Source = new BitmapImage(new Uri(attr.ImageSource, UriKind.Relative)) };
            Visibility = attr.Visibility;
            IsEnabled = Activated;
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ContextMenuAttribute : Attribute
    {
        public string Activated { get; set; }

        public Visibility Visibility { get; set; }

        public string ImageSource { get; set; }

        public string Header { get; set; }

        public int Index { get; set; }

        //public ICommand Command { get; set; }
        public ContextMenuAttribute(string header, string imageSource, 
            string activated, Visibility visibility = Visibility.Visible, int index = 0)
        {
            Header = header;
            ImageSource = imageSource;
            Activated = activated;
            Visibility = visibility;
            Index = index;
        }
        public ContextMenuAttribute(string header, string imageSource,
            bool activated = true, Visibility visibility = Visibility.Visible, int index = 0)
        {
            Header = header;
            ImageSource = imageSource;
            Activated = activated.ToString();
            Visibility = visibility;
            Index = index;
        }

    }
}
