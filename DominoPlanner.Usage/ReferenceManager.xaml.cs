using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace DominoPlanner.Usage
{
    using static Localizer;
    public class PathResolutionVM : ModelBase
    {
        private PathResolution model;

        public string RelativePath
        {
            get { return model.RelativePath; }
        }
        public string ParentPath
        {
            get
            {
                return Workspace.Find(model.reference) ?? model.ParentPath;
            }
        }

        public string AbsolutePath
        {
            set
            {
                var rp = model.RelativePath;
                model.AbsolutePath = value;
                // update the workspace state
                //Workspace.AbsolutePathFromReference(ref rp, model.reference);

                //model.RelativePath = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsResolved));
            }
            get { return model.AbsolutePath; }
        }

        public bool IsResolved
        {
            get { return File.Exists(AbsolutePath); }
        }
        public PathResolutionVM(PathResolution model)
        {
            this.model = model;
            replacePathCommand = new RelayCommand(o => ReplacePath());
        }

        private async void ReplacePath()
        {
            if (IsResolved) return;
            try
            {
                var app = Avalonia.Application.Current;
                if (app?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                    if (topLevel == null) return;

                    string extension = Path.GetExtension(RelativePath);
                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = string.Format(_("Locate file {0}"), Path.GetFileName(RelativePath)),
                        AllowMultiple = false,
                        FileTypeFilter = new[]
                        {
                            new FilePickerFileType(string.Format(GetParticularString("Files of type {0}", "{0} files"), extension)) { Patterns = new[] { $"*{extension}" } },
                            new FilePickerFileType(_("All files")) { Patterns = new[] { "*" } }
                        }
                    });

                    if (files.Count > 0 && File.Exists(files[0].Path.LocalPath))
                    {
                        AbsolutePath = files[0].Path.LocalPath;
                    }
                }
            }
            catch (Exception) { }
        }

        private RelayCommand replacePathCommand;

        public RelayCommand ReplacePathCommand
        {
            get { return replacePathCommand; }
            set { replacePathCommand = value; RaisePropertyChanged();  }
        }

    }
    public class ReferenceManagerViewModel : ModelBase
    {
        public ObservableCollection<PathResolutionVM> VMs { get; set; }
        public ReferenceManagerViewModel()
        {
            VMs = new ObservableCollection<PathResolutionVM>();
            foreach (var i in Workspace.Instance.resolvedPaths)
            {
                VMs.Add(new PathResolutionVM(i));
            }
        }
    }
    public partial class ReferenceManager : Window
    {
        
        public ReferenceManager()
        {
            this.InitializeComponent();
            this.DataContext = new ReferenceManagerViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
