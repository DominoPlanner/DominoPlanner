using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using DominoPlanner.Core;
using System.Reflection;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    class RectangularSizeVM : StructureViewModel
    {
        #region CTOR
        public RectangularSizeVM() : base()
        {
            list = new List<string>();
            StuctureTypes();
            structure_index = 0;

            sHeight = 1;
            sLength = 1;
        }
        #endregion

        #region fields
        IEnumerable<XElement> structures;
        #endregion

        #region prop
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

        private int _structure_index = -1;
        public int structure_index
        {
            get { return _structure_index; }
            set
            {
                if (_structure_index != value)
                {
                    _structure_index = value;
                    selectedStructureElement = structures.ElementAt(_structure_index);

                    RefreshDescriptionImages();

                    RaisePropertyChanged();
                }
            }
        }

        public event EventHandler SelStructureTypeChanged;

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
                XElement xElement = XElement.Parse(Properties.Resources.Structures);
                structures = xElement.Elements();

                foreach (var structure in structures)
                {
                    list.Add(structure.FirstAttribute.Value);
                }
            }
            catch (Exception es)
            {
                System.Diagnostics.Debug.WriteLine(es);
            }
        }

        public void RefreshDescriptionImages()
        {
            WriteableBitmap[,] previews = StructureParameters.getPreviews(51, selectedStructureElement);
            description_imgs = new BitmapSource[9];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    description_imgs[i + j * 3] = previews[i, j];
                }
            }
            RaisePropertyChanged("description_imgs");
        }
        #endregion
    }
}
