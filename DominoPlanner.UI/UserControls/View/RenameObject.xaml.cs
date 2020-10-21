using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.UI.UserControls
{
    public class RenameObject : Window
    {
        public RenameObject()
        {
            this.InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
        public RenameObject(string filename)
        {
            InitializeComponent();
            var dc = new RenameObjectVM(filename);
            DataContext = dc;
            dc.PropertyChanged += Npvm_PropertyChanged;
        }

        private void Npvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Close"))
            {
                var result = ((RenameObjectVM)DataContext).result;
                Close(result);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
