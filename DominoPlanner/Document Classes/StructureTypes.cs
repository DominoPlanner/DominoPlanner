using DominoPlanner.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace DominoPlanner.Document_Classes
{

    [Serializable()]
    public class ClusterStructureProvider : StructureRectProvider, INotifyPropertyChanged
    {
        public void OnPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                if (property.Contains('S')) // Structrure templates
                {
                    ReadStructureTemplates();
                }
                if (property.Contains('F')) // Fill
                {
                    FillDescriptionImages();
                }
                parent.OnPropertyChanged(property);
                PropertyChanged(this, new PropertyChangedEventArgs(null));
            }

        }

        [NonSerialized()]
        private List<string> m_list;
        public List<string> list
        {
            get { return m_list; }
            set { m_list = value; }
        }
        [field: NonSerialized()]
        public List<ClusterStructure> structure_templates;

        public ClusterStructure selected_template;
        [NonSerialized()]
        private BitmapSource[] m_description_imgs;

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public BitmapSource[] description_imgs
        {
            get
            {
                return m_description_imgs;
            }
            set { m_description_imgs = value; }
        }
        int m_length;
        public int length
        {
            get
            {
                return m_length;
            }
            set
            {
                m_length = value;
                OnPropertyChanged("RCD");
            }
        }
        int m_height;
        public int height
        {
            get
            {
                return m_height;
            }
            set
            {
                m_height = value;
                OnPropertyChanged("RCD");
            }
        }
        public int structure_index
        {
            get
            {
                int counter = 0;
                foreach (ClusterStructure c in structure_templates)
                {
                    if (selected_template.name == c.name) return counter;
                    counter++;
                }
                // structure not in list
                list.Add(selected_template.name);
                structure_templates.Add(selected_template);
                return counter;
            }
            set
            {

                selected_template = structure_templates[value];
                OnPropertyChanged("SFRCD");
            }
        }
        public ClusterStructureProvider(int width, int height, int structIndex)
        {
            m_length = width;
            m_height = height;
            list = new List<string>();
            description_imgs = new BitmapSource[9];
            // initialize structure combo box
            ReadStructureTemplates();
            selected_template = structure_templates[structIndex];
            FillDescriptionImages();
            // update structure logic

        }
        private void ReadStructureTemplates()
        {
            list.Clear();
            string templates = Properties.Settings.Default.StructureTemplates;
            XElement xml = XElement.Parse(templates);
            structure_templates = new List<ClusterStructure>();
            foreach (XElement definition in xml.Elements())
            {
                ClusterStructure d = new ClusterStructure(definition);
                list.Add(d.name);
                structure_templates.Add(d);
            }
        }

        // FÜllt die Vorschaubildchen auf
        private void FillDescriptionImages()
        {
            description_imgs = new BitmapSource[9];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    description_imgs[i + j * 3] = selected_template.DrawPreview(i, j, 50);
                }
            }
        }
        public override GenStructHelper UpdateRectangles()
        {
            return structure_templates[structure_index].GenerateStructure(length, height);
        }

        public override void OnLoad()
        {
            list = new List<string>();
            ReadStructureTemplates();
            FillDescriptionImages();

        }
    }
    [Serializable()]
    public abstract class StructureRectProvider
    {
        internal StructureDocument parent;

        public abstract GenStructHelper UpdateRectangles();

        public virtual void OnLoad() { }
    }
    [Serializable()]
    public class SpiralProvider : StructureRectProvider, INotifyPropertyChanged
    {
        int _domino_width;
        public int domino_width
        {
            get
            {
                return _domino_width;
            }
            set
            {
                if (value > 1)
                {
                    _domino_width = value;
                    OnPropertyChanged("RCD");
                }
            }
        }
        int _domino_height;
        public int domino_height
        {
            get
            {
                return _domino_height;
            }
            set
            {
                if (value > 1)
                {
                    _domino_height = value;
                    OnPropertyChanged("RCD");
                }
            }
        }
       
        int _tangential_distance;
        public int tangential_distance
        {
            get
            {
                return _tangential_distance;

            }
            set
            {
                if (value > 1)
                {
                    _tangential_distance = value;
                    OnPropertyChanged("RCD");
                }
            }
        }
        int _normal_distance;
        public int normal_distance
        {
            get
            {
                return _normal_distance;
            }
            set
            {
                if (value > 1)
                {
                    _normal_distance = value;
                    OnPropertyChanged("RCD");
                }
            }
        }
        int _quarter_rotations;
        public int quarter_rotations
        {
            get
            {
                return _quarter_rotations;
            }
            set
            {
                if (value > 4 && value < 5000)
                {
                    _quarter_rotations = value;
                    OnPropertyChanged("RCD");
                }
                
            }
        }
        [field:NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                parent.OnPropertyChanged(property);
            }
        }
        public SpiralProvider(int quarter_rotations,
            int domino_width, int domino_height, int tangential_distance, int normal_distance)
        {
            _domino_width = domino_width;
            _domino_height = domino_height;
            _tangential_distance = tangential_distance;
            _normal_distance = normal_distance;
            _quarter_rotations = quarter_rotations;
        }
        public override GenStructHelper UpdateRectangles()
        {
            DominoPlanner.Util.SpiralStructure spiral = new SpiralStructure(quarter_rotations, domino_width, domino_height, tangential_distance, normal_distance);
            return spiral.GenerateSpiral();
        }
    }
    [Serializable()]
    public class CircleProvider : StructureRectProvider, INotifyPropertyChanged
    {
        int _domino_width;
        public int domino_width
        {
            get
            {
                return _domino_width;
            }
            set
            {
                if (value > 1)
                {
                    _domino_width = value;
                    OnPropertyChanged("RCD");
                }
            }
        }
        int _domino_height;
        public int domino_height
        {
            get
            {
                return _domino_height;
            }
            set
            {
                if (value > 1)
                {
                    _domino_height = value;
                    OnPropertyChanged("RCD");
                }
            }
        }

        int _tangential_distance;
        public int tangential_distance
        {
            get
            {
                return _tangential_distance;

            }
            set
            {
                if (value > 1)
                {
                    _tangential_distance = value;
                    OnPropertyChanged("RCD");
                }
            }
        }
        int _normal_distance;
        public int normal_distance
        {
            get
            {
                return _normal_distance;
            }
            set
            {
                if (value > 1)
                {
                    _normal_distance = value;
                    OnPropertyChanged("RCD");
                }
            }
        }
        int _rotations;
        public int rotations
        {
            get
            {
                return _rotations;
            }
            set
            {
                if (value > 4 && value < 5000)
                {
                    _rotations = value;
                    OnPropertyChanged("RCD");
                }

            }
        }
        int _start_diameter;
        public int start_diameter
        {
            get
            {
                return _start_diameter;
            }
            set
            {
                if (value > _domino_height * 2)
                {
                    _start_diameter = value;
                    OnPropertyChanged("RCD");
                }

            }
        }
        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                parent.OnPropertyChanged(property);
            }
        }
        public CircleProvider(int rotations,
            int domino_width, int domino_height, int tangential_distance, int normal_distance)
        {
            _domino_width = domino_width;
            _domino_height = domino_height;
            _tangential_distance = tangential_distance;
            _normal_distance = normal_distance;
            _rotations = rotations;
            _start_diameter = 200;
        }
        public override GenStructHelper UpdateRectangles()
        {
            DominoPlanner.Util.CircleStructure circle = new CircleStructure(rotations, domino_width, domino_height, tangential_distance, normal_distance, start_diameter);
            return circle.GenerateCircle();
        }
    }


}
