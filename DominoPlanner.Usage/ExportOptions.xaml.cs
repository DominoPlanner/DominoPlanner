using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DominoPlanner.Core;

namespace DominoPlanner.Usage
{
    public class ExportOptions : Window
    {
        public ExportOptions()
        {
            this.InitializeComponent();
            var dc = new ExportOptionVM();
            DataContext = dc;
            dc.PropertyChanged += ExpVM_PropertyChanged;
#if DEBUG
            //this.AttachDevTools();
#endif
        }
        public ExportOptions(IDominoProvider provider)
        {
            InitializeComponent();
            var dc = new ProjectExportOptionsVM(provider);
            DataContext = dc;
            dc.PropertyChanged += ExpVM_PropertyChanged;
        }

        private void ExpVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Close"))
            {
                this.Close(((ExportOptionVM)DataContext).result);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
