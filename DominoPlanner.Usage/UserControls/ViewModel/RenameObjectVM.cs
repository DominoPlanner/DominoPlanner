﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

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
    public class FilenameToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "Rename file " + value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    // from https://www.codeproject.com/Tips/1249276/WPF-Select-All-Focus-Behavior
    public class SelectAllFocusBehavior
    {
        public static bool GetEnable(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(EnableProperty);
        }

        public static void SetEnable(FrameworkElement frameworkElement, bool value)
        {
            frameworkElement.SetValue(EnableProperty, value);
        }

        public static readonly DependencyProperty EnableProperty =
                 DependencyProperty.RegisterAttached("Enable",
                    typeof(bool), typeof(SelectAllFocusBehavior),
                    new FrameworkPropertyMetadata(false, OnEnableChanged));

        private static void OnEnableChanged
                   (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = d as FrameworkElement;
            if (frameworkElement == null) return;

            if (e.NewValue is bool == false) return;

            if ((bool)e.NewValue)
            {
                frameworkElement.GotFocus += SelectAll;
                frameworkElement.PreviewMouseDown += IgnoreMouseButton;
            }
            else
            {
                frameworkElement.GotFocus -= SelectAll;
                frameworkElement.PreviewMouseDown -= IgnoreMouseButton;
            }
        }

        private static void SelectAll(object sender, RoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement is TextBox)
                ((TextBoxBase)frameworkElement).SelectAll();
            else if (frameworkElement is PasswordBox)
                ((PasswordBox)frameworkElement).SelectAll();
        }

        private static void IgnoreMouseButton
                (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null || frameworkElement.IsKeyboardFocusWithin) return;
            e.Handled = true;
            frameworkElement.Focus();
        }
    }
}
