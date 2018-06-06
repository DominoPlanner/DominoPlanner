using DominoPlanner.Util;
using ImageProcessor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Document_Classes
{
    [Serializable]
    public class ImageEditingDocument : Document, INotifyPropertyChanged
    {
        
        public void OnPropertyChanged(String property)
        {
            if (this.PropertyChanged != null)
            {
                
                if (property == null)
                {
                    ApplyFilters();
                    if (parent is FieldDocument)
                    {
                        (parent as FieldDocument).OnPropertyChanged("Filter");
                    }
                    if (parent is StructureDocument)
                    {
                        (parent as StructureDocument).OnPropertyChanged("filterRCD");
                    }
                }
                this.PropertyChanged(this, new PropertyChangedEventArgs(null));
            }
        }
        [NonSerialized]
        private BitmapImage _source;
        public BitmapImage source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }
        [NonSerialized]
        private BitmapImage _preview;
        public BitmapImage preview {

            get { return _preview; }
            set { _preview = value; }
        }

        public List<Filter> available_filters
        {
            get; set;
        }
        private ObservableCollection<Filter> m_applied_filters;
        public ObservableCollection<Filter> applied_filters
        {
            get
            {
                return m_applied_filters;
            }
            set
            {
                m_applied_filters = value;
                OnPropertyChanged(null);
            }
        }
        public Document parent;
        public ImageEditingDocument()
        {
            
        }
        public ImageEditingDocument(String path)
        {
            t = type.img;
            this.path = path;
            this.filename = Path.GetFileName(path);

            var types = typeof(Filter).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Filter)));
            available_filters = new List<Filter>();
            foreach (var p in types)
            {
                available_filters.Add(Activator.CreateInstance(p as Type) as Filter);
            }

            Deserialize_Finish(path);
            applied_filters = new ObservableCollection<Filter>();
            ApplyFilters();
        }
        public ImageEditingDocument(System.Drawing.Image i)
        {
            this.t = type.img;
            var types = typeof(Filter).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Filter)));
            available_filters = new List<Filter>();
            foreach (var p in types)
            {
                available_filters.Add(Activator.CreateInstance(p as Type) as Filter);
            }
            source = ImageHelper.BitmapToBitmapImage((System.Drawing.Bitmap)i);
            applied_filters = new ObservableCollection<Filter>();
        }
        private void ApplyFilters()
        {
            using (MemoryStream inputstream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(inputstream);

                using (MemoryStream outputstream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(true))
                    {
                        inputstream.Position = 0;
                        imageFactory.Load(inputstream);
                        foreach (Filter f in applied_filters)
                        {
                            f.Apply(imageFactory);
                        }
                        imageFactory.Save(outputstream);
                    }
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = outputstream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    preview = bitmapImage;
                }
            }
        }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public override bool Compare(Document d)
        {
            return true;
        }

        public override void Save(string path)
        {
            throw new NotImplementedException();
        }

        public override void SavePNG()
        {
            throw new NotImplementedException();
        }
        public void Deserialize_Finish(String path)
        {
            this.path = path;
            source = new BitmapImage();

            source.BeginInit();

            source.UriSource = new Uri(path);
            source.DecodePixelHeight = 500;
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.EndInit();
            if (applied_filters == null)
            {
                applied_filters = new ObservableCollection<Filter>();
            }
            ApplyFilters();
            
        }
    }
}
