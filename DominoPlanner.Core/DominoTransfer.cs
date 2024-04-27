using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System.Diagnostics;

namespace DominoPlanner.Core
{
    [ProtoContract(SkipConstructor =true)]
    public class DominoTransfer : ICloneable
    {
        [ProtoMember(1)]
        public IDominoShape[] shapes;
        //[ProtoMember(3, AsReference = true)]
        public ColorRepository colors;
        [ProtoMember(4, AsReference = true)]
        public IterationInformation IterationInfo {get; set;}
        public int Length
        {
            get { return shapes.Length; }
        }
        public int FieldPlanLength
        {
            get
            {
                return shapes.Max(x => (x.position != null) ? x.position.x : 0) + 1;
            }
        }
        public int FieldPlanHeight
        {
            get
            {
                return shapes.Max(y => (y.position != null) ? y.position.y : 0) + 1;
            }
        }
        public int PhysicalLength
        {
            get
            {
                return shapes.Max(x => x.GetContainer().x2) + 1;
            }
        }
        public int PhysicalHeight
        {
            get
            {
                return shapes.Max(y => y.GetContainer().y2) + 1;
            }
        }
        public int PhysicalExpandedLength
        {
            get
            {
                return shapes.Max(x => x.GetContainer(expanded: true).x2) + 1;
            }
        }
        public int PhysicalExpandedHeight
        {
            get
            {
                return shapes.Max(y => y.GetContainer(expanded: true).y2) + 1;
            }
        }
        public IDominoShape this[int index]
        {
            get
            {
                return shapes[index];
            }
        }
        public DominoTransfer(IDominoShape[] shapes, ColorRepository colors)
        {
            this.shapes = shapes;
            this.colors = colors;
        }
        public SKSurface GenerateImage(int targetWidth = 0, bool borders = false)
        {
            return GenerateImage(Colors.White, targetWidth, borders);
        }
        public SKSurface GenerateImage(Color background, int targetWidth = 0, bool borders = false, bool expanded = false, int xShift = 5, int yShift = 5, int colorType = 0)
        {
            DateTime time = DateTime.Now;
            double scalingFactor = 1;
            int width = shapes.Max(s => s.GetContainer().x2);
            int heigth = shapes.Max(s => s.GetContainer().y2);
            if (targetWidth != 0)
            {
                // get dimensions of the structure

                //int x_min = shapes.Min(s => s.GetContainer().x1);
                //int y_min = shapes.Min(s => s.GetContainer().y1);
                scalingFactor = Math.Min((double)targetWidth / width, (double)targetWidth/heigth);
            }
            var info = new SKImageInfo((int)(width * scalingFactor) + 2 * xShift, (int)(heigth * scalingFactor) + 2 * yShift);
            SKSurface surf = SKSurface.Create(info);

            SKCanvas canvas = surf.Canvas;
            canvas.Clear(new SKColor(background.R, background.G, background.B, background.A));

            Parallel.For(0, shapes.Length, (i) =>
            {
                Color c;
                if (colorType == 0)
                {
                    c = colors[shapes[i].Color].mediaColor;
                }
                else if (colorType == 1)
                {
                    c = Color.FromArgb((byte)shapes[i].PrimaryDitherColor.Alpha, (byte)shapes[i].PrimaryDitherColor.Red,
                    (byte)shapes[i].PrimaryDitherColor.Green, (byte)shapes[i].PrimaryDitherColor.Blue);
                }
                else
                {
                    c = Color.FromArgb((byte)shapes[i].PrimaryOriginalColor.Alpha, (byte)shapes[i].PrimaryOriginalColor.Red,
                       (byte)shapes[i].PrimaryOriginalColor.Green, (byte)shapes[i].PrimaryOriginalColor.Blue);
                }
                if (shapes[i] is RectangleDomino)
                {
                    DominoRectangle rect = shapes[i].GetContainer(scalingFactor, expanded);
                    if (c.A != 0)
                    {
                        canvas.DrawRect((float)rect.x + xShift, (float)rect.y + yShift, (float)rect.width, (float)rect.height,
                            new SKPaint() { Color = new SKColor(c.R, c.G, c.B, c.A), IsAntialias = true  });
                    }
                    if (borders)
                    {
                        canvas.DrawRect((float)rect.x + xShift, (float)rect.y + yShift, (float)rect.width, (float)rect.height,
                            new SKPaint() { Color = new SKColor(0, 0, 0, 255), IsAntialias = false, IsStroke=true, StrokeWidth=1});
                    }
                }
                else
                {
                    DominoPath shape = shapes[i].GetPath(scalingFactor);
                    var sdpoints = shape.getSDPath(xShift, yShift);
                    if (sdpoints.Length != 0)
                    {
                        var path = new SKPath();
                        path.MoveTo(sdpoints[0].X, sdpoints[0].Y);
                        foreach (var line in sdpoints.Skip(0))
                            path.LineTo(line.X, line.Y);
                        path.Close();

                        if (c.A != 0)
                        {
                            canvas.DrawPath(path,
                                new SKPaint() { Color = new SKColor(c.R, c.G, c.B, c.A), IsAntialias = true, IsStroke = false });
                        }
                        if (borders)
                        {
                            canvas.DrawPath(path,
                                new SKPaint() { Color = new SKColor(0, 0, 0, 255), IsAntialias = true, IsStroke = true, StrokeWidth = 1 });
                        }
                    }
                }
            });
            Debug.WriteLine("Image export took " + (DateTime.Now - time).TotalMilliseconds + "ms");
            return surf;
        }

