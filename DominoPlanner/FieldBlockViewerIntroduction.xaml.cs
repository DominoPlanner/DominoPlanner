using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using DominoPlanner.Document_Classes;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DominoPlanner
{
    /// <summary>
    /// Interaction logic for FieldBlockViewerIntroduction.xaml
    /// </summary>
    public partial class FieldBlockViewerIntroduction : Window
    {
        public FieldDocument fld { get; set; }
        public FieldDocument temp { get; set; }
        public List<DominoColor> similar { get; set; }
        public FieldBlockViewerIntroduction()
        {
            InitializeComponent();


           
        }
        private void CheckColors()
        {
            
            int[] isInField = new int[fld.Colors.Count];
            for (int x = 0; x < fld.dominoes.GetLength(0); x++)
            {
                for (int y = 0; y < fld.dominoes.GetLength(1); y++)
                {
                    isInField[fld.dominoes[x, y]]++;
                }
            }
            List<DominoColor> removed = RemovePattern(temp.Colors, isInField);
            CieDe2000Comparison comp = new CieDe2000Comparison();
            List<DominoColor> DuplicateList = new List<DominoColor>();
            List<int> ignoreList = new List<int>();
            for (int i = 0; i < removed.Count - 1; i++)
            {
                if (ignoreList.IndexOf(i) == -1)
                {
                    for (int j = i + 1; j < removed.Count; j++)
                    {
                        if (comp.Compare((new Rgb() { R = removed[i].rgb.R, G = removed[i].rgb.G, B = removed[i].rgb.B }).To<Lab>(),
                            new Rgb() { R = removed[j].rgb.R, G = removed[j].rgb.G, B = removed[j].rgb.B }.To<Lab>()) < 12)
                        {
                            if (ignoreList.IndexOf(i) == -1)
                            {
                                ignoreList.Add(i);
                                DuplicateList.Add(removed[i]);
                            }
                            if (ignoreList.IndexOf(j) == -1)
                            {
                                ignoreList.Add(j);
                                DuplicateList.Add(removed[j]);
                            }
                        }
                    }
                }
            }
            lvColors.ItemsSource = DuplicateList;
            
        }
        private List<DominoColor> RemovePattern(List<DominoColor> list, int[] RemovePattern)
        {
            List<DominoColor> Newlist = new List<DominoColor>();
            for (int i = 0; i < RemovePattern.Length; i++)
            {
                list[i].used_in_projects = new List<int>() { i }; // abuse as temporary index
                list[i].count = RemovePattern[i]; 
                if (RemovePattern[i] != 0)
                {
                    Newlist.Add(list[i]);
                }   
            }
            return Newlist;
        }


        private void Color_Edit(object sender, RoutedEventArgs e)
        {
            DominoColor color = (DominoColor)((Button)sender).DataContext;
            ColorControl c = new ColorControl();
            c.ColorPicker.SelectedColor = color.rgb;
            c.count = color.count;
            c.name = color.name;
            c.ColorOnly = true;
            c.ShowDialog();
            if (c.DialogResult == true)
            {
                color.rgb = c.ColorPicker.SelectedColor;
                color.name = c.name;
                color.count = c.count;
            }
            temp.Colors[color.used_in_projects[0]] = color;
            lvColors.Items.Refresh();
        }

        private void Window_Loaded(object sender, System.EventArgs e)
        {
            if (fld != null)
            {
                FieldDocument fd = new FieldDocument(fld.path, fld.SourcePath, CloneDominoColors(fld.Colors), fld.length);
                fd.dominoes = (int[,]) fld.dominoes.Clone();
                temp = fd;
                CheckColors();
            }
        }
        private List<DominoColor> CloneDominoColors(List<DominoColor> source)
        {
            List<DominoColor> result = new List<DominoColor>();
            foreach (DominoColor d in source)
            {
                DominoColor dom = new DominoColor() { count = d.count, name = d.name, rgb = d.rgb, used_in_projects = d.used_in_projects };
                result.Add(dom);
            }
            return result;
        }
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            
            FieldBlockViewer fbv = new FieldBlockViewer();
            fbv.field = fld;
            fbv.BlockSize = (int) BlockSize.Value;
            fbv.Show();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
