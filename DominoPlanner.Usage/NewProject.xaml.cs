using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DominoPlanner.Usage
{
    public class NewProject : Window
    {

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public NewProject()
        {
            InitializeComponent();
        }

        public NewProject(NewProjectVM npvm)
        {
            DataContext = npvm;
            InitializeComponent();
            npvm.PropertyChanged += Npvm_PropertyChanged;
        }

        private void Npvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Close"))
                this.Close();
        }
    }
}
