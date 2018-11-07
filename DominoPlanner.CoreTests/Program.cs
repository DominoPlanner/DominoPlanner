using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMine.ColorSpaces.Comparisons;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using DominoPlanner.Core;
using System.Xml.Linq;
using DominoPlanner.Core.ColorMine.Comparisons;
using System.Diagnostics;

namespace DominoPlanner.CoreTests
{
    class Program
    {

        static void Main(string[] args)
        {
            HistoryTreeFieldTest();
            //FieldTest();
            //SpiralTest();
            //WallTest();
            Console.Read();
        }
        static void FieldTest()
        {
            //Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));

            BitmapImage b = new BitmapImage(new Uri("./NewField.jpg", UriKind.RelativeOrAbsolute));
            WriteableBitmap wb = new WriteableBitmap(b);
            FieldParameters p = new FieldParameters(wb, new List<DominoColor>(), 8, 8, 24, 8, 10000, BitmapScalingMode.HighQuality, DitherMode.NoDithering, ColorDetectionMode.CieDe2000Comparison);
            p.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            p.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            p.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            p.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            p.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            p.colors.Add(new DominoColor(Colors.White, 1000, "white"));
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //DominoTransfer t = await Dispatcher.CurrentDispatcher.Invoke(async () => await Task.Run(() => p.Generate()));

            DominoTransfer t = p.Generate();
            Console.WriteLine("Size: " + t.dominoes.Count());
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch = System.Diagnostics.Stopwatch.StartNew();
            WriteableBitmap b2 = t.GenerateImage(2000, false);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(b2));
            using (var stream = new FileStream(@"FieldTest.png", FileMode.Create))
                encoder.Save(stream);
            FileStream fs = new FileStream(@"FieldPlanTest.html", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(p.GetHTMLProcotol(new ObjectProtocolParameters()
            {
                backColorMode = ColorMode.Normal,
                foreColorMode = ColorMode.Intelligent,
                orientation = Orientation.Horizontal,
                reverse = false,
                summaryMode = SummaryMode.Large,
                textFormat = "<font face=\"Verdana\">",
                templateLength = 20,
                textRegex = "%count% %color%",
                title = "Field"
            }));
            //p.SaveXLSFieldPlan("ExcelFieldPlanTest.xlsx", new ObjectProtocolParameters()
            //{
            //    backColorMode = ColorMode.Normal,
            //    foreColorMode = ColorMode.Intelligent,
            //    orientation = Orientation.Horizontal,
            //    reverse = false,
            //    summaryMode = SummaryMode.Large,
            //    textFormat = "<font face=\"Verdana\">",
            //    templateLength = 20,
            //    textRegex = "%count%",
            //    title = "Field",
            //    path = Directory.GetCurrentDirectory()

            //});
            sw.Close();

        }
        static void WBXTest()
        {
            WriteableBitmap ex = BitmapFactory.New(500, 500);
            ex.FillPolygon(new int[] { 50, 50, 100, 50, 200, 100, 50, 50 }, Colors.Blue);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(ex));
            using (var stream = new FileStream(@"WBXTest.png", FileMode.Create))
                encoder.Save(stream);
        }
        static void SpiralTest()
        {
            Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));

            BitmapImage b = new BitmapImage(new Uri("./NewField.jpg", UriKind.Relative));
            WriteableBitmap wb = new WriteableBitmap(b);
            SpiralParameters p = new SpiralParameters(wb, 80, 24, 8, 8, 8, new List<DominoColor>(), ColorDetectionMode.CieDe2000Comparison, false, AverageMode.Corner);
            p.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            p.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            p.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            p.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            p.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            p.colors.Add(new DominoColor(Colors.White, 1000, "white"));
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //DominoTransfer t = await Dispatcher.CurrentDispatcher.Invoke(async () => await Task.Run(() => p.Generate(wb, progress)));

            DominoTransfer t = p.Generate(progress);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch = System.Diagnostics.Stopwatch.StartNew();
            WriteableBitmap b2 = t.GenerateImage(2000, false);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(b2));
            using (var stream = new FileStream(@"SpiralTest.png", FileMode.Create))
                encoder.Save(stream);
            FileStream fs = new FileStream(@"SpiralPlanTest.html", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(p.GetHTMLProcotol(new ObjectProtocolParameters()
            {
                backColorMode = ColorMode.Normal,
                foreColorMode = ColorMode.Intelligent,
                orientation = Orientation.Horizontal,
                reverse = false,
                summaryMode = SummaryMode.Large,
                textFormat = "<font face=\"Verdana\">",
                templateLength = 20,
                textRegex = "%count% %color%",
                title = "Field"
            }));
            //p.SaveXLSFieldPlan("ExcelFieldPlanTest.xlsx", new ObjectProtocolParameters()
            //{
            //    backColorMode = ColorMode.Normal,
            //    foreColorMode = ColorMode.Intelligent,
            //    orientation = Orientation.Horizontal,
            //    reverse = false,
            //    summaryMode = SummaryMode.Large,
            //    textFormat = "<font face=\"Verdana\">",
            //    templateLength = 20,
            //    textRegex = "%count%",
            //    title = "Field",
            //    path = Directory.GetCurrentDirectory()

            //});
            sw.Close();
        }
        static void WallTest()
        {
            Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));

            BitmapImage b = new BitmapImage(new Uri("./NewField.jpg", UriKind.Relative));
            WriteableBitmap wb = new WriteableBitmap(b);
            StreamReader sr = new StreamReader(new FileStream("Structures.xml", FileMode.Open));
            XElement xml = XElement.Parse(sr.ReadToEnd());
            StructureParameters p = new StructureParameters(wb, xml.Elements().ElementAt(6), 1000, new List<DominoColor>(), ColorDetectionMode.CieDe2000Comparison, AverageMode.Average, true);
            p.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            p.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            p.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            p.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            p.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            p.colors.Add(new DominoColor(Colors.LightGray, 1000, "white"));
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //DominoTransfer t = await Dispatcher.CurrentDispatcher.Invoke(async () => await Task.Run(() => p.Generate(wb, progress)));

            DominoTransfer t = p.Generate(progress);
            Console.WriteLine("Size: " + t.dominoes.Count());
            watch.Stop();
            //Console.WriteLine(watch.ElapsedMilliseconds);
            //watch = System.Diagnostics.Stopwatch.StartNew();
            //WriteableBitmap b2 = t.GenerateImage(2000, false);
            //watch.Stop();
            //Console.WriteLine(watch.ElapsedMilliseconds);
            //PngBitmapEncoder encoder = new PngBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(b2));
            //using (var stream = new FileStream(@"WallTest.png", FileMode.Create))
            //    encoder.Save(stream);
            //FileStream fs = new FileStream(@"WallPlanTest.html", FileMode.Create);
            //StreamWriter sw = new StreamWriter(fs);
            //sw.Write(p.GetHTMLProcotol(new ObjectProtocolParameters()
            //{
            //    backColorMode = ColorMode.Normal,
            //    foreColorMode = ColorMode.Intelligent,
            //    orientation = Orientation.Horizontal,
            //    reverse = false,
            //    summaryMode = SummaryMode.Large,
            //    textFormat = "<font face=\"Verdana\">",
            //    templateLength = 20,
            //    textRegex = "%count% %color%",
            //    title = "Field"
            //}));
            //p.SaveXLSFieldPlan("ExcelFieldPlanTest.xlsx", new ObjectProtocolParameters()
            //{
            //    backColorMode = ColorMode.Normal,
            //    foreColorMode = ColorMode.Intelligent,
            //    orientation = Orientation.Horizontal,
            //    reverse = false,
            //    summaryMode = SummaryMode.Large,
            //    textFormat = "<font face=\"Verdana\">",
            //    templateLength = 20,
            //    textRegex = "%count%",
            //    title = "Field",
            //    path = Directory.GetCurrentDirectory()

            //});
            //sw.Close();
        }
        static void HistoryTreeFieldTest()
        {
            //Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));

            BitmapImage b = new BitmapImage(new Uri("./NewField.jpg", UriKind.RelativeOrAbsolute));
            WriteableBitmap wb = new WriteableBitmap(b);
            FieldParameters p = new FieldParameters(wb, new List<DominoColor>(), 8, 8, 24, 8, 10000, BitmapScalingMode.HighQuality, DitherMode.NoDithering, ColorDetectionMode.CieDe2000Comparison);
            p.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            p.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            p.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            p.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            p.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            p.colors.Add(new DominoColor(Colors.White, 1000, "white"));
            //FieldParameters state = p.current.getState();
            wb.Freeze();
            p.current.getState();





            //saveDominoTransfer("Tests/before_resize", state.Generate());
            //ChangeDimensionOperation<FieldParameters> dims = new ChangeDimensionOperation<FieldParameters>(p.current) {width= 10, length= 10};

            //p.current = dims;
            //saveDominoTransfer("Tests/after_resize", p.current.getState().Generate());
        }
        public static void saveDominoTransfer(string path, DominoTransfer transfer)
        {
            Console.WriteLine("Size: " + transfer.dominoes.Count());
            WriteableBitmap b2 = transfer.GenerateImage(2000, false);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(b2));
            using (var stream = new FileStream(Path.Combine(path, "FieldTest.png"), FileMode.Create))
                encoder.Save(stream);
        }

    }
}
