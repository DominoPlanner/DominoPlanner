using System.Windows.Input;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class TabCommands
    {
        public ICommand CopyCom { get; set; }
        public ICommand PasteCom { get; set; }
        public ICommand SelectAllCom { get; set; }
        public ICommand AddRowAbove { get; set; }
        public ICommand AddRowBelow { get; set; }
        public ICommand AddColumnLeft { get; set; }
        public ICommand AddColumnRight { get; set; }
        public ICommand RemoveRows { get; set; }
        public ICommand RemoveColumns { get; set; }
    }
}
