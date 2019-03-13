using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using DominoPlanner.Usage.Serializer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage
{
    public class ProjectComposite : ModelBase
    { 
        #region CTOR
        public ProjectComposite(ProjectElement projectTransfer)
        {
            this.FilePath = projectTransfer.FilePath;
            
            if (projectTransfer.documentNode is DocumentNode documentNode)
            {
                conMenu = ContextMenueSelector.TreeViewMenues(projectTransfer.CurrType, Workspace.LoadHasProtocolDefinition<IDominoProvider>(projectTransfer.FilePath));
            }
            else
            {
                conMenu = ContextMenueSelector.TreeViewMenues(projectTransfer.CurrType);
            }
                conMenu.fieldprotoMI.Click += FieldprotoMI_Click;
            conMenu.exportImageMI.Click += ExportImageMI_Click;
            conMenu.openFolderMI.Click += OpenFolderMI_Click;
            MouseClickCommand = new RelayCommand(o => { IsClicked?.Invoke(this, EventArgs.Empty); });

            this.Name = projectTransfer.Name;
            this.PicturePath = projectTransfer.IcoPath;

            if (projectTransfer.CurrType == NodeType.MasterplanNode)
                Img = "/Icons/folder_txt.ico";
            else if (projectTransfer.CurrType == NodeType.ColorListNode)
                Img = "/Icons/colorLine.ico";
            else
                Img = PicturePath;
            this.ActType = projectTransfer.CurrType;
            Project = projectTransfer;
        }

        public ProjectComposite(ProjectElement projectTransfer, int parentID) : this(projectTransfer)
        {
            this.ParentProjectID = parentID;
        }
        #endregion

        #region Command
        private ICommand _MouseClickCommand;
        public ICommand MouseClickCommand { get { return _MouseClickCommand; } set { if (value != _MouseClickCommand) { _MouseClickCommand = value; } } }
        #endregion

        #region Methods
        private void FieldprotoMI_Click(object sender, RoutedEventArgs e)
        {
            if (Project.documentNode is DocumentNode documentNode)
            {
                if (!documentNode.obj.HasProtocolDefinition)
                {
                    Errorhandler.RaiseMessage("Could not generate a protocol!", "No Protocol", Errorhandler.MessageType.Warning);
                    return;
                }
                ProtocolV protocolV = new ProtocolV();
                documentNode.obj.Generate();
                protocolV.DataContext = new ProtocolVM(documentNode.obj, Path.GetFileNameWithoutExtension(documentNode.relativePath));
                protocolV.ShowDialog();
            }
        }

        private void ExportImageMI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Project.documentNode is DocumentNode documentNode)
                {
                    System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    saveFileDialog.Filter = "png files (*.png)|*.png";
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (File.Exists(saveFileDialog.FileName))
                        {
                            File.Delete(saveFileDialog.FileName);
                        }
                        documentNode.obj.Generate().GenerateImage().Save(saveFileDialog.FileName);
                    }
                }
            }
            catch (Exception) { }
        }

        private void OpenFolderMI_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", this.FilePath));
        }
        #endregion

        #region prope
        private ProjectElement _Project;
        public ProjectElement Project
        {
            get { return _Project; }
            set
            {
                if (_Project != value)
                {
                    _Project = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _OwnID;
        public int OwnID
        {
            get { return _OwnID; }
            set
            {
                if (_OwnID != value)
                {
                    _OwnID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ParentProjectID;
        public int ParentProjectID
        {
            get { return _ParentProjectID; }
            set
            {
                if (_ParentProjectID != value)
                {
                    _ParentProjectID = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string PicturePath;

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Img;
        public string Img
        {
            get { return _Img; }
            set
            {
                if (_Img != value)
                {
                    _Img = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ContextMenueProjectList _conMenu;
        public ContextMenueProjectList conMenu
        {
            get { return _conMenu; }
            set
            {
                if (_conMenu != value)
                {
                    _conMenu = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Path;
        public string FilePath
        {
            get { return _Path; }
            set
            {
                if (_Path != value)
                {
                    _Path = value;
                    RaisePropertyChanged();
                }
            }
        }

        private NodeType _ActType;
        public NodeType ActType
        {
            get { return _ActType; }
            set
            {
                if (_ActType != value)
                {
                    _ActType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    SelectedEvent?.Invoke(this, EventArgs.Empty);
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsExpanded;
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                if (_IsExpanded != value)
                {
                    _IsExpanded = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Eventhandler
        public event EventHandler SelectedEvent;
        public event EventHandler IsClicked;
        #endregion
    }

    public enum NodeType
    {
        MasterplanNode,
        ColorListNode,
        ProjectNode
    }

    public class ContextMenueProjectList : ContextMenu
    {
        public ContextMenueProjectList() : base()
        {
            openFolderMI.Header = "Open Folder";
            openFolderMI.Icon = new System.Windows.Controls.Image { Source = new BitmapImage(new Uri("Icons/folder_tar.ico", UriKind.Relative)) };
            exportImageMI.Header = "Export as Image";
            exportImageMI.Icon = new System.Windows.Controls.Image { Source = new BitmapImage(new Uri("Icons/image.ico", UriKind.Relative)) };
            removeMI.Header = "Remove";
            removeMI.Icon = new System.Windows.Controls.Image { Source = new BitmapImage(new Uri("Icons/remove.ico", UriKind.Relative)) };
            createMI.Header = "Create Field/Structure";
            createMI.Icon = new System.Windows.Controls.Image { Source = new BitmapImage(new Uri("Icons/add.ico", UriKind.Relative)) };
            fieldprotoMI.Header = "Generate Fieldprotocol";
            fieldprotoMI.Icon = new System.Windows.Controls.Image { Source = new BitmapImage(new Uri("Icons/file_export.ico", UriKind.Relative)) };
            renameMI.Header = "Rename";
            renameMI.Icon = new System.Windows.Controls.Image { Source = new BitmapImage(new Uri("Icons/draw_freehand.ico", UriKind.Relative)) };
            Items.Add(openFolderMI);
        }
        public MenuItem fieldprotoMI = new MenuItem();
        public MenuItem createMI = new MenuItem();
        public MenuItem removeMI = new MenuItem();
        public MenuItem exportImageMI = new MenuItem();
        public MenuItem openFolderMI = new MenuItem();
        public MenuItem renameMI = new MenuItem();
    }

    public class ContextMenueSelector
    {
        public static ContextMenueProjectList TreeViewMenues(NodeType nodeType, bool hasProtocol = false)
        {
            ContextMenueProjectList cm = new ContextMenueProjectList();
            switch (nodeType)
            {
                case NodeType.MasterplanNode:
                    cm.Items.Add(cm.createMI);
                    cm.Items.Add(cm.renameMI);
                    cm.Items.Add(cm.removeMI);
                    
                    break;
                case NodeType.ColorListNode:
                    break;
                case NodeType.ProjectNode:
                    cm.Items.Add(cm.exportImageMI);
                    if(hasProtocol) cm.Items.Add(cm.fieldprotoMI);
                    cm.Items.Add(cm.renameMI);
                    cm.Items.Add(cm.removeMI);
                    break;
                default:
                    break;
            }
            return cm;
        }
    }
}
