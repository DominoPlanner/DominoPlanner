using SharpShell.SharpPreviewHandler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DominoPlanner.Core;
using Emgu.CV;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.Diagnostics;
using SharpShell.SharpThumbnailHandler;
using SharpShell.ServerRegistration;
using System.IO;

namespace DominoPlanner.PreviewHandler
{
    //[ComVisible(true)]
    //[COMServerAssociation(AssociationType.ClassOfExtension, ".dobject")]
    //[DisplayName("DominoPlanner Object Preview Handler")]
    //[PreviewHandler(DisableLowILProcessIsolation = false, SurrogateHostType = SurrogateHostType.Prevhost)]
    public class IDominoProviderPreviewHandler : SharpPreviewHandler
    {
        
        protected override PreviewHandlerControl DoPreview()
        {
            var handler = new IDominoProviderPreviewHandlerControl();
            Log("Handler initialized");
            if (!string.IsNullOrEmpty(SelectedFilePath))
                handler.DoPreview(SelectedFilePath);
            return handler;
        }
    }
    public class IDominoProviderPreviewHandlerControl : PreviewHandlerControl
    {
        public IDominoProviderPreviewHandlerControl()
        {
            InitializeComponent();
        }
        public void DoPreview(string filepath)
        {
            Logging.Log("in DoPreview");
            Logging.Log("try to load file " + filepath);
            var obj = Workspace.Load<IDominoProvider>(filepath);
            Logging.Log("object loaded into workspace");
            var preview = obj.last.GenerateImage(200, false);
            Logging.Log("image generated");
            var pictureBox = new PictureBox
            {
                Location = new System.Drawing.Point(0, 0),
                Image = preview.ToImage<Emgu.CV.Structure.Bgra, Byte>().ToBitmap(),
                Width = 200,
                Height = 200,
            };
            panelImages.Controls.Add(pictureBox);
        }
        private System.Windows.Forms.Panel panelImages;

        private void InitializeComponent()
        {
            this.panelImages = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panelImages
            // 
            this.panelImages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelImages.AutoScroll = true;
            this.panelImages.BackColor = System.Drawing.Color.White;
            this.panelImages.Location = new System.Drawing.Point(0, 0);
            this.panelImages.Name = "panelImages";
            this.panelImages.Size = new System.Drawing.Size(288, 237);
            this.panelImages.TabIndex = 1;
            // 
            // IconPreviewHandlerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelImages);
            this.Name = "IconPreviewHandlerControl";
            this.Size = new System.Drawing.Size(288, 237);
            this.ResumeLayout(false);

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private System.ComponentModel.IContainer components = null;
    }
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.FileExtension, ".dobject")]
    [DisplayName("DominoPlanner Object Thumbnail Handler")]
    public class IDominoProviderThumbnail : SharpThumbnailHandler
    {
        public IDominoProviderThumbnail()
        {

        }
        protected override Bitmap GetThumbnailImage(uint width)
        {
            Logging.Log("in GetThumbnailImage");
            byte[] buffer = new byte[SelectedItemStream.Length];
            SelectedItemStream.Read(buffer, 0, (int)SelectedItemStream.Length);
            using (var fs = new MemoryStream(buffer))
            {
                byte[] array = Workspace.LoadThumbnailFromStream(fs);
                Logging.Log("Datei eingelesen");
                Bitmap bmp;
                Bitmap result;
                using (var ms = new MemoryStream(array))
                {
                    bmp = new Bitmap(ms);
                    int largerSide = Math.Max(bmp.Size.Height, bmp.Size.Width);

                    result = new Bitmap(largerSide+2, largerSide+2);
                    
                    Graphics g = Graphics.FromImage(result);
                    g.Clear(System.Drawing.Color.White);
                    
                    g.DrawImage(bmp, largerSide / 2 + 1 - bmp.Size.Width / 2, 
                        largerSide / 2 + 1- bmp.Size.Height / 2, bmp.Size.Width, bmp.Size.Height);
                    g.DrawRectangle(new Pen(Color.Black), new Rectangle(0, 0, largerSide, largerSide));
                    //debug
                    //g.DrawString($"size:{bmp.Size.Width}x{bmp.Size.Height}", new Font("Arial", 5, FontStyle.Regular), Brushes.Black, 5, 5);
                }
                return result;
            }
        }
    }
}
