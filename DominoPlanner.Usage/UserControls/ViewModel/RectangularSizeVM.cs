using System;
using System.Collections.Generic;
using System.Xml;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class RectangularSizeVM : StructureViewModel
    {
        #region CTOR
        public RectangularSizeVM()
        {   
            list = new List<string>();
            StuctureTypes();
        }
        #endregion

        #region prop
        private string _Path = @"D:\Dropbox\Dropbox\Structures.xml";
        public string Path
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

        private List<string> _list;
        public List<string> list
        {
            get { return _list; }
            set
            {
                if (_list != value)
                {
                    _list = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _sLength;
        public int sLength
        {
            get { return _sLength; }
            set
            {
                if (_sLength != value)
                {
                    _sLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _sHeight;
        public int sHeight
        {
            get { return _sHeight; }
            set
            {
                if (_sHeight != value)
                {
                    _sHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _structure_index;
        public int structure_index
        {
            get { return _structure_index; }
            set
            {
                if (_structure_index != value)
                {
                    _structure_index = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BitmapSource[] _description_imgs;
        public BitmapSource[] description_imgs
        {
            get { return _description_imgs; }
            set
            {
                if (_description_imgs != value)
                {
                    _description_imgs = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        
        #region Methods
        private void StuctureTypes()
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(_Path);
                for (int i = 0; i < document.FirstChild.ChildNodes.Count; i++)
                    list.Add(document.FirstChild.ChildNodes[i].Attributes["Name"].Value);
            }
            catch (Exception es)
            {
                System.Diagnostics.Debug.WriteLine(es);
            }
        }
        #endregion
    }
}
