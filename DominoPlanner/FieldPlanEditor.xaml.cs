using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
//using Excel = Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using DominoPlanner.Document_Classes;

namespace DominoPlanner
{
    /// <summary>
    /// Interaction logic for FieldPlanEditor.xaml
    /// </summary>
    public partial class FieldPlanEditor : Window
    {
        public FieldDocument basedoc {
            get; set;
        }
        FieldPlanDocument fpd;
        public FieldPlanEditor()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (basedoc.ColorList == null) this.Title = "Edit Structure Protocol";
            else this.Title = "Edit Field Protocol";
            fpd = new FieldPlanDocument(basedoc);

            this.DataContext = fpd;
        }

        private void SaveHTML(object sender, RoutedEventArgs e)
        {
            FieldPlanDocument d = this.DataContext as FieldPlanDocument;
            d.SaveHTMLDocument();

        }

        private void ShowAdvancedProperties(object sender, RoutedEventArgs e)
        {
            fpd.fpd = new FieldProtocolDetails();
            fpd.fpd.DataContext = fpd;
            fpd.fpd.Show();
        }


        private void WindowClosed(object sender, EventArgs e)
        {
            if (fpd.fpd != null)
                fpd.fpd.Close();
        }

        private void SaveXLSX(object sender, RoutedEventArgs e)
        {
            FieldPlanDocument d = this.DataContext as FieldPlanDocument;
            d.SaveXLSXDocument();
        }
    }
    public static class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
            "Html",
            typeof(string),
            typeof(BrowserBehavior),
            new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser wb = d as WebBrowser;
            if (wb != null)
                wb.NavigateToString(e.NewValue as string);
        }
    }
    public class FieldPlanDocument : INotifyPropertyChanged
    {
        private FieldPlanHelper temp = new FieldPlanHelper();
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(String property)
        {
            if (this.PropertyChanged != null)
            {
                
                if (property.Contains('C'))
                {
                    temp = CalculateFieldplan();
                }
                if (property.Contains('D'))
                {
                    if (temp.colors == null) temp = CalculateFieldplan();
                    TempHTML = GenerateHTMLFieldPlan(temp);
                }
                this.PropertyChanged(this, new PropertyChangedEventArgs(null));
            }
        }
        #region properties
        private string m_title;
        public string title
        {
            get
            {
                return m_title;
            }
            set
            {
                m_title = value;
                OnPropertyChanged("D");
            }
        }
        private string m_text_format;
        public string text_format
        {
            get
            {
                return m_text_format;
            }
            set
            {
                m_text_format = value;
                OnPropertyChanged("D");
            }
        }
        private string m_text_regex;
        public string text_regex
        {
            get
            {
                return m_text_regex;
            }
            set
            {
                m_text_regex = value;
                OnPropertyChanged("D");
            }
        }
        private bool m_use_template;
        public bool use_template
        {
            get
            {
                return m_use_template;
            }
            set
            {
                m_use_template = value;
                if (!m_use_template)
                {
                    backup_template_length = m_template_length;
                    template_length = (BaseDoc.horizontal == true) ? BaseDoc.dominoes.GetLength(0) : BaseDoc.dominoes.GetLength(1);
                }
                else template_length = backup_template_length;
            }
        }
        private int backup_template_length;
        private int m_template_length;
        public int template_length
        {
            get
            {
                return m_template_length;
            }
            set
            {
                m_template_length = value;
                OnPropertyChanged("CD");
            }
        }
        
        
        private SummaryEnum m_summary_selection;
        public SummaryEnum summary_selection
        {
            get
            {
                return m_summary_selection;
            }
            set
            {
                m_summary_selection = value;
                OnPropertyChanged("D");
            }
        }
        private bool m_hide_text;
        public bool hide_text
        {
            get
            {
                return m_hide_text;
            }
            set
            {
                if (value) m_text_regex = "%count%"; else m_text_regex = "%count%&nbsp;%color%";
                m_hide_text = value;
                OnPropertyChanged("D");
            }
        }
        private CollapsedMode m_collapse_borders;
        public CollapsedMode collapse_borders
        {
            get
            {
                return m_collapse_borders;
            }
            set
            {
                m_collapse_borders = value;
                OnPropertyChanged("D");
            }
        }
        private bool? m_build_reverse;
        public bool? build_reverse
        {
            get
            {
                return m_build_reverse;
            }
            set
            {
                m_build_reverse = value;
                OnPropertyChanged("CD");
            }
        }
        private ColorMode m_back_color_mode;
        public ColorMode back_color_mode
        {
            get
            {
                return m_back_color_mode;
            }
            set
            {
                m_back_color_mode = value;
                OnPropertyChanged("D");
            }
        }
        private ColorMode m_fore_color_mode;
        public ColorMode fore_color_mode
        {
            get
            {
                return m_fore_color_mode;
            }
            set
            {
                m_fore_color_mode = value;
                OnPropertyChanged("D");
            }
        }
        private FlowDocument m_doc;
        public FlowDocument doc
        {
            get
            {
                return m_doc;
            }
            set
            {
                m_doc = value;
                OnPropertyChanged("0");
            }
        }
        private String m_TempHTML;
        public String TempHTML {
            get
            {
                return m_TempHTML;
            }
            set
            {
                m_TempHTML = value;
                OnPropertyChanged("TempHTML");
            }
        }
        private Color m_fixed_back_color;
        public Color fixed_back_color
        {
            get
            {
                return m_fixed_back_color;
            }
            set
            {
                m_fixed_back_color = value;
                OnPropertyChanged("D");
            }
        }
        private Color m_fixed_fore_color;
        public Color fixed_fore_color
        {
            get
            {
                return m_fixed_fore_color;
            }
            set
            {
                m_fixed_fore_color = value;
                OnPropertyChanged("D");
            }
        }
        private FieldDocument BaseDoc;
        public FieldProtocolDetails fpd;
        #endregion
        public FieldPlanDocument(FieldDocument BaseDocument)
        {
            
            m_title = System.IO.Path.GetFileNameWithoutExtension(BaseDocument.path);
            BaseDoc = BaseDocument;
            m_use_template = Properties.Settings.Default.UseTemplate;
            m_template_length = Properties.Settings.Default.TemplateLength;
            m_hide_text = Properties.Settings.Default.FieldPlanHideText;
            m_summary_selection = (SummaryEnum)Enum.Parse(typeof(SummaryEnum), Properties.Settings.Default.FieldPlanPropertyMode);
            m_text_format = Properties.Settings.Default.FieldPlanFormatString;
            m_text_regex = Properties.Settings.Default.FieldPlanRegex;
            m_fore_color_mode = (ColorMode)Enum.Parse(typeof(ColorMode), Properties.Settings.Default.FieldPlanForeColorMode);
            m_back_color_mode = (ColorMode)Enum.Parse(typeof(ColorMode), Properties.Settings.Default.FieldPlanBackColorMode);
            m_build_reverse = false;
            TempHTML = GenerateHTMLFieldPlan(CalculateFieldplan());
            m_fixed_fore_color = SDtoSM(Properties.Settings.Default.FieldPlanFixedForeColor);
            m_fixed_back_color = SDtoSM(Properties.Settings.Default.FieldPlanFixedBackColor);

        }
        public void SaveHTMLDocument()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".html";
            dlg.Filter = "Field Plan Document (.html)|*.html";
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;
                
                try
                {
                    FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(TempHTML);
                    fs.Close();
                }
                catch (Exception ex){ MessageBox.Show("Fehler: " + ex.Message); }
            }
        }
        public void SaveXLSXDocument()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Excel Document (.xlsx)|*.xlsx";
            
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;

                try
                {
                    // first method: excel interop
                    //        string filePath = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".html";

                    //        //Creates an HTML file from the web browser and save it to a temp location
                    //        File.WriteAllText(filePath, TempHTML);

                    //        //Creates a new instance of Microsoft office interop for using MS Excel
                    //        Excel.Application excelApp = new Excel.Application();
                    //        //Don't Show MS Excel
                    //        excelApp.Visible = false;
                    //        //Open a file at location
                    //        Excel.Workbook excelWorkBook = excelApp.Workbooks.Open(filePath);
                    //        //Convert that file to an XLSX document


                    //        excelWorkBook.SaveAs(filename, Excel.XlFileFormat.xlXMLSpreadsheet, Missing.Value, Missing.Value, false, false, Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlUserResolution, true, Missing.Value, Missing.Value, Missing.Value);
                    //        // resize cols
                    //        Excel.Range aRange = ((Excel.Worksheet)excelWorkBook.Worksheets[1]).Range["A1", "ZZ10000"];
                    //        aRange.Columns.AutoFit();

                    //        //Close excel
                    //        excelApp.Visible = true;

                    // Method 2 : NPOI

                    FileInfo file = new FileInfo(filename);
                    if (file.Exists) file.Delete();
                    ExcelPackage pack = new ExcelPackage(file);
                        pack = GenerateExcelFieldplan(CalculateFieldplan(), pack);
                        pack.Save();
                    pack.Dispose();


                }
                catch (Exception ex) { MessageBox.Show("Fehler: " + ex.Message); }
            }

            
        }
        public FieldPlanHelper CalculateFieldplan()
        {
            return CalculateFieldplan(BaseDoc, m_template_length, m_build_reverse);

        }
        public static FieldPlanHelper CalculateFieldplan(FieldDocument BaseDoc, int TemplateLength, bool? reverse = false)
        {
            int[,] dominoes = BaseDoc.dominoes;
            if (BaseDoc.horizontal == false)
            {
                // if direction = vertical, then translate the field
                dominoes = new int[dominoes.GetLength(1), dominoes.GetLength(0)];
                for (int i = 0; i < dominoes.GetLength(0); i++)
                {
                    for (int j = 0; j < dominoes.GetLength(1); j++)
                    {
                        dominoes[i, j] = BaseDoc.dominoes[j, dominoes.GetLength(0) - 1 - i];
                    }
                }
            }
            int[,] tempdominoes = dominoes;
            if (reverse == true)
            {
                // if reversed building direction
                dominoes = new int[dominoes.GetLength(0), dominoes.GetLength(1)];
                for (int i = 0; i < dominoes.GetLength(0); i++)
                {
                    for (int j = 0; j < dominoes.GetLength(1); j++)
                    {
                        dominoes[i, j] = tempdominoes[dominoes.GetLength(0) - i- 1, dominoes.GetLength(1) - j-1];
                    }
                }
            }
            int blocks = (int)Math.Ceiling((double)((double) dominoes.GetLength(0) / (double) TemplateLength));
            FieldPlanHelper b = new FieldPlanHelper();
            b.counts = new int[dominoes.GetLength(1), blocks][];
            b.colors = new DominoColor[dominoes.GetLength(1), blocks][];

            for (int i = 0; i < b.counts.GetLength(0); i++) // foreach line
            {
                for (int j = 0; j < b.counts.GetLength(1); j++) // foreach block in this line
                {
                    int initial_x = j * TemplateLength;
                    int pos_x = 0;
                    int currentcount = 0;
                    DominoColor currentColor = null;
                    List<DominoColor> currentColors = new List<DominoColor>();
                    List<int> currentCounts = new List<int>();
                    while (pos_x < TemplateLength && (initial_x + pos_x) < dominoes.GetLength(0))
                    {
                        
                        if (dominoes[initial_x + pos_x, i] != -1 && BaseDoc.Colors[dominoes[initial_x + pos_x, i]] == currentColor)
                        {
                            currentcount++;
                        }
                        else
                        {
                            if (currentColor != null)
                            {
                                currentColors.Add(currentColor);
                                currentCounts.Add(currentcount);
                            }

                            currentcount = 1;
                            currentColor = (dominoes[initial_x + pos_x, i] == -1) ? null : BaseDoc.Colors[dominoes[initial_x + pos_x, i]];
                        }
                        pos_x++;

                    }
                    currentColors.Add(currentColor);
                    currentCounts.Add(currentcount);
                    b.colors[i, j] = currentColors.ToArray();
                    b.counts[i, j] = currentCounts.ToArray();
                }
            }
            int numberofcolumns = 0;
            for (int i = 0; i < b.colors.GetLength(0); i++)
            {
                int tempnum = 0;
                // get the number of cells for each line
                for (int j = 0; j < b.colors.GetLength(1); j++)
                {
                    tempnum += b.colors[i, j].Length;
                }
                if (tempnum > numberofcolumns) numberofcolumns = tempnum;
            }
            b.cellcount = numberofcolumns;
            return b;
        }
        public String GenerateHTMLFieldPlan(FieldPlanHelper helper)
        {
            // html head
            StringBuilder html = new StringBuilder();
            html.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\">\n"
                + "<html>\n"
                + "    <head>\n"
                + "    <meta http-equiv=\"Content-Style-Type\" content=\"text/css\">   <title>" + (m_title + " Fieldplan")
                + "</title>\n    <meta http-equiv=\"Content-Type\" content=\"text/css\"; charset=UTF-8\">\n    </head>\n    <body>");
            html.AppendLine(m_text_format + "    <h3 align=\"left\">" + m_title + "</h3>");
            html.AppendLine("        <table border=0 cellspacing=3 cellpadding=5 style=\"border-collapse:"
                + ((m_collapse_borders == CollapsedMode.Collapsed) ? "collapse" : "separate") + "\">");
            if (helper.colors == null) return "Hmmm, irgendwas ist schiefgegangen...";
            for (int i = 0; i < helper.colors.GetLength(0); i++) // foreach row
            {
                html.AppendLine("            <tr>");
                html.AppendLine("                <td>" + ((BaseDoc.horizontal == true) ? "Row" : "Column") + "&nbsp;" + (i + 1) + "</td>");
                for (int j = 0; j < helper.colors.GetLength(1); j++) // foreach block
                {

                    for (int k = 0; k < helper.colors[i, j].Length; k++)
                    {
                        if (helper.colors[i, j][k] != null) // for all non-empty dominoes
                        {
                            System.Drawing.Color c = System.Drawing.Color.Black;
                            switch (m_back_color_mode)
                            {
                                case ColorMode.Normal: c = SMtoSD(helper.colors[i, j][k].rgb); break; // normal color
                                case ColorMode.Inverted: c = Invert(helper.colors[i, j][k].rgb); break; // inverted color
                                case ColorMode.Fixed: c = SMtoSD(m_fixed_back_color); break; // intelligent BW
                            }
                            html.Append("                <td bgcolor='");
                            html.Append(System.Drawing.ColorTranslator.ToHtml(c));
                            html.Append("'");
                            if (k == helper.colors[i, j].Length - 1) // if end of block, draw right border in intelligent black/white
                            {
                                html.Append(" style='width:115.15pt;border-right:solid " + System.Drawing.ColorTranslator.ToHtml(IntelligentBW(helper.colors[i, j][k].rgb)) + " 5.0pt'");
                            }
                            html.Append("> <font color='");
                            switch (m_fore_color_mode)
                            {
                                case ColorMode.Normal: html.Append(System.Drawing.ColorTranslator.ToHtml(SMtoSD(helper.colors[i, j][k].rgb))); break; // normal color
                                case ColorMode.Inverted: html.Append(System.Drawing.ColorTranslator.ToHtml(Invert(helper.colors[i, j][k].rgb))); break; // inverted color
                                case ColorMode.Intelligent: html.Append(System.Drawing.ColorTranslator.ToHtml(IntelligentBW(SDtoSM(c)))); break; // intelligent BW
                                case ColorMode.Fixed: html.Append(System.Drawing.ColorTranslator.ToHtml(SMtoSD(m_fixed_fore_color))); break;
                            }
                            html.Append("'>");
                            html.Append(ParseRegex(helper.colors[i, j][k], helper.counts[i, j][k]) + "</font></td>\n");

                        }
                    }
                }
                html.AppendLine("            </tr>");
            }
            html.AppendLine("        </table>");
            html.AppendLine("\n" + GetSmallDominoBalance());
            html.AppendLine("\n" + GetLargeDominoBalance());
            html.AppendLine("\n        </font>\n    </body>\n</html>");
            //Dateistream wird beendet und der temporaere Dateiname zurueckgegeben.
            return html.ToString();

        }
        private String GetSmallDominoBalance()
        {
            if (summary_selection == SummaryEnum.None) return "";
            string s = "<br>" + text_format;
            if (BaseDoc.horizontal == true)
            {
                s += "Rows: " + BaseDoc.dominoes.GetLength(1) + ", Columns: " + BaseDoc.dominoes.GetLength(0);
            }
            else
            {
                s += "Columns: " + BaseDoc.dominoes.GetLength(0) + ", Rows: " + BaseDoc.dominoes.GetLength(1);
            }
            s += "<br>Total Number of dominoes: " + BaseDoc.dominoes.GetLength(0) * BaseDoc.dominoes.GetLength(1) + "<br></font>";
            
            return s;
        }
        private String GetLargeDominoBalance()
        {
            if (summary_selection != SummaryEnum.Large)
            {
                return "";

            }
            StringBuilder s = new StringBuilder("<br>" + text_format);
            int[] counts = new int[BaseDoc.Colors.Count + 1];
            // count dominoes
            for (int i = 0; i < BaseDoc.dominoes.GetLength(0); i++)
            {
                for (int j = 0; j < BaseDoc.dominoes.GetLength(1); j++)
                {
                    counts[(BaseDoc.dominoes[i, j] > -1) ? BaseDoc.dominoes[i, j] : BaseDoc.Colors.Count]++;
                }
            }
            s.Append("<table border=\"0\" rules=\"groups\">\n");
            s.Append("  <thead>\n");
            s.Append("       <tr>\n");
            s.Append("           <th></th>\n");
            s.Append("           <th>" + "Name" + "</th>\n");
            s.Append("           <th>" + "Count" + "</th>\n");
            s.Append("           <th>" + "Used" + "</th>\n");
            s.Append("       </tr>\n");
            s.Append("  </thead>\n");
            for (int i = 0; i < counts.Length - 1; i++)
            {
                if (counts[i] != 0)
                {
                    s.Append("      <tr>\n");
                    s.Append("          <td bgcolor=\'" + System.Drawing.ColorTranslator.ToHtml(SMtoSD(BaseDoc.Colors[i].rgb)) + "\'>" + "&nbsp;&nbsp;" + "</td>\n");
                    s.Append("          <td> " + BaseDoc.Colors[i].name + "</td>\n");
                    s.Append("          <td> " + BaseDoc.Colors[i].count + "</td>\n");
                    s.Append("          <td> " + counts[i] + "</td>\n");
                    s.Append("      </tr>\n");
                }
            }
            s.Append("</table></font>");
            return s.ToString();
        }
        public ExcelPackage GenerateExcelFieldplan(FieldPlanHelper helper, ExcelPackage pack)
        {
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add(title);
            int cols = 0;
            int rowcounter = 1;

            for (int i = 0; i < helper.colors.GetLength(0); i++) // foreach row
            {
                int cellcounter = 1;
                ExcelRange cell1 = ws.Cells[++rowcounter, cellcounter++];
                cell1.Value = ((BaseDoc.horizontal == true) ? "Row" : "Column") + " " + (i + 1);
                cell1.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell1.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                for (int j = 0; j < helper.colors.GetLength(1); j++) // foreach block
                {
                    for (int k = 0; k < helper.colors[i, j].Length; k++)
                    {
                        if (helper.colors[i, j][k] != null) // for all non-empty dominoes
                        {
                            using (ExcelRange cell = ws.Cells[rowcounter, cellcounter++])
                            {
                                System.Drawing.Color b = System.Drawing.Color.Black;
                                switch (m_back_color_mode)
                                {
                                    case ColorMode.Normal: b = SMtoSD(helper.colors[i, j][k].rgb); break; // normal color
                                    case ColorMode.Inverted: b = Invert(helper.colors[i, j][k].rgb); break; // inverted color
                                    case ColorMode.Fixed: b = SMtoSD(m_fixed_back_color); break; // intelligent BW
                                }
                                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(b);

                                if (k == helper.colors[i, j].Length - 1) // if end of block, draw right border in intelligent black/white
                                {
                                    System.Drawing.Color border = IntelligentBW(helper.colors[i, j][k].rgb);
                                    cell.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                                    cell.Style.Border.Right.Color.SetColor(border);
                                }
                                System.Drawing.Color f = System.Drawing.Color.Black;
                                switch (m_fore_color_mode)
                                {
                                    case ColorMode.Normal: f = SMtoSD(helper.colors[i, j][k].rgb); break; // normal color
                                    case ColorMode.Inverted: f = Invert(helper.colors[i, j][k].rgb); break; // inverted color
                                    case ColorMode.Intelligent: f = IntelligentBW(SDtoSM(b)); break; // intelligent BW
                                    case ColorMode.Fixed: f = SMtoSD(m_fixed_fore_color); break;
                                }
                                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                cell.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                                cell.Style.Font.Color.SetColor(f);
                                cell.Style.Numberformat.Format = "@";
                                cell.Value = ParseRegex(helper.colors[i, j][k], helper.counts[i, j][k]);

                            }
                        }
                    }
                }
                if (cellcounter > cols) cols = cellcounter;
            }
            // those two assignments are needed for whatever reason (see http://stackoverflow.com/questions/38860557/strange-behavior-in-autofitcolumns-using-epplus)
            ws.Cells[1, 1].Value = "a";
            ws.Cells[1, 2].Value = "b";
            // apply font scheme to all cells
            ws.Cells[1, 1, rowcounter, cols].Style.Font.SetFromFont(StringToFont(text_format));
            // resize cells
            ws.Cells.AutoFitColumns(0);
            // add title
            ws.Cells[1, 2].Clear();
            ws.Cells[1, 1].Value = title;
            ws.Cells[1, 1].Style.Font.Size = 15;
            ws.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            ws.Row(1).Height = 27;
            // add small summary 
            if (summary_selection != SummaryEnum.None)
            {
                ExcelRange cell1 = ws.Cells[rowcounter + 2, 1];
                if (BaseDoc.horizontal == true)
                {
                    cell1.Value = "Rows: " + BaseDoc.dominoes.GetLength(1) + ", Columns: " + BaseDoc.dominoes.GetLength(0);
                }
                else
                {
                    cell1.Value = "Columns: " + BaseDoc.dominoes.GetLength(0) + ", Rows: " + BaseDoc.dominoes.GetLength(1);
                }
                ws.Cells[rowcounter + 3, 1].Value = "Total Number of dominoes: " + BaseDoc.dominoes.GetLength(0) * BaseDoc.dominoes.GetLength(1);
                ws.Cells[rowcounter + 2, 1, rowcounter + 3, 1].Style.Font.SetFromFont(StringToFont(text_format));
            }
            rowcounter += 4;
            // set footer
            ws.HeaderFooter.FirstFooter.CenteredText = String.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            ws.HeaderFooter.EvenFooter.CenteredText = String.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            ws.HeaderFooter.OddFooter.CenteredText = String.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            ws.HeaderFooter.OddHeader.RightAlignedText = DateTime.Today.ToShortDateString();
            ws.HeaderFooter.EvenHeader.RightAlignedText = DateTime.Today.ToShortDateString();
            ws.HeaderFooter.FirstHeader.LeftAlignedText = "Project: " + new DirectoryInfo(System.IO.Path.GetDirectoryName(BaseDoc.path)).Name; 
            ws.HeaderFooter.OddHeader.LeftAlignedText = title;
            ws.HeaderFooter.EvenHeader.LeftAlignedText = title;
            ws.HeaderFooter.FirstHeader.RightAlignedText = "Go to View -> Page Break Preview for page overview";
            ws.HeaderFooter.OddHeader.CenteredText = "Project: " + new DirectoryInfo(System.IO.Path.GetDirectoryName(BaseDoc.path)).Name;
            ws.HeaderFooter.EvenHeader.CenteredText = "Project: " + new DirectoryInfo(System.IO.Path.GetDirectoryName(BaseDoc.path)).Name;
            ws.PrinterSettings.TopMargin = (decimal)0.5;
            ws.PrinterSettings.LeftMargin = (decimal)0.4;
            ws.PrinterSettings.RightMargin = (decimal)0.4;
            ws.PrinterSettings.BottomMargin = (decimal)0.5;
            ws.PrinterSettings.HeaderMargin = (decimal)0.2;
            ws.PrinterSettings.FooterMargin = (decimal)0.2;
            if (summary_selection == SummaryEnum.Large)
            {
                // The list is first stored to a new workbook to get the minimum size of each column.
                ExcelWorksheet summary = pack.Workbook.Worksheets.Add("Summary");
                summary.Cells[1, 1].Value = " ";
                summary.Cells[1, 2].Value = "Color";
                summary.Cells[1, 3].Value = "Total";
                summary.Cells[1, 4].Value = "Used";
                int[] counts = new int[BaseDoc.Colors.Count + 1];
                // count dominoes
                for (int i = 0; i < BaseDoc.dominoes.GetLength(0); i++)
                {
                    for (int j = 0; j < BaseDoc.dominoes.GetLength(1); j++)
                    {
                        counts[(BaseDoc.dominoes[i, j] > -1) ? BaseDoc.dominoes[i, j] : BaseDoc.Colors.Count]++;
                    }
                }
                int non_empty_cols = 0;
                for (int i = 0; i < counts.Length - 1; i++)
                {
                    if (counts[i] != 0)
                    {
                        summary.Cells[i + 2, 2].Value = BaseDoc.Colors[i].name;
                        summary.Cells[i + 2, 3].Value = BaseDoc.Colors[i].count;
                        summary.Cells[i + 2, 4].Value = counts[i];
                        non_empty_cols++;
                    }
                }
                summary.Cells[1, 1, counts.Length, 4].Style.Font.SetFromFont(StringToFont(text_format));
                summary.Cells.AutoFitColumns(0);
                double[] widths = new double[] { summary.Column(1).Width, summary.Column(2).Width, summary.Column(3).Width, summary.Column(4).Width };
                // the sheet is not needed anymore
                pack.Workbook.Worksheets.Delete(summary);

                ws.Cells[rowcounter + 1, 1].Value = "Overview of used dominoes:";
                int[] indices = new int[3]; // fist item: end index of name column,...
                for (int i = 0; i < 3; i++)
                {
                    int endindex = (i == 0) ? 2 : indices[i - 1] + 1;
                    double width = ws.Column(endindex).Width;
                    while (width < widths[i+1])
                    {
                        endindex++;
                        width += ws.Column(endindex).Width;
                    }
                    indices[i] = endindex;
                }
                System.Drawing.Font textfont = StringToFont(text_format);
                ws.Cells[rowcounter + 1, 1].Style.Font.SetFromFont(textfont);
                rowcounter +=2;
                using (ExcelRange Color_Header = ws.Cells[rowcounter, 2, rowcounter, indices[0]])
                {
                    Color_Header.Merge = true;
                    Color_Header.Value = "Color";
                    Color_Header.Style.Font.SetFromFont(textfont);
                    SetAllBorders(Color_Header, ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                    Color_Header.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                }
                using (ExcelRange Count_Header = ws.Cells[rowcounter, indices[0] + 1, rowcounter, indices[1]])
                {
                    Count_Header.Merge = true;
                    Count_Header.Value = "Count";
                    Count_Header.Style.Font.SetFromFont(textfont);
                    SetAllBorders(Count_Header, ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                    Count_Header.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                }
                using (ExcelRange Used_Header = ws.Cells[rowcounter, indices[1] + 1, rowcounter, indices[2]])
                {
                    Used_Header.Merge = true;
                    Used_Header.Value = "Used";
                    Used_Header.Style.Font.SetFromFont(textfont);
                    SetAllBorders(Used_Header, ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                    Used_Header.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                }
                for (int i = 0; i < counts.Length - 1; i++)
                {
                    if (counts[i] > 0)
                    {
                        rowcounter++;
                        using (ExcelRange color_cell = ws.Cells[rowcounter, 1])
                        {
                            color_cell.Value = " ";
                            color_cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            color_cell.Style.Fill.BackgroundColor.SetColor(SMtoSD(BaseDoc.Colors[i].rgb));
                            SetAllBorders(color_cell, ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        }
                        using (ExcelRange name_cell = ws.Cells[rowcounter, 2, rowcounter, indices[0]])
                        {
                            name_cell.Merge = true;
                            name_cell.Value = BaseDoc.Colors[i].name;
                            SetAllBorders(name_cell, ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            name_cell.Style.Font.SetFromFont(textfont);
                        }
                        using (ExcelRange count_cell = ws.Cells[rowcounter, indices[0] + 1, rowcounter, indices[1]])
                        {
                            count_cell.Merge = true;
                            count_cell.Value = BaseDoc.Colors[i].count;
                            SetAllBorders(count_cell, ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            count_cell.Style.Font.SetFromFont(textfont);
                        }
                        using (ExcelRange used_cell = ws.Cells[rowcounter, indices[1] + 1, rowcounter, indices[2]])
                        {
                            used_cell.Merge = true;
                            used_cell.Value = counts[i];
                            SetAllBorders(used_cell, ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            used_cell.Style.Font.SetFromFont(textfont);
                        }
                    }
                }
                ws.Cells[rowcounter + 1, 1, rowcounter + non_empty_cols + 1, indices[2]].Style.Font.SetFromFont(StringToFont(text_format));

            }
            return pack;
        }
        private void SetAllBorders(ExcelRange range, ExcelBorderStyle style, System.Drawing.Color color)
        {
            range.Style.Border.Bottom.Style = style;
            range.Style.Border.Top.Style = style;
            range.Style.Border.Right.Style = style;
            range.Style.Border.Left.Style = style;
            range.Style.Border.Right.Color.SetColor(color);
            range.Style.Border.Left.Color.SetColor(color);
            range.Style.Border.Top.Color.SetColor(color);
            range.Style.Border.Bottom.Color.SetColor(color);
        }
        private System.Drawing.Font StringToFont(String format)
        {
            String family = "Calibri";
            int face = format.IndexOf("face=\"");
            if (face > 0)
            {
                int b = format.IndexOf('\"', face + 6);
                family = format.Substring(face +  6, b - face - 6);
            }
            System.Drawing.Font f = new System.Drawing.Font(family, 12);
            return f;
        }
        // old version of drawing, replaced by html
        // possibly needed for xps export
        public void DrawFieldPlan(FieldPlanHelper helper)
        {
            FlowDocument d = new FlowDocument();
            d.PageWidth = 5000;
            Table table1 = new Table();
            d.Blocks.Add(new Paragraph(new Run(m_title)) { FontSize = 20 });
            d.Blocks.Add(table1);
            
            table1.CellSpacing = 5;
            table1.Background = Brushes.White;
            
            for (int i = 0; i < helper.cellcount; i++)
            {
                table1.Columns.Add(new TableColumn() { Width = new GridLength(100) });
            }
            table1.RowGroups.Add(new TableRowGroup());
            for (int i = 0; i < 10; i++)
            {
                TableRow tr = new TableRow();
                tr.Cells.Add(new TableCell(new Paragraph(new Run(((BaseDoc.horizontal == true) ? "Row" : "Column") + " " + (i + 1)))));
                for (int j = 0; j < helper.colors.GetLength(1); j++) // foreach block
                {
                    for (int k = 0; k < helper.colors[i, j].Length; k++)
                    {
                        TableCell td = new TableCell();
                        switch (m_back_color_mode)
                        {
                            case ColorMode.Normal: td.Background = new SolidColorBrush(helper.colors[i, j][k].rgb); break; // normal color
                            case ColorMode.Inverted: td.Background = new SolidColorBrush(SDtoSM(Invert(helper.colors[i, j][k].rgb))); break; // inverted color
                            case ColorMode.Intelligent: td.Background = new SolidColorBrush(SDtoSM(IntelligentBW(helper.colors[i, j][k].rgb))); break; // intelligent BW
                        }
                        
                        if (k == helper.colors[i, j].Length - 1) // if end of block, draw right border in intelligent black/white
                        {
                            td.BorderThickness = new Thickness(0, 0, 5, 0);
                            td.BorderBrush = Brushes.Black; 
                        }
                        Paragraph p = new Paragraph();
                        Run r = new Run();
                        switch (m_fore_color_mode)
                        {
                            case ColorMode.Normal: r.Foreground = new SolidColorBrush(helper.colors[i, j][k].rgb); break; // normal color
                            case ColorMode.Inverted: r.Foreground = new SolidColorBrush(SDtoSM(Invert(helper.colors[i, j][k].rgb))); break; // inverted color
                            case ColorMode.Intelligent: r.Foreground = new SolidColorBrush(SDtoSM(IntelligentBW(helper.colors[i, j][k].rgb))); break; // intelligent BW
                        }
                        r.Text = ParseRegex(helper.colors[i, j][k], helper.counts[i, j][k]);

                        p.Inlines.Add(r);
                        td.Blocks.Add(p);
                        tr.Cells.Add(td);
                    }
                }
                table1.RowGroups[0].Rows.Add(tr);
            }
            if (helper.colors.GetLength(0) > 10)
                d.Blocks.Add(new Paragraph(new Run("+ additional " + (helper.colors.GetLength(0) - 10) + "rows")));
            autoresizeColumns(table1);
            doc = d;


        }
        void autoresizeColumns(Table table)
        {
            TableColumnCollection columns = table.Columns;
            TableRowCollection rows = table.RowGroups[0].Rows;
            TableCellCollection cells;
            TableRow row;
            TableCell cell;

            int columnCount = columns.Count;
            int rowCount = rows.Count;
            int cellCount = 0;

            double[] columnWidths = new double[columnCount];
            double columnWidth;

            // loop through all rows
            for (int r = 0; r < rowCount; r++)
            {
                row = rows[r];
                cells = row.Cells;
                cellCount = cells.Count;

                // loop through all cells in the row    
                for (int c = 0; c < columnCount && c < cellCount; c++)
                {
                    cell = cells[c];
                    columnWidth = getDesiredWidth(new TextRange(cell.ContentStart, cell.ContentEnd)) + 19;

                    if (columnWidth > columnWidths[c])
                    {
                        columnWidths[c] = columnWidth;
                    }
                }
            }

            // set the columns width to the widest cell
            for (int c = 0; c < columnCount; c++)
            {
                columns[c].Width = new GridLength(columnWidths[c]);
            }
        }


        double getDesiredWidth(TextRange textRange)
        {
            return new FormattedText(
                textRange.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    textRange.GetPropertyValue(TextElement.FontFamilyProperty) as System.Windows.Media.FontFamily,
                    (FontStyle)textRange.GetPropertyValue(TextElement.FontStyleProperty),
                    (FontWeight)textRange.GetPropertyValue(TextElement.FontWeightProperty),
                    FontStretches.Normal),
                    (double)textRange.GetPropertyValue(TextElement.FontSizeProperty),
                Brushes.Black,
                null,
                TextFormattingMode.Display).Width;
        }
        public static System.Drawing.Color SMtoSD(System.Windows.Media.Color c)
        {
            return System.Drawing.Color.FromArgb(c.R, c.G, c.B);
        }
        private System.Drawing.Color Invert(System.Windows.Media.Color c)
        {
            return System.Drawing.Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B);
        }
        public static Color SDtoSM(System.Drawing.Color c)
        {
            return Color.FromArgb(255, c.R, c.G, c.B);
        }
        private System.Drawing.Color IntelligentBW(Color c)
        {
            if (c.R * .3d + c.G * .59 + c.B * 0.11 > 128)
            {
                return System.Drawing.Color.Black;
            }
            return System.Drawing.Color.White;
        }
        private String ParseRegex(DominoColor dc, int count)
        { 
             string s = "";
            for (int i = 0; i < m_text_regex.Length; i++)
            {
                if (m_text_regex[i] == '%')
                {
                    string substring= "";
                    try
                    {
                        substring = m_text_regex.Substring(i + 1, m_text_regex.IndexOf('%', i + 1) - i - 1);
                    }
                    catch
                    {

                    }
                    if (substring.ToLower() == "color")
                    {
                        s += dc.name;
                    }
                    else if (substring.ToLower() == "count")
                    {
                        s += count;
                    }
                    i = (m_text_regex.IndexOf('%', i + 1) == -1) ? i : m_text_regex.IndexOf('%', i + 1);
                }
                else
                {
                    s += m_text_regex[i];
                }
            }
            return s;
        }
        

    }
    public struct FieldPlanHelper
    {
        public int[,][] counts;
        public DominoColor[,][] colors;
        public int cellcount;
    }
    public class SummaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
    [Serializable]
    public enum SummaryEnum
    {
        None, Small, Large
    }
    public enum ColorMode
    {
        Normal, Intelligent, Inverted, Fixed
    }
    public enum CollapsedMode
    {
        Collapsed, Separate
    }

}
