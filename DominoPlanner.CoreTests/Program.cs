using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using DominoPlanner.Core;
using System.Xml.Linq;
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
            Thread.Sleep(500);
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
            //CircleTest("bird.jpg");
            for (int i = 0; i < 1; i++)
            //    SpiralTest("bird.jpg");
            //WallTest("tests/bird.jpg");
            FieldTest("bird.jpg");
            //ColorRepoSaveTest();
            //var result1 = ColorRepoLoadTest("colors.DColor");
            //var result2 = ColorRepoLoadTest("colors.DColor");
            //Console.WriteLine($"zurückgegebene Objekte identisch? { result1 == result2}");
            //for (int i = 0; i < result1.Length; i++)
            //{
            //    Console.WriteLine(result1[i].name + " " + (i > 0 ? result1.Anzeigeindizes[i-1] : -1));
            //}
            //Console.WriteLine(String.Join("\n", result1.RepresentionForCalculation.Select(x => $"{x.name}, {x.mediaColor}").ToArray()));

            //FieldTest("tests/bird.jpg");

            //Console.WriteLine(Test());
            Console.ReadLine();
            
        }

        private static void ColorRepoSaveTest()
        {
            var repo = new ColorRepository();
            repo.Add(new DominoColor(Color.FromArgb(255, 17, 17, 16), 1000, "schwarz"));
            repo.Add(new DominoColor(Color.FromArgb(255, 38, 36, 36), 1000, "dunkelgrau"));
            repo.Add(new DominoColor(Color.FromArgb(255, 124, 119, 115), 1000, "silber"));
            repo.Add(new DominoColor(Color.FromArgb(255, 170, 164, 155), 1000, "grau"));
            repo.Add(new DominoColor(Color.FromArgb(255, 39, 24, 17), 1000, "braunschwarz"));
            repo.Add(new DominoColor(Color.FromArgb(255, 84, 38, 19), 1000, "braun"));
            repo.Add(new DominoColor(Color.FromArgb(255, 133, 56, 23), 1000, "kupfer"));
            repo.Add(new DominoColor(Color.FromArgb(255, 151, 90, 27), 1000, "ocker"));
            repo.Add(new DominoColor(Color.FromArgb(255, 151, 99, 71), 1000, "hellbraun"));
            repo.Add(new DominoColor(Color.FromArgb(255, 46, 13, 36), 1000, "dunkelviolett"));
            repo.Add(new DominoColor(Color.FromArgb(255, 86, 31, 70), 1000, "lila"));
            repo.Add(new DominoColor(Color.FromArgb(255, 158, 34, 104), 1000, "hellviolett"));
            repo.Add(new DominoColor(Color.FromArgb(255, 193, 126, 144), 1000, "pastellviolett"));
            repo.Add(new DominoColor(Color.FromArgb(255, 6, 46, 184), 1000, "blau"));
            repo.Add(new DominoColor(Color.FromArgb(255, 70, 131, 191), 1000, "hellblau"));
            repo.Add(new DominoColor(Color.FromArgb(255, 51, 170, 142), 1000, "gelb"));
            repo.Add(new DominoColor(Color.FromArgb(255, 228, 160, 82), 1000, "maisgelb"));
            repo.Add(new DominoColor(Color.FromArgb(255, 229, 184, 134), 1000, "sandgelb"));
            repo.Add(new DominoColor(Color.FromArgb(255, 135, 98, 10), 1000, "gold"));
            repo.Add(new DominoColor(Color.FromArgb(255, 236, 78, 17), 1000, "orange"));
            repo.Add(new DominoColor(Color.FromArgb(255, 252, 80, 60), 1000, "orangerot"));
            repo.Add(new DominoColor(Color.FromArgb(255, 210, 64, 74), 1000, "altrosa"));
            repo.Add(new DominoColor(Color.FromArgb(255, 207, 30, 22), 1000, "rot"));
            repo.Add(new DominoColor(Color.FromArgb(255, 186, 45, 36), 1000, "blutorange"));
            repo.Add(new DominoColor(Color.FromArgb(255, 146, 23, 26), 1000, "himbeerrot"));
            repo.Add(new DominoColor(Color.FromArgb(255, 255, 42, 80), 1000, "leuchtrot"));
            repo.Add(new DominoColor(Color.FromArgb(255, 230, 127, 121), 1000, "rosa"));
            repo.Add(new DominoColor(Color.FromArgb(255, 255, 54, 196), 1000, "pink"));
            repo.Add(new DominoColor(Color.FromArgb(255, 242, 241, 193), 1000, "elfenbein"));
            repo.Add(new DominoColor(Color.FromArgb(255, 230, 230, 230), 2000, "weiß"));
            repo.MoveUp((DominoColor) repo[3]);
            Console.WriteLine(String.Join(", ", repo.SortedRepresentation.Select(x => $"{x.name}").ToArray()));
            repo.MoveUp((DominoColor)repo[3]);
            Console.WriteLine(String.Join(", ", repo.SortedRepresentation.Select(x => $"{x.name}").ToArray()));
            repo.MoveDown((DominoColor)repo[3]);
            Console.WriteLine(String.Join(", ", repo.SortedRepresentation.Select(x => $"{x.name}").ToArray()));
            repo.MoveUp((DominoColor)repo[4]);
            Console.WriteLine(String.Join(", ", repo.SortedRepresentation.Select(x => $"{x.name}").ToArray()));
            repo.Save("colors.DColor");
        }
        private static ColorRepository ColorRepoLoadTest(String path)
        {
            return Workspace.Load<ColorRepository>(path);
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
            Console.WriteLine("=======   FIELD TEST   =======");
            //Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
            //Mat mat = CvInvoke.Imread(path, ImreadModes.Unchanged);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            FieldParameters p = new FieldParameters(path, "colors.DColor", 8, 8, 24, 8, 20000, Inter.Lanczos4,
                new Dithering(), ColorDetectionMode.CieDe2000Comparison, new NoColorRestriction());
            p.TransparencySetting = 128;
            DominoTransfer t = p.Generate();
            p.background = System.Windows.Media.Colors.White;
            //DominoTransfer t = await Dispatcher.CurrentDispatcher.Invoke(async () => await Task.Run(() => p.Generate()));
            // Filtertests für ein Logo oder ähnliches
            /*var erster = ((BlendFileFilter)p.ImageFilters[0]);
            erster.RotateAngle = 30;
            erster.ScaleX = 0.5;
            erster.ScaleY = 0.3;
            erster.CenterX = erster.GetSizeOfMat().Width * 0.6;
            erster.CenterY = erster.GetSizeOfMat().Height * 0.6;
            var zweiter = new BlendTextFilter();
            zweiter.FontSize = 40;
            zweiter.FontFamily = "Calibri";
            zweiter.Text = "Hallo Welt";
            zweiter.Color = System.Drawing.Color.Green;
            p.ImageFilters.Add(zweiter);
            zweiter.CenterX = 500;
            zweiter.CenterY = 500;*/
            // Filtertests für Bilder
            // Helligkeit / Kontrast
            /*var hell = new ContrastLightFilter();
            hell.Alpha = 1.5;
            hell.Beta = -150;
            p.ImageFilters.Add(hell);*/
            //var gamma = new GammaCorrectFilter();
            //gamma.Gamma = 1.1;
            //p.ImageFilters.Add(gamma);

            /*var blur = new GaussianSharpenFilter();
            blur.KernelSize = 21;
            blur.StandardDeviation = 5;
            blur.SharpenWeight = 0.5;
            p.ImageFilters.Add(blur);*/
            /*var replace = new ReplaceColorFilter();
            replace.BeforeColor = System.Drawing.Color.LightBlue;
            replace.AfterColor = System.Drawing.Color.Red;
            replace.Tolerance = 50;
            p.ImageFilters.Add(replace);*/
            t = p.Generate();
            Console.WriteLine(String.Join("\n", p.counts));
            Console.WriteLine("Size: " + t.dominoes.Count());
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch = System.Diagnostics.Stopwatch.StartNew();
            Mat b2 = t.GenerateImage(Colors.Transparent);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            b2.Save("tests/FieldTest.png");
            FileStream fs = new FileStream(@"tests/FieldPlanTest.html", FileMode.Create);
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
            /*p.SaveXLSFieldPlan("tests/ExcelFieldPlanTest.xlsx", new ObjectProtocolParameters()
            {
                backColorMode = ColorMode.Normal,
                foreColorMode = ColorMode.Intelligent,
                orientation = Core.Orientation.Horizontal,
                reverse = false,
                summaryMode = SummaryMode.Large,
                textFormat = "<font face=\"Verdana\">",
                templateLength = 20,
                textRegex = "%count%",
                title = "Field",
                path = Directory.GetCurrentDirectory()

            });*/
            sw.Close();
            p.Save("FieldTest.DObject");
            watch = Stopwatch.StartNew();
            int[] counts = Workspace.LoadColorList<FieldParameters>("FieldTest.DObject");
            watch.Stop();
            Console.WriteLine("Preview Load Time: " + watch.ElapsedMilliseconds);
            Console.WriteLine(String.Join(", ", counts));
            
            watch = Stopwatch.StartNew();
            FieldParameters loaded = Workspace.Load<FieldParameters>("FieldTest.DObject");
            watch.Stop();
            Console.WriteLine("Load Time: " + watch.ElapsedMilliseconds);
            //loaded.last.GenerateImage(Colors.Transparent).Save("tests/afterLoad.png");
            Console.WriteLine(p.colors == loaded.colors);
            watch = Stopwatch.StartNew();
            int[] counts2 = Workspace.LoadColorList<FieldParameters>("FieldTest.DObject");
            watch.Stop();
            Console.WriteLine("Preview Load Time: " + watch.ElapsedMilliseconds);
            Console.WriteLine(String.Join(", ", counts2));
            Console.WriteLine("Number of image filters: " + loaded.ImageFilters.Count);
            //loaded.ImageHeight = 1500;
            loaded.Generate();
            
            Console.WriteLine(String.Join(", ", loaded.counts));
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
            Console.WriteLine("=======   SPIRAL TEST   =======");
            Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));

            //Mat mat = CvInvoke.Imread(path, ImreadModes.AnyColor);
            SpiralParameters p = new SpiralParameters(path, 7, "colors.DColor", 
                ColorDetectionMode.CieDe2000Comparison, AverageMode.Corner, new NoColorRestriction());
            p.ThetaMin = 0.3d * Math.PI;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //DominoTransfer t = await Dispatcher.CurrentDispatcher.Invoke(async () => await Task.Run(() => p.Generate(wb, progress)));
            
            DominoTransfer t = p.Generate();
            Console.WriteLine(t.length);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch = System.Diagnostics.Stopwatch.StartNew();
            Mat b2 = t.GenerateImage(Colors.Transparent);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            b2.Save("tests/SpiralTest.png");
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
            p.Save("spiral.DObject");
            IDominoProvider c = Workspace.Load<IDominoProvider>("spiral.DObject");

        }
        static void CircleTest(string path)
        {
            Console.WriteLine("=======   CIRCLE TEST   =======");
            Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));

            //Mat mat = CvInvoke.Imread(path, ImreadModes.AnyColor);
            CircleParameters p = new CircleParameters(path, 50, "colors.DColor",
                ColorDetectionMode.CieDe2000Comparison, AverageMode.Corner, new NoColorRestriction());
            p.AngleShiftFactor = -0.02;
            p.ForceDivisibility = 5;
            p.StartDiameter = 200;
            
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
            p.Save("circle.DObject");
            IDominoProvider c = Workspace.Load<IDominoProvider>("circle.DObject");
        }
        static void WallTest(String path)
        {
            Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
            Mat mat = CvInvoke.Imread(path, ImreadModes.Unchanged);
            StreamReader sr = new StreamReader(new FileStream("Structures.xml", FileMode.Open));
            XElement xml = XElement.Parse(sr.ReadToEnd());
            StructureParameters p = new StructureParameters(path, xml.Elements().ElementAt(1), 10000, 
                "colors.DColor", ColorDetectionMode.CieDe2000Comparison, 
                AverageMode.Corner, new NoColorRestriction(), true);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //DominoTransfer t = await Dispatcher.CurrentDispatcher.Invoke(async () => await Task.Run(() => p.Generate(wb, progress)));

            DominoTransfer t = p.Generate();
            Console.WriteLine("Size: " + t.dominoes.Count());
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch = System.Diagnostics.Stopwatch.StartNew();
            Mat b2 = t.GenerateImage(borders: false);
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
            p.Save("structure.DObject");
            StructureParameters p2 = Workspace.Load<StructureParameters>("structure.DObject");
        }
        static void HistoryTreeFieldTest(String path)
        {
            //Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));

            BitmapImage b = new BitmapImage(new Uri("./NewField.jpg", UriKind.RelativeOrAbsolute));
            Mat mat = CvInvoke.Imread(path, ImreadModes.AnyColor);
            FieldParameters p = new FieldParameters(path, "colors.DColor", 8, 8, 24, 8, 10000, Inter.Lanczos4,
                new Dithering(), ColorDetectionMode.CieDe2000Comparison, new NoColorRestriction());
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
