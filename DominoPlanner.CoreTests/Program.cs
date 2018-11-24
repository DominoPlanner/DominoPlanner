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
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using DominoPlanner.Core.Dithering;

namespace DominoPlanner.CoreTests
{
    class Program
    {
        static void Main(string[] args)

        {
            Thread.Sleep(3000);
            Workspace.Instance.root_path = Path.GetFullPath("tests");
            Console.WriteLine($"Rootpfad des Workspaces: {Workspace.Instance.root_path}");
            
            //HistoryTreeFieldTest("tests/NewField.jpg");
            /*try
            {
                for (int i = 0; i < 10; i++)
                {
                    FieldTest("tests/mars.jpg");
                    Thread.Sleep(2000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }*/
            //CircleTest("tests/NewField.jpg");
            //
            //WallTest("tests/NewField.jpg");
            //FieldTest("tests/NewField.jpg");
            ColorRepoSaveTest();
            var result1 = ColorRepoLoadTest("colors.xml");
            var result2 = ColorRepoLoadTest("colors.xml");
            Console.WriteLine($"zurückgegebene Objekte identisch? { result1 == result2}");
            for (int i = 0; i < result1.Length; i++)
            {
                Console.WriteLine(result1[i].name + " " + (i > 0 ? result1.Anzeigeindizes[i-1] : -1));
            }
            Console.WriteLine(String.Join("\n", result1.RepresentionForCalculation.Select(x => $"{x.name}, {x.mediaColor}").ToArray()));

            //Console.WriteLine(Test());
            Console.ReadLine();
            
        }

