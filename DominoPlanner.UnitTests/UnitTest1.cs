using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emgu.CV;
using DominoPlanner.Core;

namespace DominoPlanner.UnitTests
{
    [TestClass]
    public class FieldTests
    {
        [TestMethod]
        public void CheckTransparency()
        {
            Mat mat = CvInvoke.Imread("./images/transparency.png", ImreadModes.Unchanged);
            FieldParameters p = new FieldParameters(mat, new List<DominoColor>(), 8, 8, 24, 8, 10000, Inter.Lanczos4,
                DitherMode.NoDithering, ColorDetectionMode.Cie94Comparison);
            p.colors.Add(new DominoColor(Colors.Black, 1000, "black"));
            p.colors.Add(new DominoColor(Colors.Blue, 1000, "blue"));
            p.colors.Add(new DominoColor(Colors.Green, 1000, "green"));
            p.colors.Add(new DominoColor(Colors.Yellow, 1000, "yellow"));
            p.colors.Add(new DominoColor(Colors.Red, 1000, "red"));
            p.colors.Add(new DominoColor(Colors.White, 1000, "white"));
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //DominoTransfer t = await Dispatcher.CurrentDispatcher.Invoke(async () => await Task.Run(() => p.Generate()));

            DominoTransfer t = p.Generate();
        }
    }
}
