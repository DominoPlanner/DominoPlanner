using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.Usage.UserControls.View
{
    public class FieldSize : UserControl
    {
        public FieldSize()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
    public class UpdateValueOnLeave : AvaloniaObject
    {
        static UpdateValueOnLeave()
        {
            CommandProperty.Changed.AddClassHandler<Control>(OnPropertyEnabled);
        }
        public static ICommand GetCommand(AvaloniaObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(AvaloniaObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static object GetCommandParameter(AvaloniaObject obj)
        {
            return obj.GetValue(CommandProperty);
        }

        public static void SetCommandParameter(AvaloniaObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static readonly StyledProperty<ICommand> CommandProperty =
                AvaloniaProperty.RegisterAttached<Control, FileDragDropHelper, ICommand>("Command", null);

        public static readonly StyledProperty<object> CommandParameterProperty =
                AvaloniaProperty.RegisterAttached<Control, FileDragDropHelper, object>("CommandParameter", null);

        private static void OnPropertyEnabled(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue) return;
            if (d is Control control && control != null)
            {
                control.AddHandler(Control.LostFocusEvent, OnLostFocusMethod);
                control.AddHandler(Control.KeyUpEvent, OnLostFocusMethod);
            }
        }
        private static void OnLostFocusMethod(object _sender, RoutedEventArgs _EventArgs)
        {
            if (!(_sender is AvaloniaObject d))
            {
                return;
            }
            if (_EventArgs is KeyEventArgs key)
            {
                if (key.Key != Key.Enter)
                    return;
            }
            var command = d.GetValue(CommandProperty);
            if (command != null)
                command.Execute(d.GetValue(CommandParameterProperty));
        }
    }
}
