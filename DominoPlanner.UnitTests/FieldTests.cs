using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emgu.CV;
using DominoPlanner.Core;
using DominoPlanner.Core.Dithering;
using Emgu.CV.CvEnum;
using System.Windows.Media;

namespace DominoPlanner.UnitTests
{
    [TestClass]
    public class FieldTests
    {
        public void InitializeWorkspace()
        {
            Workspace.Instance.root_path = "tests";
        }
        public void CreateField()
        {
            
        }
        [TestMethod]
        public void CheckTransparency()
        {
            InitializeWorkspace();
            FieldParameters p = new FieldParameters("image_transparency.png", "colors.DColor", 8, 8, 24, 8, 20000, Inter.Lanczos4,
                new Dithering(), ColorDetectionMode.CieDe2000Comparison, new NoColorRestriction());
            p.TransparencySetting = 128;
            DominoTransfer t = p.Generate();
            
            Assert.AreEqual(0, t.dominoes[0]);
            p.background = Colors.White;
            t = p.Generate();
            Assert.AreEqual(29, t.dominoes[0]);
        }
    }
}
