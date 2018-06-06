using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DominoPlanner.Document_Classes
{
    [Serializable()]
    public abstract class ProjectDocument : Document, INotifyPropertyChanged
    {
        public List<String> ColorList;
        [field: NonSerialized]
        public abstract event PropertyChangedEventHandler PropertyChanged;

        [field: NonSerialized]
        private DominoCanvas _editCanvas;
        [field: NonSerialized]
        public DominoCanvas EditCanvas { get {
                if(_editCanvas == null)
                {
                    _editCanvas = new DominoCanvas();
                }
                return _editCanvas; }
        set
            {
                if(_editCanvas != value)
                {
                    _editCanvas = value;
                    OnPropertyChanged("EditCanvas");
                }
            }
        }
        public abstract ImageEditingDocument filters { get; set; }
        public abstract void OnPropertyChanged(String property);

        private Visibility m_Locked;

        public Visibility Locked
        {
            get
            {
                return m_Locked;
            }
            set
            {
                m_Locked = value;
                OnPropertyChanged("Locked");
            }
        }

        public String _sourcePath;
        public String SourcePath
        {
            get
            {
                if (_sourcePath == null) return null;
                return Path.Combine(Path.GetDirectoryName(path), "Source Images", Path.GetFileName(_sourcePath));
            }
            set
            {
                _sourcePath = Path.Combine(Path.GetDirectoryName(path), "Source Images", Path.GetFileName(value));
            }
        }

        [field: NonSerialized()]
        internal List<DominoColor> m_Colors;
        public abstract  List<DominoColor> Colors { get; set; }

        [field: NonSerialized()]
        internal ObservableCollection<DominoColor> m_UsedColors;
        public ObservableCollection<DominoColor> UsedColors
        {
            get
            {
                return m_UsedColors;
            }
            set
            {
                m_UsedColors = value;
            }
        }
        
        internal int m_length;
        public abstract int length { get; set; }
        internal int m_height;
        public abstract int height { get; set; }
    }
}
