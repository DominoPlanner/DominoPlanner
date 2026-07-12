using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage; // Wichtig für den neuen StorageProvider!
using DominoPlanner.Usage.UserControls.ViewModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DominoPlanner.Usage
{
	static class DialogExtensions
	{
		public static string GetCurrentProjectPath()
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
			{
				MainWindow parentWindow = desktopLifetime.Windows.OfType<MainWindow>().FirstOrDefault();
				if (parentWindow?.DataContext is MainWindowViewModel m)
				{
					return m.SelectedProject.GetInitialDirectory();
				}
			}
			return "";
		}

		public static string GetInitialDirectory(this DominoWrapperNodeVM node)
		{
			var project = node;
			if (project is DocumentNodeVM vm)
			{
				project = vm.Parent;
			}
			if (project is AssemblyNodeVM vm2)
			{
				var path = vm2.AbsolutePath;
				if (Environment.OSVersion.Platform == PlatformID.Win32NT || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					path = Path.GetDirectoryName(path);
				}
				return path;
			}
			return "";
		}

		// --- AB HIER: DIE NEUEN DATEIDIALOG-ERWEITERUNGEN FÜR AVALONIA 11 ---

		/// <summary>
		/// Öffnet einen modernen Datei-Auswahldialog.
		/// </summary>
		public async static Task<IReadOnlyList<IStorageFile>> ShowOpenFileDialogAsync<T>(string title, string initialDirectory = null, bool allowMultiple = false) where T : Window
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
			{
				T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
				if (parentWindow != null)
				{
					var options = new FilePickerOpenOptions
					{
						Title = title,
						AllowMultiple = allowMultiple
					};

					if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory))
					{
						options.SuggestedStartLocation = await parentWindow.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
					}

					return await parentWindow.StorageProvider.OpenFilePickerAsync(options);
				}
			}
			return null;
		}

		/// <summary>
		/// Öffnet einen modernen Ordner-Auswahldialog.
		/// </summary>
		public async static Task<IReadOnlyList<IStorageFolder>> ShowOpenFolderDialogAsync<T>(string title, string initialDirectory = null) where T : Window
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
			{
				T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
				if (parentWindow != null)
				{
					var options = new FolderPickerOpenOptions
					{
						Title = title
					};

					if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory))
					{
						options.SuggestedStartLocation = await parentWindow.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
					}

					return await parentWindow.StorageProvider.OpenFolderPickerAsync(options);
				}
			}
			return null;
		}

		/// <summary>
		/// Öffnet einen modernen Datei-Speichern-Dialog.
		/// </summary>
		public async static Task<IStorageFile> ShowSaveFileDialogAsync<T>(string title, string suggestedName = null, string initialDirectory = null) where T : Window
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
			{
				T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
				if (parentWindow != null)
				{
					var options = new FilePickerSaveOptions
					{
						Title = title,
						SuggestedFileName = suggestedName
					};

					if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory))
					{
						options.SuggestedStartLocation = await parentWindow.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
					}

					return await parentWindow.StorageProvider.SaveFilePickerAsync(options);
				}
			}
			return null;
		}

		// --- STANDARD-WINDOW-ERWEITERUNGEN (Unverändert, nur Null-Checks hinzugefügt) ---

		public async static Task<R> GetDialogResultWithParent<T, R>(this Window window) where T : Window
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
			{
				T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
				if (parentWindow != null)
				{
					return await window.ShowDialog<R>(parentWindow);
				}
			}
			return default;
		}

		public async static Task ShowDialogWithParent<T>(this Window window) where T : Window
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
			{
				T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
				if (parentWindow != null)
				{
					await window.ShowDialog(parentWindow);
				}
			}
		}

		public static void ShowWithParent<T>(this Window window) where T : Window
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
			{
				T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
				if (parentWindow != null)
				{
					window.Show(parentWindow);
				}
			}
		}

		// --- MSBOX-ERWEITERUNG (Angepasst an MsBox v2+ und ClickEnum) ---

		public static async Task<ButtonResult> ShowDialogWithParent<T>(this IMsBox<ButtonResult> window) where T : Window
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
			{
				// Sucht das aktive Hauptfenster vom Typ T (z.B. MainWindow)
				T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
				if (parentWindow != null)
				{
					// Öffnet die Box sauber als modales Popup über dem Parent-Fenster
					return await window.ShowAsPopupAsync(parentWindow);
				}
			}

			// Fallback, falls kein Parent-Fenster gefunden wurde
			return await window.ShowAsync();
		}

		public async static Task<ClickEnum> ShowDialogWithParent<T>(this IMsBox<ClickEnum> window) where T : Window
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
			{
				T parentWindow = desktopLifetime.Windows.OfType<T>().FirstOrDefault();
				if (parentWindow != null)
				{
					return await window.ShowAsPopupAsync(parentWindow);
				}
			}
			return ClickEnum.None;
		}
	}
}