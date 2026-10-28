using Avalonia;
using Avalonia.Controls;
using System;
using System.ComponentModel;

namespace DominoPlanner.Usage
{
	public class LengthNumericUpDown : NumericUpDown
	{
		protected override Type StyleKeyOverride => typeof(NumericUpDown);

		private const decimal MillimetersPerInch = 25.4m;
		private bool _updating;

		public static readonly StyledProperty<int> MillimetersProperty =
			AvaloniaProperty.Register<LengthNumericUpDown, int>(nameof(Millimeters));

		public int Millimeters
		{
			get => GetValue(MillimetersProperty);
			set => SetCurrentValue(MillimetersProperty, value);
		}

		public LengthNumericUpDown()
		{
			AttachedToVisualTree += OnAttachedToVisualTree;
			DetachedFromVisualTree += OnDetachedFromVisualTree;
			ValueChanged += OnValueChanged;
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			if (!_updating && change.Property == MillimetersProperty)
				UpdateDisplayedValue();
		}

		private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
		{
			UserSettings.Instance.PropertyChanged += OnSettingsChanged;
			UpdateDisplayedValue();
		}

		private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
		{
			UserSettings.Instance.PropertyChanged -= OnSettingsChanged;
		}

		private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(UserSettings.UseImperialUnits))
				UpdateDisplayedValue();
		}

		private void OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
		{
			if (_updating || Value is not decimal displayValue)
				return;

			decimal millimeters = UserSettings.Instance.UseImperialUnits
				? displayValue * MillimetersPerInch
				: displayValue;
			int roundedMillimeters = (int)Math.Round(millimeters, MidpointRounding.AwayFromZero);

			if (Millimeters != roundedMillimeters)
			{
				_updating = true;
				SetCurrentValue(MillimetersProperty, roundedMillimeters);
				_updating = false;
			}
		}

		public void RefreshDisplayedValue()
		{
			_updating = true;
			Value = UserSettings.Instance.UseImperialUnits
				? Millimeters / MillimetersPerInch
				: Millimeters;
			_updating = false;
		}

		private void UpdateDisplayedValue() => RefreshDisplayedValue();
	}
}
