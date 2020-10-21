using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input;

namespace DominoPlanner.Usage
{
    class RenameObjectVM : ModelBase
    {
        public RenameObjectVM(string filename)
        {
            OldName = filename;
            Cancel = new RelayCommand((o) => { result = false; Close = true; });
            OK = new RelayCommand((o) => { result = true; Close = true; });
        }
        #region properties
        private string _CurrentName;
        public string CurrentName
        {
            get => _CurrentName;
            set
            {
                if (value != _CurrentName)
                {
                    _CurrentName = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _OldName;
        public string OldName
        {
            get => _OldName;
            set
            {
                if (value != _OldName)
                {
                    _OldName = value;
                    CurrentName = Path.GetFileNameWithoutExtension(value);
                    Extension = Path.GetExtension(value);
                    RaisePropertyChanged();
                }
            }
        }
        private string _Extension;
        public string Extension
        {
            get => _Extension;
            set
            {
                if (value != _Extension)
                {
                    _Extension = value;
                    RaisePropertyChanged();
                }
            }
        }
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
        public bool result;
        public string NewName
        {
            get => CurrentName + Extension;
        }
        #endregion
        #region command
        private ICommand _Cancel;
        public ICommand Cancel { get { return _Cancel; } set { if (value != _Cancel) { _Cancel = value; } } }

        private ICommand _OK;
        public ICommand OK { get { return _OK; } set { if (value != _OK) { _OK = value; } } }

        #endregion

    }
    // from https://www.codeproject.com/Tips/1249276/WPF-Select-All-Focus-Behavior
    public class SelectAllFocusBehavior
    {
        public static bool GetEnable(Control frameworkElement)
        {
            return (bool)frameworkElement.GetValue(EnableProperty);
        }

        public static void SetEnable(Control frameworkElement, bool value)
        {
            frameworkElement.SetValue(EnableProperty, value);
        }

        public static readonly AvaloniaProperty EnableProperty =
                 AvaloniaProperty.RegisterAttached<TextBox, bool>("Enable", typeof(SelectAllFocusBehavior));

        private static void OnEnableChanged(AvaloniaPropertyChangedEventArgs e)
        {
            
            var frameworkElement = e.Sender as Control;
            if (frameworkElement == null) return;

            if (e.NewValue is bool == false) return;

            if ((bool)e.NewValue)
            {
                frameworkElement.GotFocus += SelectAll;
                frameworkElement.PointerPressed += IgnoreMouseButton;
            }
            else
            {
                frameworkElement.GotFocus -= SelectAll;
                frameworkElement.PointerPressed -= IgnoreMouseButton;
            }
        }

        private static void SelectAll(object sender, GotFocusEventArgs e)
        {
            var frameworkElement = e.Source as Control;
            if (frameworkElement is TextBox)
                ((TextBox)frameworkElement).SelectAll();
        }

        private static void IgnoreMouseButton
                (object sender, PointerPressedEventArgs e)
        {
            var frameworkElement = sender as Control;
            if (frameworkElement == null) return;
            e.Handled = true;
            frameworkElement.Focus();
        }
    }
}
