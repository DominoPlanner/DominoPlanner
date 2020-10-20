using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Usage
{
    public class DominoStone : ModelBase
    {
        #region CTOR
        public DominoStone(string name, int count, System.Windows.Media.Color rgb, int ID)
        {
            this.Name = name;
            this.Count = count;
            this.RGB = rgb;
            this.ID = ID;
        }
        #endregion

        #region prop
        private int _ID;
        [Category("Stone")]
        [Browsable(false)]
        [DisplayName("Count")]
        public int ID
        {
            get { return _ID; }
            set
            {
                if (_ID != value)
                {
                    _ID = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _Count;
        [Category("Stone")]
        [Browsable(true)]
        [DisplayName("Count")]
        public int Count
        {
            get { return _Count; }
            set
            {
                if (_Count != value)
                {
                    _Count = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Name;
        [Category("Stone")]
        [DisplayName("Name")]
        [Browsable(true)]
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

        private System.Windows.Media.Color _RGB;
        [Category("Stone")]
        [DisplayName("Color")]
        [Browsable(true)]
        public System.Windows.Media.Color RGB
        {
            get { return _RGB; }
            set
            {
                if (_RGB != value)
                {
                    _RGB = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
    }
}
