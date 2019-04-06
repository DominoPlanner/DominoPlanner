using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class CreateStructureVM : DominoProviderVM
    {
        public CreateStructureVM(GeneralShapesProvider dominoProvider, bool? AllowRegenerate) : base(dominoProvider, AllowRegenerate)
        {
            _draw_borders = true;
            Collapsible = Visibility.Collapsed;
            if (CurrentProject.HasProtocolDefinition)
                VisibleFieldplan = Visibility.Visible;
            else
                VisibleFieldplan = Visibility.Hidden;
            if (CurrentProject.Counts != null) RefreshColorAmount();
            UnsavedChanges = false;
        }
        public override TabItemType tabType
        {
            get
            {
                return TabItemType.CreateStructure;
            }
        }
    }
}