        private static void ColorRepoSaveTest()
        {
            var repo = new ColorRepository();
            repo.Add(new DominoColor(Colors.Black, 2000, "black"));
            repo.Add(new DominoColor(Colors.Blue, 2000, "blue"));
            repo.Add(new DominoColor(Colors.Gray, 2000, "gray"));
            repo.Add(new DominoColor(Colors.DarkGreen, 2000, "dark_green"));
            repo.Add(new DominoColor(Colors.Green, 2000, "green"));
            repo.Add(new DominoColor(Colors.Yellow, 2000, "yellow"));
            repo.Add(new DominoColor(Colors.Red, 2000, "red"));
            repo.Add(new DominoColor(Color.FromArgb(255, 230, 230, 230), 2000, "white"));
            repo.MoveUp((DominoColor) repo[3]);
            Console.WriteLine(String.Join(", ", repo.SortedRepresentation.Select(x => $"{x.name}").ToArray()));
            repo.MoveUp((DominoColor)repo[3]);
            Console.WriteLine(String.Join(", ", repo.SortedRepresentation.Select(x => $"{x.name}").ToArray()));
            repo.MoveDown((DominoColor)repo[3]);
            Console.WriteLine(String.Join(", ", repo.SortedRepresentation.Select(x => $"{x.name}").ToArray()));
            repo.MoveUp((DominoColor)repo[4]);
            Console.WriteLine(String.Join(", ", repo.SortedRepresentation.Select(x => $"{x.name}").ToArray()));
            repo.Save("colors.xml");
        }
        private static ColorRepository ColorRepoLoadTest(String path)
        {
            return ColorRepository.Load(path);
        }
        static async Task<String> Test()
        {
            long res = await Task.Run(() => OpenCVTest("NewField.jpg", 1));
            Console.Write(res);
            res = await Task.Run(() => OpenCVTest("NewField.jpg", 10));
            Console.Write(res);
            res = await Task.Run(() => OpenCVTest("NewField.jpg", -1));
            Console.Write(res);
            return "Test";
        }
        static void FieldTest(String path)
        {
            //Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
            Mat mat = CvInvoke.Imread(path, ImreadModes.AnyColor);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            FieldParameters p = new FieldParameters(mat, new List<DominoColor>(), 8, 8, 24, 8, 100000, Inter.Lanczos4,
                new JarvisJudiceNinkeDithering(), ColorDetectionMode.CieDe2000Comparison, new NoColorRestriction());
            p.colors.Add(new DominoColor(Colors.Black, 2000, "black"));
            p.colors.Add(new DominoColor(Colors.Blue, 2000, "blue"));
            p.colors.Add(new DominoColor(Colors.Gray, 2000, "gray"));
            p.colors.Add(new DominoColor(Colors.DarkGreen, 2000, "dark_green"));
            p.colors.Add(new DominoColor(Colors.Green, 2000, "green"));
            p.colors.Add(new DominoColor(Colors.Yellow, 2000, "yellow"));
            p.colors.Add(new DominoColor(Colors.Red, 2000, "red"));
            p.colors.Add(new DominoColor(Color.FromArgb(255, 230, 230, 230), 2000, "white"));

            //DominoTransfer t = await Dispatcher.CurrentDispatcher.Invoke(async () => await Task.Run(() => p.Generate()));

            DominoTransfer t = p.Generate();
            Console.WriteLine("Size: " + t.dominoes.Count());
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch = System.Diagnostics.Stopwatch.StartNew();
            Mat b2 = t.GenerateImage(Colors.Black);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            b2.Save("tests/FieldTest.png");
            FileStream fs = new FileStream(@"FieldPlanTest.html", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            /*sw.Write(p.GetHTMLProcotol(new ObjectProtocolParameters()
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
            }));*/
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
        static void SpiralTest(string path)
        {
            Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));

            Mat mat = CvInvoke.Imread(path, ImreadModes.AnyColor);
            SpiralParameters p = new SpiralParameters(mat, 220, 24, 8, 8, 8, new List<DominoColor>(), 
                ColorDetectionMode.CieDe2000Comparison, AverageMode.Corner, new NoColorRestriction());
            p.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            p.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            p.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            p.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            p.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            p.colors.Add(new DominoColor(Colors.White, 1000, "white"));
            
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //DominoTransfer t = await Dispatcher.CurrentDispatcher.Invoke(async () => await Task.Run(() => p.Generate(wb, progress)));

            DominoTransfer t = p.Generate();
            Console.WriteLine(t.length);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch = System.Diagnostics.Stopwatch.StartNew();
            //Mat b2 = t.GenerateImage(1000, false);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            //b2.Save("tests/SpiralTest.png");
            FileStream fs = new FileStream(@"SpiralPlanTest.html", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(p.GetHTMLProcotol(new ObjectProtocolParameters()
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
        static void CircleTest(string path)
        {
            Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));

            Mat mat = CvInvoke.Imread(path, ImreadModes.AnyColor);
            CircleParameters p = new CircleParameters(mat, 200, 8, 24, 8, 8, new List<DominoColor>(),
                ColorDetectionMode.CieDe2000Comparison, AverageMode.Corner, new NoColorRestriction());
            p.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            p.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            p.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            p.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            p.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            p.colors.Add(new DominoColor(Colors.White, 1000, "white"));

            var watch = System.Diagnostics.Stopwatch.StartNew();
            //DominoTransfer t = await Dispatcher.CurrentDispatcher.Invoke(async () => await Task.Run(() => p.Generate(wb, progress)));

            DominoTransfer t = p.Generate();
            Console.WriteLine(t.length);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch = System.Diagnostics.Stopwatch.StartNew();
            Mat b2 = t.GenerateImage();
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            b2.Save("tests/CircleTest.png");
            FileStream fs = new FileStream(@"SpiralPlanTest.html", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(p.GetHTMLProcotol(new ObjectProtocolParameters()
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
        static void WallTest(String path)
        {
            Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
            Mat mat = CvInvoke.Imread(path, ImreadModes.Unchanged);
            StreamReader sr = new StreamReader(new FileStream("Structures.xml", FileMode.Open));
            XElement xml = XElement.Parse(sr.ReadToEnd());
            StructureParameters p = new StructureParameters(mat, xml.Elements().ElementAt(1), 200000, 
                new List<DominoColor>(), ColorDetectionMode.CieDe2000Comparison, AverageMode.Corner, new NoColorRestriction(), true);
            p.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            p.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            p.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            p.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            p.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            p.colors.Add(new DominoColor(Colors.LightGray, 1000, "white"));
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //DominoTransfer t = await Dispatcher.CurrentDispatcher.Invoke(async () => await Task.Run(() => p.Generate(wb, progress)));

            DominoTransfer t = p.Generate();
            Console.WriteLine("Size: " + t.dominoes.Count());
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch = System.Diagnostics.Stopwatch.StartNew();
            Mat b2 = t.GenerateImage(1000, false);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            b2.Save("tests/WallTest.png");
            sr.Close();
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
        static void HistoryTreeFieldTest(String path)
        {
            //Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));

            BitmapImage b = new BitmapImage(new Uri("./NewField.jpg", UriKind.RelativeOrAbsolute));
            Mat mat = CvInvoke.Imread(path, ImreadModes.AnyColor);
            FieldParameters p = new FieldParameters(mat, new List<DominoColor>(), 8, 8, 24, 8, 10000, Inter.Lanczos4,
                new Dithering(), ColorDetectionMode.CieDe2000Comparison, new NoColorRestriction());
            p.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            p.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            p.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            p.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            p.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            p.colors.Add(new DominoColor(Colors.White, 1000, "white"));
            FieldParameters state = p.current.getState();
            state.Generate().GenerateImage(2000, false).Save("Tests/before_resize");
            ChangeDimensionOperation<FieldParameters> dims = new ChangeDimensionOperation<FieldParameters>(p.current) {width= 10, length= 10};
            p.current = dims;
            p.current.getState().Generate().GenerateImage(2000, false).Save("Tests/after_resize");
        }
        public static async Task<long> OpenCVTest(string path, int threads)
        {
            Console.WriteLine("called");
            int progress = 0;
            Stopwatch watch = Stopwatch.StartNew();
            Mat mat = CvInvoke.Imread(path, ImreadModes.AnyColor);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds + " öffnen");
            watch = Stopwatch.StartNew();
            Image<Rgb, Byte> img = mat.ToImage<Rgb, Byte>();
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds + " konvertieren");
            await Task.Run(() =>
            {
                // Iterate over the first 10 lines
                Parallel.For(0, img.Height, new ParallelOptions { MaxDegreeOfParallelism = threads }, (i) =>
                {
                    for (int j = 0; j < img.Width; j++)
                    {
                        //Console.WriteLine("Pixel Data at " + i + ","  + j + ": " + img[i, 0]);

                    }
                    if (i % 200 == 0)
                    {
                        Interlocked.Increment(ref progress);
                        //Console.WriteLine("Completed rows: " + progress * 200);
                     }
                    //Thread.Sleep(1000);
                });

            });
            
            
            return watch.ElapsedMilliseconds;

        }



    }
}
