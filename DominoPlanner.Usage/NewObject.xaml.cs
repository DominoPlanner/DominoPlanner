using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.Usage
{
    /// <summary>
/// IFileDragDropTarget Interface
/// </summary>
public interface IFileDragDropTarget
{
    void OnFileDrop(string[] filepaths);
}

/// <summary>
/// FileDragDropHelper
/// modified from https://stackoverflow.com/a/37608994
/// </summary>
public class FileDragDropHelper : AvaloniaObject
{
    static FileDragDropHelper()
    {
        IsFileDragDropEnabledProperty.Changed.AddClassHandler<Control>(OnFileDragDropEnabled);
    }
    public static bool GetIsFileDragDropEnabled(AvaloniaObject obj)
    {
        return (bool)obj.GetValue(IsFileDragDropEnabledProperty);
    }

    public static void SetIsFileDragDropEnabled(AvaloniaObject obj, bool value)
    {
        obj.SetValue(IsFileDragDropEnabledProperty, value);
    }

    public static bool GetFileDragDropTarget(AvaloniaObject obj)
    {
        return (bool)obj.GetValue(FileDragDropTargetProperty);
    }

    public static void SetFileDragDropTarget(AvaloniaObject obj, bool value)
    {
        obj.SetValue(FileDragDropTargetProperty, value);
    }

    public static readonly StyledProperty<bool> IsFileDragDropEnabledProperty =
            AvaloniaProperty.RegisterAttached<Control, FileDragDropHelper, bool>("IsFileDragDropEnabled", false); 

    public static readonly StyledProperty<object> FileDragDropTargetProperty =
            AvaloniaProperty.RegisterAttached<Control, FileDragDropHelper, object>("FileDragDropTarget", null);

    private static void OnFileDragDropEnabled(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue == e.OldValue) return;
        if (d is Control control && control != null) 
        {
            control.AddHandler(DragDrop.DropEvent, OnDrop);
        }
    }

    private static void OnDrop(object _sender, DragEventArgs _dragEventArgs)
    {
        if (!(_sender is AvaloniaObject d))
        {
            return;
        }
        var target = d.GetValue(FileDragDropTargetProperty);
        if (target is IFileDragDropTarget fileTarget && fileTarget != null)
        {
            if (_dragEventArgs.Data.Contains(DataFormats.FileNames))
            {
                fileTarget.OnFileDrop(_dragEventArgs.Data.GetFileNames().ToArray());
            }
        }
        else
        {
            throw new Exception("FileDragDropTarget object must be of type IFileDragDropTarget");
        }
    }
}
    public class NewObject : Window
    {
        public NewObject()
        {
            this.InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
        public NewObject(NewObjectVM novm)
        {
            InitializeComponent();
            DataContext = novm;
            ((NewObjectVM)DataContext).CloseChanged += NewObject_CloseChanged;
            // this breaks the MVVM pattern, but we need the current window to correctly raise dialogs
            NewObjectVM.Window = this;
        }

        private void NewObject_CloseChanged(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
