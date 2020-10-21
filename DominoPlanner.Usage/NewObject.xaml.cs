using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.Usage
{
    public class NewObject : Window
    {
        public NewObject()
        {
            this.InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }
        public NewObject(NewObjectVM novm)
        {
            InitializeComponent();
            DataContext = novm;
            ((NewObjectVM)DataContext).CloseChanged += NewObject_CloseChanged;
        }

        private void NewObject_CloseChanged(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
