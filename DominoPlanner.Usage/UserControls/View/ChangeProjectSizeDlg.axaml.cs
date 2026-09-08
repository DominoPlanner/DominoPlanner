using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.Usage.UserControls
{
	public partial class ChangeProjectSizeDlg : Window
	{
		public int Count { get; private set; }
		public ResizeMode ResizePlace { get; private set; }

		public ChangeProjectSizeDlg()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private void OnTop(object? sender, RoutedEventArgs e)
		{
			Count = (int)nudSizeChanger.Value;
			ResizePlace = ResizeMode.Top;
			this.Close();
		}

		private void OnBottom(object? sender, RoutedEventArgs e)
		{
			Count = (int)nudSizeChanger.Value;
			ResizePlace = ResizeMode.Bottom;
			this.Close();
		}

		private void OnLeft(object? sender, RoutedEventArgs e)
		{
			Count = (int)nudSizeChanger.Value;
			ResizePlace = ResizeMode.Left;
			this.Close();
		}

		private void OnRight(object? sender, RoutedEventArgs e)
		{
			Count = (int)nudSizeChanger.Value;
			ResizePlace = ResizeMode.Right;
			this.Close();
		}
	}

	// Local enum for resize placement used by the Usage project dialog
	public enum ResizeMode
	{
		Top,
		Bottom,
		Left,
		Right,
		none
	}
}
