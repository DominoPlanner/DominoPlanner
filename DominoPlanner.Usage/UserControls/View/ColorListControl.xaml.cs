using DominoPlanner.Usage.UserControls.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DominoPlanner.Usage.UserControls.View
{
    /// <summary>
    /// Interaction logic for ColorListControl.xaml
    /// </summary>
    public partial class ColorListControl : UserControl
    {
        public ColorListControl()
        {
            InitializeComponent();
            DataContextChanged += ColorListControl_DataContextChanged;
        }

        private void ColorListControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ColorListControlVM vm)
            {
                vm.ShowProjectsChanged += Vm_ShowProjectsChanged;
                RefreshSumVisibility();
            }
        }

        private void Vm_ShowProjectsChanged(object sender, EventArgs e)
        {
            RefreshSumVisibility();
        }

        public ColorListControl(ColorListControlVM clcvm) : this()
        {
            DataContext = clcvm;
            RefreshSumVisibility();
        }

        private void RefreshSumVisibility()
        {
            if (DataContext is ColorListControlVM vm)
            {
                sumtemplate.Visibility = vm.ShowProjects ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void Color_Delete(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Delete!");
        }
        
        public static readonly DependencyProperty BindableColumnsProperty = DependencyProperty.RegisterAttached("BindableColumns", typeof(ObservableCollection<DataGridColumn>), typeof(ColorListControl), new UIPropertyMetadata(null, BindableColumnsPropertyChanged));

        private static void BindableColumnsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            DataGrid dataGrid = source as DataGrid;
            ObservableCollection<DataGridColumn> columns = e.NewValue as ObservableCollection<DataGridColumn>;
            if (columns == null) return;
            foreach (DataGridColumn column in columns)
            {
                try
                {
                    dataGrid.Columns.Add(column);
                }
                catch (ArgumentException ex)
                {

                }
            }

            columns.CollectionChanged += (sender, e2) =>
            {
                NotifyCollectionChangedEventArgs ne = e2 as NotifyCollectionChangedEventArgs;
                switch (ne.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (ne.NewItems != null)
                        {
                            foreach (DataGridColumn column in ne.NewItems)
                            {
                                dataGrid.Columns.Add(column);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (ne.OldItems != null)
                        {
                            foreach (DataGridColumn column in ne.OldItems)
                            {
                                dataGrid.Columns.Remove(column);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        while (dataGrid.Columns.Count > 3)
                        {
                            dataGrid.Columns.RemoveAt(3);
                        }
                        break;
                    default:
                        break;
                }
            };
        }


        public static void SetBindableColumns(DependencyObject element, ObservableCollection<DataGridColumn> value)
        {
            element.SetValue(BindableColumnsProperty, value);
        }

        public ObservableCollection<DataGridColumn> GetBindableColumns(DependencyObject element)
        {
            return (ObservableCollection<DataGridColumn>)element.GetValue(BindableColumnsProperty);
        }
    }
}
