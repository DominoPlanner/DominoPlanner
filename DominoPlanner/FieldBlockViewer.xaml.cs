using DominoPlanner.Document_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DominoPlanner
{
    /// <summary>
    /// Interaction logic for FieldBlockViewer.xaml
    /// </summary>
    public partial class FieldBlockViewer : Window
    {
        public FieldDocument field { get; set; }
        FieldPlanHelper p;
        public int BlockSize { get; set; }
        public int block { get; set; }
        public int row { get; set; }
        public FieldBlockViewer()
        {
            InitializeComponent();
            row = 0;
            block = 0;
            this.KeyDown += new System.Windows.Input.KeyEventHandler(Window_KeyDown);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Window_MouseLeftButtonDown);

        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                case Key.Space:
                case Key.Enter: // next block
                    if (block != p.colors.GetLength(1) - 1)
                    {
                        block++;
                    }
                    else if (row != p.colors.GetLength(0) - 1)
                    {
                        row++;
                        block = 0;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Don't forget to press record :)");
                        this.Close();

                    }
                    break;
                case Key.Left: // previous block
                    if (block != 0)
                    {
                        block--;
                    }
                    else if (row != 0)
                    {
                        row--;
                        block = p.colors.GetLength(1) - 1;
                    }
                    break;
                case Key.Up: // row up
                    if (row != 0)
                    {
                        row--;
                    }
                    break;
                case Key.Down: // row down
                    if (row != p.colors.GetLength(0) - 1)
                    {
                        row++;
                    }
                    break;
            }
            ReDraw();
        }

        private void ReDraw()
        {

            numBlocks.Value = block + 1;
            numRows.Value = row + 1;
            Battery.Content = SystemInformation.PowerStatus.BatteryLifePercent * 100 + "%"
                + (((SystemInformation.PowerStatus.BatteryChargeStatus & BatteryChargeStatus.Charging) == BatteryChargeStatus.Charging) ? ", charging" : "");
            Time.Content = DateTime.Now.ToShortTimeString();
            Bitmap b = new Bitmap(200 * BlockSize, 2100);
            Graphics g = Graphics.FromImage(b);
            List<DominoColor> currentcolors = new List<DominoColor>();
            for (int i = 0; i < p.colors[row, block].Length; i++)
            {
                int currentcount = p.counts[row, block][i];

                for (int j = 0; j < currentcount; j++)
                {
                    currentcolors.Add(p.colors[row, block][i]);
                }
            }
            for (int i = 0; i < currentcolors.Count; i++)
            {
                // draw dominoes
                g.FillRectangle(new SolidBrush(SMtoSD(currentcolors[i].rgb)), i * 200 + 10, 10, 180, 960);
                g.DrawRectangle(new Pen(Brushes.Black, 10), i * 200 + 10, 10, 180, 960);
            }
            int counter = 0;
            string[] texts = new String[(int)Math.Ceiling((double)p.colors[row, block].Length / 5d)];
            for (int i = 0; i < p.colors[row, block].Length; i++)
            {
                int count = p.counts[row, block][i];
                int fontsize = (count == 1) ? 32 : 50;
                if (count == 2) fontsize = 40;
                using (Font font1 = new Font("Segoe UI", 70, System.Drawing.FontStyle.Bold, GraphicsUnit.Point))
                {
                    StringFormat f = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                    g.DrawString(p.counts[row, block][i] + "", font1, Brushes.Black, new RectangleF(counter * 200, 1010, count * 200, 80), f);
                }
                using (Font font1 = new Font("Segoe UI", fontsize, GraphicsUnit.Point))
                {
                    StringFormat f = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    string s = p.colors[row, block][i].name.Replace('_', ' ');
                    g.DrawString(s, font1, Brushes.Black, new RectangleF(counter * 200, 1080, count * 200, 150), f);
                }
                counter += count;
                texts[i / 5] += p.counts[row, block][i] + " " + p.colors[row, block][i].name.Replace('_', ' ') + "\n";
            }
            g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(255, 150, 150, 150)), 8), 10, 1250, b.Width - 10, 1250);
            g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(255, 150, 150, 150)), 8), 10, 1265, b.Width - 10, 1265);
            for (int i = 0; i < texts.Length; i++)
            {
                using (Font font1 = new Font("Segoe UI", 60, System.Drawing.FontStyle.Regular, GraphicsUnit.Point))
                {
                    int x = 1310;
                    foreach (string s in texts[i].Split('\n'))
                    {
                        g.DrawString(s, font1, Brushes.Black, new RectangleF(i * 1000, x, 900, 1200));
                        x += 150;
                    }
                }
            }
            PictBx.Image = b;
            GC.Collect();
            PictBx.SizeMode = PictureBoxSizeMode.Zoom;


        }
        public static Color SMtoSD(System.Windows.Media.Color c)
        {
            return Color.FromArgb(c.R, c.G, c.B);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            p = FieldPlanDocument.CalculateFieldplan(field, BlockSize);
            ReDraw();
            MaxRows.Content = p.colors.GetLength(0);
        }

        private void numRows_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (numRows.Value <= 0)
                numRows.Value = 1;
            if(numRows.Value >= p.colors.GetLength(0))
                numRows.Value = p.colors.GetLength(0);
            row = (int) numRows.Value - 1;
            ReDraw();
        }

        private void numBlocks_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (numBlocks.Value <= 0)
                numBlocks.Value = 1;
            if (numBlocks.Value >= p.colors.GetLength(1))
                numBlocks.Value = p.colors.GetLength(1);
            block = (int)numBlocks.Value - 1;
            ReDraw();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (block != p.colors.GetLength(1) - 1)
            {
                block++;
            }
            else if (row != p.colors.GetLength(0) - 1)
            {
                row++;
                block = 0;
            }
            else
            {
                System.Windows.MessageBox.Show("Don't forget to press record :)");
                this.Close();
            }
            ReDraw();
        }

        private void PictBx_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Window_MouseLeftButtonDown(null, null);
        }
    }
}
