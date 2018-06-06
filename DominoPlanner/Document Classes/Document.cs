using ColorMine.ColorSpaces.Comparisons;
using Dominorechner_V2.ColorMine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DominoPlanner.Document_Classes
{
    public enum type
    {
        mst, clr, fld, sct, flp, img
    }
    [Serializable()] 
    public abstract class Document
    {
        public string path { get; set; }

        public string filename { get; set; }

        public type t { get; set; }

        //public List<DominoColor> Colors { get; set; }
        
        public static Document Load(String path)
        {
            switch (Path.GetExtension(path))
            {
                case ".clr":
                    return ColorArrayDocument.LoadColorArray(path);
                case ".fld":
                    return FieldDocument.LoadFieldDocument(path);
                case ".sct":
                    return StructureDocument.LoadStructureDocument(path);
                default:
                    return null;
            }
        }
        public static Document FastLoad(String path)
        {
            switch (Path.GetExtension(path))
            {
                case ".clr":
                    return ColorArrayDocument.LoadColorArray(path);
                case ".fld":
                    return FieldDocument.LoadFieldDocumentWithoutDrawing(path);
                case ".sct":
                    return StructureDocument.LoadStructureDocumentWithoutDrawing(path);
                default:
                    return null;
            }
        }
        public abstract void SavePNG();
        public abstract bool Compare(Document d);
        public abstract void Save(String path);
    }
}
