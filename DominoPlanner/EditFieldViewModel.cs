using DominoPlanner.Document_Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DominoPlanner
{
    class EditFieldViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public EditFieldViewModel(List<DominoColor> Colors)
        {
            this.Colors = Colors;
            MessageBox.Show(Colors[0].name.ToString());
            //SaveAndEncrypt = new RelayCommand(o => DoSaveAndEncrypt());
        }

        #region Commands
        private ICommand _SaveAndEncrypt;
        public ICommand SaveAndEncrypt { get { return _SaveAndEncrypt; } set { if (value != _SaveAndEncrypt) { MessageBox.Show("Hier kommt die Methode hin!"); } } }

        #endregion

        #region Properties
        private string _name;
        public string Name { get { return _name; } set { if (value != _name) { _name = value; RaisePropertyChanged(); } } }

        private List<DominoColor> _colors;
        public List<DominoColor> Colors { get { return _colors; } set { if(value != _colors) { _colors = value; RaisePropertyChanged(); } } }
        #endregion
    }


    public class RelayCommand : ICommand
    {
        #region Properties

        private readonly Action<object> ExecuteAction;
        private readonly Predicate<object> CanExecuteAction;

        #endregion

        public RelayCommand(Action<object> execute)
          : this(execute, _ => true)
        {
        }
        public RelayCommand(Action<object> action, Predicate<object> canExecute)
        {
            ExecuteAction = action;
            CanExecuteAction = canExecute;
        }

        #region Methods

        public bool CanExecute(object parameter)
        {
            return CanExecuteAction(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            ExecuteAction(parameter);
        }

        #endregion
    }

}
