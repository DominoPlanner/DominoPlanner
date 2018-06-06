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
    class ProjectComposite : ModelBase
    {
        #region CTOR
        public ProjectComposite(int ownID, int projectID, string path, NodeType actType)
        {
            OwnID = ownID;
            ParentProjectID = projectID;
            FilePath = path;
            ActType = actType;
            conMenu = ContextMenueSelector.TreeViewMenues(actType);
            conMenu.fieldprotoMI.Click += FieldprotoMI_Click;
            conMenu.exportImageMI.Click += ExportImageMI_Click;
            conMenu.openFolderMI.Click += OpenFolderMI_Click;
            MouseClickCommand = new RelayCommand(o => { IsClicked?.Invoke(this, EventArgs.Empty); });
        }

        public ProjectComposite(int ownID, int projectID, string Name, string picturePath, string path, NodeType actType) : this(ownID, projectID, path, actType)
        {
            this.Name = Name;
            this.PicturePath = picturePath;

            if (actType == NodeType.ProjectNode)
                Img = "/Icons/folder_txt.ico";
            else if (actType == NodeType.ColorListNode)
                Img = "/Icons/colorLine.ico";
            else if (actType == NodeType.FreeHandFieldNode)
                Img = "/Icons/draw_freehand.ico";
            else
                Img = picturePath;
        }
        #endregion

        #region Command
        private ICommand _MouseClickCommand;
        public ICommand MouseClickCommand { get { return _MouseClickCommand; } set { if (value != _MouseClickCommand) { _MouseClickCommand = value; } } }
        #endregion

        #region Methods
        private void FieldprotoMI_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ProtocolV protocolV = new ProtocolV();
            protocolV.DataContext = new ProtocolVM(FilePath);
            protocolV.ShowDialog();
        }

        private void ExportImageMI_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OpenFolderMI_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", this.FilePath));
        }
        #endregion

        #region prope
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


        public string PicturePath { get; set; }

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
        ProjectNode,
        ColorListNode,
        FieldNode,
        FreeHandFieldNode,
        StructureNode
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

            Items.Add(openFolderMI);
        }
        public MenuItem fieldprotoMI = new MenuItem();
        public MenuItem createMI = new MenuItem();
        public MenuItem removeMI = new MenuItem();
        public MenuItem exportImageMI = new MenuItem();
        public MenuItem openFolderMI = new MenuItem();
    }

    public class ContextMenueSelector
    {
        public static ContextMenueProjectList TreeViewMenues(NodeType nodeType)
        {
            ContextMenueProjectList cm = new ContextMenueProjectList();
            switch (nodeType)
            {
                case NodeType.ProjectNode:
                    cm.Items.Add(cm.createMI);
                    cm.Items.Add(cm.removeMI);
                    break;
                case NodeType.ColorListNode:
                    break;
                case NodeType.FieldNode:
                    cm.Items.Add(cm.exportImageMI);
                    cm.Items.Add(cm.fieldprotoMI);
                    cm.Items.Add(cm.removeMI);
                    break;
                case NodeType.FreeHandFieldNode:
                    cm.Items.Add(cm.exportImageMI);
                    cm.Items.Add(cm.fieldprotoMI);
                    cm.Items.Add(cm.removeMI);
                    break;
                case NodeType.StructureNode:
                    cm.Items.Add(cm.exportImageMI);
                    cm.Items.Add(cm.removeMI);
                    break;
                default:
                    break;
            }
            return cm;
        }
    }
}
