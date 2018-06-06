using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.ComponentModel;
using System;

namespace DominoPlanner
{
    /// <summary>
    /// Interactionlogic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        /// <summary>
        /// Dependency Object for the value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty _value =
            DependencyProperty.Register("Value", typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(0.0, ValuePropertyChangeCallback), new ValidateValueCallback(validateValue));

        /// <summary>
        /// Dependency Object for the Minimal Value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty _minvalue =
            DependencyProperty.Register("MinValue", typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(0.0, MinMaxValueCallback));

        /// <summary>
        /// Dependency Object for the Maximal Value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty _maxvalue =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(100.0, MinMaxValueCallback));

        /// <summary>
        /// Dependency Object for the Maximal Value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty _decimation =
            DependencyProperty.Register("Decimation", typeof(uint), typeof(NumericUpDown), new FrameworkPropertyMetadata(0U, DecimationCallback));

        /// <summary>
        /// Dependency Object for the Step Value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty _step =
            DependencyProperty.Register("Step", typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(1.0));

        /// <summary>
        /// Dependency Object for the state of visibility of the UpDown Buttons
        /// </summary>
        public static readonly DependencyProperty _showButtons =
            DependencyProperty.Register("ShowButtons", typeof(bool), typeof(NumericUpDown), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Event Definition Value Change
        /// </summary>
        public static RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumericUpDown));

        /// <summary>
        /// Event fired when value changes
        /// </summary>
        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        /// <summary>
        /// Event Helper Function when Value is changed
        /// </summary>
        protected virtual void OnValueChanged()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = ValueChangedEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// Minimal possible value of the control
        /// </summary>
        static double _minValue = double.MinValue;

        /// <summary>
        /// Maximum possible value of the control
        /// </summary>
        static double _maxValue = double.MaxValue;

        /// <summary>
        /// Reference for static Functions
        /// </summary>
        static NumericUpDown thisReference = null;

        /// <summary>
        /// Default Constructor (nothing special here x) )
        /// </summary>
        public NumericUpDown()
        {
            InitializeComponent();
            thisReference = this;
        }

        /// <summary>
        /// Destroys the Class (sets thispointer null)
        /// </summary>
        ~NumericUpDown()
        {
            thisReference = null;
        }

        /// <summary>
        /// Specifies / Reads the number of digits shown after the decimal point
        /// </summary>
        public uint Decimation
        {
            get
            {
                return (uint)GetValue(_decimation);
            }
            set
            {
                SetValue(_decimation, value);
                SetDecimationBinding(value);
            }
        }

        /// <summary>
        /// Gets / Sets the value that the control is showing
        /// </summary>
        /// <exception cref="ArgumentException" />
        /// <remarks>If Value exceeds <see cref="MaxValue"/> or falls below <see cref="MinValue"/>, an <see cref="ArgumentException"/> is thrown</remarks>
        public double Value
        {
            get
            {
                return (double)GetValue(_value);
            }
            set
            {
                SetValue(_value, value);
            }
        }

        /// <summary>
        /// Specifies / Reads weather the UpDown Buttons are to be shown
        /// </summary>
        public bool ShowButtons
        {
            get
            {
                return (bool)GetValue(_value);
            }
            set
            {
                SetValue(_value, value);
            }
        }

        /// <summary>
        /// Gets / Sets the minimal value of the control's value
        /// </summary>
        public double MinValue
        {
            get
            {
                return (double)GetValue(_minvalue);
            }
            set
            {
                SetValue(_minvalue, value);
            }
        }

        /// <summary>
        /// Gets / Sets the maximal value of the control's value
        /// </summary>
        public double MaxValue
        {
            get
            {
                return (double)GetValue(_maxvalue);
            }
            set
            {
                SetValue(_maxvalue, value);
            }
        }

        /// <summary>
        /// Gets / Sets the step size (increment / decrement size) of the control's value
        /// </summary>
        public double Step
        {
            get
            {
                return (double)GetValue(_step);
            }
            set
            {
                SetValue(_step, value);
            }
        }


        /// <summary>
        /// Increments the control's value by the value defined by <see cref="Step"/>
        /// </summary>
        /// <remarks>The value doesn't increment over MaxValue or under MinValue</remarks>
        public void Increment()
        {
            try
            {
                Value += Step;
            }
            catch (ArgumentException)
            {
            }
        }

        /// <summary>
        /// Decrements the control's value by the value defined by <see cref="Step"/>
        /// </summary>
        /// <remarks>The value doesn't increment over MaxValue or under MinValue</remarks>
        public void Decrement()
        {
            try
            {
                Value -= Step;
            }
            catch (ArgumentException)
            {
            }
        }


        /// <summary>
        /// Validation function for the value.
        /// Checks weather Value is inbetween <see cref="MinValue"/> and <see cref="MaxValue"/>
        /// </summary>
        /// <param name="value">The current value of the Dependency Property</param>
        /// <returns><list type="bullet"><item><term>true</term><description>The Value is inbetween <see cref="MinValue"/> and <see cref="MaxValue"/></description></item><item><term>false</term><description>The value is out of bounds</description></item></list></returns>
        static bool validateValue(object value)
        {
            double val = Convert.ToDouble(value);
            return (val >= _minValue && val <= _maxValue);
        }

        /// <summary>
        /// Handler for the Up Button Click.
        /// Increments the <see cref="Value"/> by <see cref="Step"/>
        /// </summary>
        /// <param name="sender">The Up Button Control</param>
        /// <param name="e"></param>
        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            Increment();
        }

        /// <summary>
        /// Handler for the Down Button Click.
        /// Decrements the <see cref="Value"/> by <see cref="Step"/>
        /// </summary>
        /// <param name="sender">The Down Button Control</param>
        /// <param name="e"></param>
        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            Decrement();
        }

        /// <summary>
        /// Sets the decimation binding.
        /// </summary>
        /// <param name="decimation">The decimation.</param>
        private void SetDecimationBinding(uint decimation)
        {
            Binding bindingValue = new Binding("Value");
            bindingValue.Converter = new DecimationConverter();
            bindingValue.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingValue.ConverterParameter = decimation;
            bindingValue.ValidationRules.Add(new ExceptionValidationRule());

            tbValue.SetBinding(TextBox.TextProperty, bindingValue);
        }

        /// <summary>
        /// Change Event for Value
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void ValuePropertyChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (thisReference != null)
            {
                thisReference.OnValueChanged();
            }
        }

        /// <summary>
        /// Change Event for Min and Max Value
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void MinMaxValueCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == _minvalue)
            {
                _minValue = Convert.ToDouble(e.NewValue);
            }
            else if (e.Property == _maxvalue)
            {
                _maxValue = Convert.ToDouble(e.NewValue);
            }
        }

        /// <summary>
        /// Change Event for Decimation
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void DecimationCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //DecimationConverter.decimation = Convert.ToUInt32(e.NewValue);
            if (thisReference != null)
            {
                thisReference.SetDecimationBinding(Convert.ToUInt32(e.NewValue));
            }

            // Hack to update the control's binding.
            if (thisReference != null)
            {
                thisReference.Value++;
                thisReference.Value--;
            }
        }

        /// <summary>
        /// Handles the Loaded event of the ucNumericUpDown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ucNumericUpDown_Loaded(object sender, RoutedEventArgs e)
        {
            SetDecimationBinding(Decimation);
        }
    }

    /// <summary>
    /// Value Conversion Class for the button height.
    /// Divides the Hight of the control by two to get the height for one button.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class BtnHeightConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Value Converter for the Button Show Property.
    /// Converts from <see cref="bool"/> to <see cref="System.Windows.Visibility"/>
    /// </summary>
    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class BtnShowConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToBoolean(value))
            {
                return System.Windows.Visibility.Visible;
            }
            else
            {
                return System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Value Converter for the Button Show Property.
    /// Converts from <see cref="bool"/> to <see cref="System.Windows.Visibility"/>
    /// </summary>
    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class BtnShowGridConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToBoolean(value))
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for the Text in the Value Text Box.
    /// Makes sure that the correct decimation is displayed
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class DecimationConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dataValue = (double)value;

            return dataValue.ToString("F" + ((uint)parameter).ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return System.Convert.ToDouble(value);
            }
            catch (Exception)
            {
                return value;
            }
        }
    }
}
