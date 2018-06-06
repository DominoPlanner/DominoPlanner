using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DominoPlanner
{
    /// <summary>
    /// Interaktionslogik für FieldProtocolDetails.xaml
    /// </summary>
    public partial class FieldProtocolDetails : Window
    {
        public FieldProtocolDetails()
        {
            InitializeComponent();
        }

        private void SaveSettingsGlobal(object sender, RoutedEventArgs e)
        {
            FieldPlanDocument fpd = ((FieldPlanDocument)this.DataContext);
            Properties.Settings.Default.FieldPlanHideText = fpd.hide_text;
            Properties.Settings.Default.FieldPlanForeColorMode = fpd.fore_color_mode.ToString();
            Properties.Settings.Default.FieldPlanBackColorMode = fpd.back_color_mode.ToString();
            Properties.Settings.Default.FieldPlanFixedBackColor = FieldPlanDocument.SMtoSD(fpd.fixed_back_color);
            Properties.Settings.Default.FieldPlanFixedForeColor = FieldPlanDocument.SMtoSD(fpd.fixed_fore_color);
            Properties.Settings.Default.FieldPlanFormatString = fpd.text_format;
            Properties.Settings.Default.FieldPlanPropertyMode = fpd.summary_selection.ToString();
            Properties.Settings.Default.FieldPlanRegex = fpd.text_regex;
            Properties.Settings.Default.TemplateLength = fpd.template_length;
            Properties.Settings.Default.Save();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ChangeColor(object sender, RoutedEventArgs e)
        {
            ColorControl c = new ColorControl();
            c.ColorOnly = true;
            if (((Button) sender).Name.ToString() == "BackColor")
            {
                c.ColorPicker.SelectedColor = ((FieldPlanDocument)this.DataContext).fixed_back_color;
            }
            else
            {
                c.ColorPicker.SelectedColor = ((FieldPlanDocument)this.DataContext).fixed_fore_color;
            }
            c.ShowDialog();
            if (((Button)sender).Name.ToString() == "BackColor")
            {
                 ((FieldPlanDocument)this.DataContext).fixed_back_color= c.ColorPicker.SelectedColor;
            }
            else
            {
                 ((FieldPlanDocument)this.DataContext).fixed_fore_color = c.ColorPicker.SelectedColor;
            }
        }
    }
}
