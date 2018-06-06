using DominoPlanner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for ChangeProjectSize.xaml
    /// </summary>
    public partial class ChangeProjectSize : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _iCounter;
        public int iCounter
        {
            get { return _iCounter; }
            set
            {
                if (_iCounter != value) { _iCounter = value; RaisePropertyChanged(); }
            }
        }


        public bool doIt { get; set; }
        public ResizeMode ResizePlace { get; set; }

        public ChangeProjectSize()
        {
            this.DataContext = this;
            ResizePlace = DominoPlanner.ResizeMode.none;
            InitializeComponent();
            nudSizeChanger.Value = 1;
            nudSizeChanger.MinValue = 1;
        }

        private void TopClick(object sender, RoutedEventArgs e)
        {
            ResizePlace = DominoPlanner.ResizeMode.Top;
            CloseIt();
        }

        private void LeftClick(object sender, RoutedEventArgs e)
        {
            ResizePlace = DominoPlanner.ResizeMode.Left;
            CloseIt();
        }

        private void RightClick(object sender, RoutedEventArgs e)
        {
            ResizePlace = DominoPlanner.ResizeMode.Right;
            CloseIt();
        }

        private void BottomClick(object sender, RoutedEventArgs e)
        {
            ResizePlace = DominoPlanner.ResizeMode.Bottom;
            CloseIt();
        }

        private void CloseIt()
        {
            MessageBoxResult dialogResult = MessageBoxResult.No;
            if (ResizePlace == DominoPlanner.ResizeMode.Bottom)
            {
                dialogResult = MessageBox.Show(this, String.Format("Add {0} Rows below?", iCounter), "Add Bottom?", MessageBoxButton.YesNo);
            }
            else if (ResizePlace == DominoPlanner.ResizeMode.Top)
            {
                dialogResult = MessageBox.Show(this, String.Format("Add {0} Rows above?", iCounter), "Add Top?", MessageBoxButton.YesNo);
            }
            else if (ResizePlace == DominoPlanner.ResizeMode.Left)
            {
                dialogResult = MessageBox.Show(this, String.Format("Add {0} Column at the left side?", iCounter), "Add left?", MessageBoxButton.YesNo);
            }
            else if (ResizePlace == DominoPlanner.ResizeMode.Right)
            {
                dialogResult = MessageBox.Show(this, String.Format("Add {0} Column at the right side?", iCounter), "Add right?", MessageBoxButton.YesNo);
            }

            if (dialogResult == MessageBoxResult.Yes)
            {
                this.Close();
            }else
            {
                ResizePlace = DominoPlanner.ResizeMode.none;
            }
        }

        private void nudSizeChanger_ValueChanged(object sender, RoutedEventArgs e)
        {
            iCounter = (int)((NumericUpDown)sender).Value;
        }
    }

    public enum ResizeMode
    {
        Top, Bottom, Left, Right, none
    }
}