        public SKSurface GenerateFloorPlan(int dpi = 300)
        {
            Color background = Colors.Transparent;
            int xShift = 5;
            int yShift = 5;
            int colorType = 0;

            DateTime time = DateTime.Now;
            double scalingFactor = 1;
            int width = shapes.Max(s => s.GetContainer().x2);
            int heigth = shapes.Max(s => s.GetContainer().y2);

            int targetWidth = (int)(width / 25.4 * (double)dpi);

            if (targetWidth != 0)
            {
                // get dimensions of the structure
                scalingFactor = Math.Min((double)targetWidth / width, (double)targetWidth / heigth);
            }
            var info = new SKImageInfo((int)(width * scalingFactor) + 2 * xShift, (int)(heigth * scalingFactor) + 2 * yShift);
            SKSurface surf = SKSurface.Create(info);

            SKCanvas canvas = surf.Canvas;
            canvas.Clear(new SKColor(background.R, background.G, background.B, background.A));

            SKPaint paint = new SKPaint();
            paint.TextSize = 64.0f;
            paint.IsAntialias = true;
            paint.Color = new SKColor(0x42, 0x81, 0xA4);
            paint.IsStroke = false;

            Parallel.For(0, shapes.Length, (i) =>
            {
                {
                    Color c;
                    IDominoColor dominoColor = null;
                    if (colorType == 0)
                    {
                        dominoColor = colors[shapes[i].Color];
                        c = dominoColor.mediaColor;
                    }
                    else if (colorType == 1)
                    {
                        c = Color.FromArgb((byte)shapes[i].PrimaryDitherColor.Alpha, (byte)shapes[i].PrimaryDitherColor.Red,
                        (byte)shapes[i].PrimaryDitherColor.Green, (byte)shapes[i].PrimaryDitherColor.Blue);
                    }
                    else
                    {
                        c = Color.FromArgb((byte)shapes[i].PrimaryOriginalColor.Alpha, (byte)shapes[i].PrimaryOriginalColor.Red,
                           (byte)shapes[i].PrimaryOriginalColor.Green, (byte)shapes[i].PrimaryOriginalColor.Blue);
                    }

                    if (c.A != 0)
                    {
                        if (shapes[i] is RectangleDomino)
                        {
                            DominoRectangle rect = shapes[i].GetContainer(scalingFactor, false);

                            canvas.DrawRect((float)rect.x + xShift, (float)rect.y + yShift, (float)rect.width, (float)rect.height,
                                new SKPaint() { Color = new SKColor(0, 0, 0, 255), IsAntialias = true, IsStroke = true, StrokeWidth = 1 });

                            //todo add Text
                        }
                        else
                        {
                            DominoPath shape = shapes[i].GetPath(scalingFactor);
                            var sdpoints = shape.getSDPath(xShift, yShift);
                            if (sdpoints.Length != 0)
                            {
                                var path = new SKPath();
                                path.MoveTo(sdpoints[0].X, sdpoints[0].Y);

                                var textpath = new SKPath();
                                float startX = sdpoints[1].X - ((sdpoints[1].X - sdpoints[2].X) / 2.5f);
                                float startY = sdpoints[1].Y - ((sdpoints[1].Y - sdpoints[2].Y) / 2.5f);

                                float endX = sdpoints[0].X - ((sdpoints[0].X - sdpoints[3].X) / 2.5f);
                                float endY = sdpoints[0].Y - ((sdpoints[0].Y - sdpoints[3].Y) / 2.5f);

                                textpath.MoveTo(startX, startY);
                                textpath.LineTo(endX, endY);

                                foreach (var line in sdpoints.Skip(0))
                                    path.LineTo(line.X, line.Y);
                                path.Close();

                                using (SKPaint pathPaint = new SKPaint())
                                {
                                    pathPaint.Color = new SKColor(0, 0, 0, 255);
                                    pathPaint.IsAntialias = true;
                                    pathPaint.IsStroke = true;
                                    pathPaint.StrokeWidth = 1;
                                    canvas.DrawPath(path, pathPaint);
                                }

                                using (var paintTest = new SKPaint())
                                {
                                    paintTest.TextSize = 48f;
                                    paintTest.IsAntialias = true;
                                    paintTest.Color = new SKColor(0, 0, 0, 255);
                                    paintTest.IsStroke = false;
                                    paintTest.StrokeWidth = 0;
                                    paintTest.TextAlign = SKTextAlign.Center;

                                    canvas.DrawTextOnPath(dominoColor.name, textpath, new SKPoint(), paintTest);
                                }
                            }
                        }
                    }
                }
            });

            Debug.WriteLine("Image export took " + (DateTime.Now - time).TotalMilliseconds + "ms");
            return surf;

        }

        public object Clone()
        {
            return Serializer.DeepClone<DominoTransfer>(this);
        }
    }

    
}
