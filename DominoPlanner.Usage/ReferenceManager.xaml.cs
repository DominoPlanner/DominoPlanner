using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DominoPlanner.Core;
using DynamicData.Annotations;
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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Directory = Path.GetDirectoryName(Path.GetFullPath(Path.Combine(ParentPath, RelativePath)));
            ofd.Title = string.Format(_("Locate file {0}"), Path.GetFileName(RelativePath));
            string extension = Path.GetExtension(RelativePath);
            ofd.Filters.Add(new FileDialogFilter() { Extensions = new List<string> { extension.Replace(".", "") }, Name = string.Format(GetParticularString("Files of type {0}", "{0} files"), extension) });
            ofd.Filters.Add(new FileDialogFilter() { Extensions = new List<string> { "*" }, Name = _("All files") });
            ofd.AllowMultiple = false;
            string[] result = await ofd.ShowAsync(ReferenceManagerViewModel.window);
            if (result != null && result.Length == 1 && File.Exists(result[0]))
            {
                AbsolutePath = result[0];
            }
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
        public static ReferenceManager window;
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
    public class ReferenceManager : Window
    {
        
        public ReferenceManager()
        {
            this.InitializeComponent();
            this.DataContext = new ReferenceManagerViewModel();
            ReferenceManagerViewModel.window = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
