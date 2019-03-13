using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DominoPlanner.Core
{
    public class ObjectProtocolParameters
    {
        public string title {
            get; set; }
        public int templateLength { get; set; }
        public SummaryMode summaryMode { get; set; }
        public string textFormat { get; set; }
        public string textRegex { get; set; }
        public ColorMode foreColorMode { get; set; }
        public ColorMode backColorMode { get; set; }
        public bool reverse { get; set; }
        public Color fixedBackColor { get; set; }
        public Orientation orientation { get; set; }
        public String project { get; set; }
        public ObjectProtocolParameters()
        {
            reverse = false;
            fixedBackColor = Colors.White;
            orientation = Orientation.Horizontal;
        }
        internal string GetHTMLProcotol(ProtocolTransfer obj)
        {
            // html head

            StringBuilder html = new StringBuilder();
            html.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\">\n"
                + "<html>\n"
                + "    <head>\n"
                + "    <meta http-equiv=\"Content-Style-Type\" content=\"text/css\">   <title>" + (title + " Procotol")
                + "</title>\n    <meta http-equiv=\"Content-Type\" content=\"text/css\"; charset=\"UTF-8\">\n    </head>\n    <body>");
            html.AppendLine(textFormat + "    <h3 align=\"left\">" + title + "</h3>");
            html.AppendLine("        <table border=0 cellspacing=3 cellpadding=5 style=\"border-collapse:collapse\">");
            if (obj.dominoes == null) return "Dominoes are empty!";
            for (int i = 0; i < obj.dominoes.Length; i++) // foreach row
            {
                html.AppendLine("            <tr>");
                html.AppendLine("                <td>" + ((orientation == Orientation.Horizontal) ? "Row" : "Column") + "&nbsp;" + (i + 1) + "</td>");
                for (int j = 0; j < obj.dominoes[i].Count; j++) // foreach block
                {
                    for (int k = 0; k < obj.dominoes[i][j].Count; k++)
                    {
                        if (obj.dominoes[i][j][k] != null && obj.dominoes[i][j][k].Item1 >= 0) // for all non-empty dominoes
                        {
                            Color c = Colors.White;
                            switch (backColorMode)
                            {
                                case ColorMode.Normal: c = obj.colors[obj.dominoes[i][j][k].Item1].mediaColor; break; // normal color
                                case ColorMode.Inverted: c = obj.colors[obj.dominoes[i][j][k].Item1].mediaColor.Invert(); break; // inverted color
                                case ColorMode.Fixed: c = fixedBackColor; break; // intelligent BW
                            }
                            html.Append("                <td bgcolor='");
                            html.Append(c.ToHTML());
                            html.Append("'");
                            if (k == obj.dominoes[i][j].Count - 1) // if end of block, draw right border in intelligent black/white
                            {
                                html.Append(" style='width:115.15pt;border-right:solid " + obj.colors[obj.dominoes[i][j][k].Item1].mediaColor.IntelligentBW().ToHTML() + " 5.0pt'");
                            }
                            html.Append("> <font color='");

                            switch (foreColorMode)
                            {
                                case ColorMode.Normal: html.Append(obj.colors[obj.dominoes[i][j][k].Item1].mediaColor.ToHTML()); break; // normal color
                                case ColorMode.Inverted: html.Append(obj.colors[obj.dominoes[i][j][k].Item1].mediaColor.Invert().ToHTML()); break; // inverted color
                                case ColorMode.Intelligent: html.Append(c.IntelligentBW().ToHTML()); break; // intelligent BW
                            }
                            html.Append("'>");
                            html.Append(ParseRegex(obj.colors[obj.dominoes[i][j][k].Item1].name, obj.dominoes[i][j][k].Item2) + "</font></td>\n");

                        }
                    }
                }
                html.AppendLine("            </tr>");
            }
            html.AppendLine("        </table>");
            html.AppendLine("\n" + GetSmallDominoBalanceHTML(obj));
            html.AppendLine("\n" + GetLargeDominoBalanceHTML(obj));
            html.AppendLine("\n        </font>\n    </body>\n</html>");
            return html.ToString();

        }
        private List<ColorSortHelper> OrderedColorBalance(ProtocolTransfer obj)
        {
            var list = new List<ColorSortHelper>();
            int counter = 0;
            for (int i = 0; i < obj.counts.Length; i++)
            {
                int anzeigeindex = -1;
                if (obj.colors[i] is DominoColor)
                {
                    anzeigeindex = obj.colors.Anzeigeindizes[counter];
                    counter++;
                }
                list.Add(new ColorSortHelper() { color = obj.colors[i], count = obj.counts[i], index = anzeigeindex});
            }
            return list.OrderBy(x => x.index).ToList();
        }
        private String GetSmallDominoBalanceHTML(ProtocolTransfer obj)
        {
            if (summaryMode == SummaryMode.None) return "";
            string s = "<br>" + textFormat;
            s += "Rows: " + obj.rows + ", Columns: " + obj.columns;
            s += "<br>Total Number of dominoes: " + obj.counts.Sum();
            return s;
        }
        private string GetLargeDominoBalanceHTML(ProtocolTransfer obj)
        {
            var orderedList = OrderedColorBalance(obj);
            if (summaryMode != SummaryMode.Large)
            {
                return "";

            }
            StringBuilder s = new StringBuilder("<br>" + textFormat);
            s.Append("<table border=\"0\" rules=\"groups\">\n");
            s.Append("  <thead>\n");
            s.Append("       <tr>\n");
            s.Append("           <th></th>\n");
            s.Append("           <th>" + "Name" + "</th>\n");
            s.Append("           <th>" + "Count" + "</th>\n");
            s.Append("           <th>" + "Used" + "</th>\n");
            s.Append("       </tr>\n");
            s.Append("  </thead>\n");
            for (int i = 0; i < orderedList.Count; i++)
            {
                if (orderedList[i].count != 0)
                {
                    s.Append("      <tr>\n");
                    s.Append("          <td bgcolor=\'" + orderedList[i].color.mediaColor.ToHTML() + "\'>" + "&nbsp;&nbsp;" + "</td>\n");
                    s.Append("          <td> " + orderedList[i].color.name + "</td>\n");
                    s.Append("          <td> " + (orderedList[i].color is DominoColor ? orderedList[i].color.count.ToString() : "&nbsp;")+ "</td>\n");
                    s.Append("          <td> " + orderedList[i].count + "</td>\n");
                    s.Append("      </tr>\n");
                }
            }
            s.Append("</table></font>");
            return s.ToString();
        }
        private string ParseRegex(string name, int count)
        {
            string s = "";
            for (int i = 0; i < textRegex.Length; i++)
            {
                if (textRegex[i] == '%')
                {
                    string substring = "";
                    try
                    {
                        substring = textRegex.Substring(i + 1, textRegex.IndexOf('%', i + 1) - i - 1);
                    }
                    catch
                    {

                    }
                    if (substring.ToLower() == "color")
                    {
                        s += name;
                    }
                    else if (substring.ToLower() == "count")
                    {
                        s += count;
                    }
                    i = (textRegex.IndexOf('%', i + 1) == -1) ? i : textRegex.IndexOf('%', i + 1);
                }
                else
                {
                    s += textRegex[i];
                }
            }
            return s;
        }
        internal ExcelPackage GenerateExcelFieldplan(ProtocolTransfer trans, ExcelPackage pack)
        {
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add(title);
            int cols = 0;
            int rowcounter = 1;
            ws.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            for (int i = 0; i < trans.dominoes.Length; i++) // foreach row
            {
                if (trans.dominoes[i] == null) throw new InvalidOperationException("Object not valid!");
                int cellcounter = 1;
                ExcelRange cell1 = ws.Cells[++rowcounter, cellcounter++];
                cell1.Value = ((trans.orientation == Orientation.Horizontal) ? "Row" : "Column") + " " + (i + 1);
                cell1.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell1.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                for (int j = 0; j < trans.dominoes[i].Count; j++) // foreach block
                {
                    if (trans.dominoes[i][j] == null) throw new InvalidOperationException("Field not valid!");
                    for (int k = 0; k < trans.dominoes[i][j].Count; k++)
                    {
                        if (trans.dominoes[i][j][k] != null && trans.dominoes[i][j][k].Item1 >= 0) // for all non-empty dominoes
                        {
                            using (ExcelRange cell = ws.Cells[rowcounter, cellcounter++])
                            {
                                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                Color b = Colors.Black;
                                switch (backColorMode)
                                {
                                    case ColorMode.Normal: b = trans.colors[trans.dominoes[i][j][k].Item1].mediaColor; break; // normal color
                                    case ColorMode.Inverted: b = trans.colors[trans.dominoes[i][j][k].Item1].mediaColor.Invert(); break; // inverted color
                                    case ColorMode.Fixed: b = fixedBackColor; break; // fixed
                                }
                                if ((trans.colors[trans.dominoes[i][j][k].Item1] is EmptyDomino))
                                {
                                    cell.Style.Fill.PatternType = ExcelFillStyle.DarkUp;
                                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                                    cell.Style.Fill.PatternColor.SetColor(System.Drawing.Color.Black);
                                }
                                else cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                var background = b.ToSD(true);
                                cell.Style.Fill.BackgroundColor.SetColor(background);

                                if (k == trans.dominoes[i][j].Count - 1) // if end of block, draw right border in intelligent black/white
                                {
                                    System.Drawing.Color border = trans.colors[trans.dominoes[i][j][k].Item1].mediaColor.IntelligentBW().ToSD();
                                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thick;
                                    cell.Style.Border.Right.Color.SetColor(border);
                                }
                                Color f = Colors.Black;
                                switch (foreColorMode)
                                {
                                    case ColorMode.Normal: f = trans.colors[trans.dominoes[i][j][k].Item1].mediaColor; break; // normal color
                                    case ColorMode.Inverted: f = trans.colors[trans.dominoes[i][j][k].Item1].mediaColor.Invert(); break; // inverted color
                                    case ColorMode.Intelligent: f = b.IntelligentBW(); break; // intelligent BW
                                }
                                cell.Style.Font.Color.SetColor(f.ToSD());
                                

                                string parsed = ParseRegex(trans.colors[trans.dominoes[i][j][k].Item1].name, trans.dominoes[i][j][k].Item2);
                                // Bugfix für Warnung "Zahl als Text" in Excel
                                if (parsed.Trim().All(char.IsDigit))
                                {
                                    cell.Value = Int32.Parse(parsed);
                                }
                                else
                                {
                                    cell.Value = parsed;
                                }
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
            ws.Cells[1, 1, rowcounter, cols].Style.Font.SetFromFont(StringToFont(textFormat));
            // resize cells
            ws.Cells.AutoFitColumns(0);
            // add title
            ws.Cells[1, 2].Clear();
            ws.Cells[1, 1].Value = title;
            ws.Cells[1, 1].Style.Font.Size = 15;
            ws.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            ws.Row(1).Height = 27;
            // add small summary 
            if (summaryMode != SummaryMode.None)
            {
                ExcelRange cell1 = ws.Cells[rowcounter + 2, 1];
                cell1.Value = "Rows: " + trans.rows + ", Columns: " + trans.columns;
                ws.Cells[rowcounter + 3, 1].Value = "Total Number of dominoes: " + trans.counts.Sum();
                ws.Cells[rowcounter + 2, 1, rowcounter + 3, 1].Style.Font.SetFromFont(StringToFont(textFormat));
            }
            rowcounter += 4;
            // set footer
            ws.HeaderFooter.FirstFooter.CenteredText = string.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            ws.HeaderFooter.EvenFooter.CenteredText = string.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            ws.HeaderFooter.OddFooter.CenteredText = string.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            ws.HeaderFooter.OddHeader.RightAlignedText = DateTime.Today.ToShortDateString();
            ws.HeaderFooter.EvenHeader.RightAlignedText = DateTime.Today.ToShortDateString();
            ws.HeaderFooter.FirstHeader.LeftAlignedText = "Project: " + project;
            ws.HeaderFooter.OddHeader.LeftAlignedText = title;
            ws.HeaderFooter.EvenHeader.LeftAlignedText = title;
            ws.HeaderFooter.FirstHeader.RightAlignedText = "Go to View -> Page Break Preview for page overview";
            ws.HeaderFooter.OddHeader.CenteredText = "Project: " + project;
            ws.HeaderFooter.EvenHeader.CenteredText = "Project: " + project;
            ws.PrinterSettings.TopMargin = (decimal)0.5;
            ws.PrinterSettings.LeftMargin = (decimal)0.4;
            ws.PrinterSettings.RightMargin = (decimal)0.4;
            ws.PrinterSettings.BottomMargin = (decimal)0.5;
            ws.PrinterSettings.HeaderMargin = (decimal)0.2;
            ws.PrinterSettings.FooterMargin = (decimal)0.2;
            if (summaryMode == SummaryMode.Large)
            {
                var orderedList = OrderedColorBalance(trans);
                // The list is first stored to a new workbook to get the minimum size of each column.
                ExcelWorksheet summary = pack.Workbook.Worksheets.Add("Summary");
                summary.Cells[1, 1].Value = " ";
                summary.Cells[1, 2].Value = "Color";
                summary.Cells[1, 3].Value = "Total";
                summary.Cells[1, 4].Value = "Used";
                int non_empty_cols = 0;
                for (int i = 0; i < orderedList.Count; i++)
                {
                    if (orderedList[i].count != 0)
                    {
                        summary.Cells[i + 2, 2].Value = orderedList[i].color.name;
                        if (orderedList[i].color is DominoColor)
                            summary.Cells[i + 2, 3].Value = orderedList[i].color.count;
                        summary.Cells[i + 2, 4].Value = orderedList[i].count;
                        non_empty_cols++;
                    }
                }
                summary.Cells[1, 1, trans.counts.Length, 4].Style.Font.SetFromFont(StringToFont(textFormat));
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
                    while (width < widths[i + 1])
                    {
                        endindex++;
                        width += ws.Column(endindex).Width;
                    }
                    indices[i] = endindex;
                }
                System.Drawing.Font textfont = StringToFont(textFormat);
                ws.Cells[rowcounter + 1, 1].Style.Font.SetFromFont(textfont);
                rowcounter += 2;
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
                for (int i = 0; i < orderedList.Count; i++)
                {
                    if (orderedList[i].count != 0)
                    { 
                        rowcounter++;
                        using (ExcelRange color_cell = ws.Cells[rowcounter, 1])
                        {
                            color_cell.Value = " ";
                            color_cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            color_cell.Style.Fill.BackgroundColor.SetColor(orderedList[i].color.mediaColor.ToSD());
                            SetAllBorders(color_cell, ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        }
                        using (ExcelRange name_cell = ws.Cells[rowcounter, 2, rowcounter, indices[0]])
                        {
                            name_cell.Merge = true;
                            name_cell.Value = orderedList[i].color.name;
                            SetAllBorders(name_cell, ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            name_cell.Style.Font.SetFromFont(textfont);
                        }
                        using (ExcelRange count_cell = ws.Cells[rowcounter, indices[0] + 1, rowcounter, indices[1]])
                        {
                            count_cell.Merge = true;
                            if (orderedList[i].color is DominoColor)
                            {
                                count_cell.Value = orderedList[i].color.count;
                            }
                            SetAllBorders(count_cell, ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            count_cell.Style.Font.SetFromFont(textfont);
                        }
                        using (ExcelRange used_cell = ws.Cells[rowcounter, indices[1] + 1, rowcounter, indices[2]])
                        {
                            used_cell.Merge = true;
                            used_cell.Value = orderedList[i].count;
                            SetAllBorders(used_cell, ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            used_cell.Style.Font.SetFromFont(textfont);
                        }
                    }
                }
                ws.Cells[rowcounter + 1, 1, rowcounter + non_empty_cols + 1, indices[2]].Style.Font.SetFromFont(StringToFont(textFormat));

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
                family = format.Substring(face + 6, b - face - 6);
            }
            System.Drawing.Font f = new System.Drawing.Font(family, 12);
            return f;
        }

    }
    public struct ColorSortHelper
    {
        public int count { get; set; }
        public IDominoColor color { get; set; }
        public int index { get; set; }
    }
}