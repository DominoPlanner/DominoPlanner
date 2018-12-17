using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DominoPlanner.Core;

namespace DominoPlanner.CoreTests
{
    class PostFilterTests
    {
        public static void PostFilterTest(string path)
        {
            PostFilterFieldTest(path);
        }
        public static void PostFilterFieldTest(string path)
        {
            FieldParameters fp = new FieldParameters(path, "colors.DColor", 8, 8, 24, 8, 10000, Emgu.CV.CvEnum.Inter.Lanczos4,
                ColorDetectionMode.Cie94Comparison, new Dithering(), new NoColorRestriction());

            fp.Generate().GenerateImage().Save("tests/FilterTests/vorFilter.png");
            InsertRowFieldTest(fp);
            DeleteRowFieldTest(fp);
            CopyPasteFieldTest(fp);
        }
        public static void InsertRowFieldTest(FieldParameters fp )
        {
            AddRows addRows = new AddRows(fp, fp.height * fp.length - 1, 5, 5, true);
            addRows.Apply();
            AddRows addRows2 = new AddRows(fp, 0, 5, 5, false);
            addRows2.Apply();
            fp.last.GenerateImage().Save("tests/FilterTests/nachRowEinfügen.png");
            SaveFieldPlan(fp, "tests/FilterTests/FeldplanNachRowEinfuegen.html");
            addRows2.Undo();
            addRows.Undo();
            fp.last.GenerateImage().Save("tests/FilterTests/nachRowEinfügenUndo.png");
        }
        public static void DeleteRowFieldTest(FieldParameters fp)
        {
            DeleteRow deleteRow = new DeleteRow(fp, (new int[] { 10, 15, 20 }).Select(x => x * fp.current_width).ToArray());
            deleteRow.Apply();
            
            SaveFieldPlan(fp, "tests/FilterTests/FeldplanNachRowLöschen.html");
            deleteRow.Undo();
            fp.last.GenerateImage().Save("tests/FilterTests/nachRowLoeschenUndo.png");

        }
        public static void CopyPasteFieldTest(FieldParameters fp)
        {
            int[] source_area = new int[400];
            int startx = 20;
            int starty = 20;
            int target_x = 30;
            int target_y = 30;
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    source_area[j * 20 + i] = fp.current_width * (j + starty) + (startx + i); 
                }
            }
            PasteFilter paste = new PasteFilter(fp, fp.current_width * starty + startx, source_area, fp.current_width * target_y + target_x);
            paste.Apply();
            fp.last.GenerateImage().Save("tests/FilterTests/nachPaste.png");
            paste.Undo();
            fp.last.GenerateImage().Save("tests/FilterTests/nachPasteUndo.png");
        }
        public static void SaveFieldPlan(FieldParameters fp, string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs); 
            sw.Write(fp.GetHTMLProcotol(new ObjectProtocolParameters()
            {
                backColorMode = ColorMode.Normal,
                foreColorMode = ColorMode.Intelligent,
                orientation = Core.Orientation.Horizontal,
                reverse = false,
                summaryMode = SummaryMode.Large,
                textFormat = "<font face=\"Verdana\">",
                templateLength = 20,
                textRegex = "%count% %color%",
                title = "Field"
            }));
        }
    }
}
