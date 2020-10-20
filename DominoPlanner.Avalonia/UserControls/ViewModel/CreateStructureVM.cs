using DominoPlanner.Core;

namespace DominoPlanner.UI.UserControls.ViewModel
{
    public class CreateStructureVM : DominoProviderVM
    {
        public CreateStructureVM(GeneralShapesProvider dominoProvider, bool? AllowRegenerate) : base(dominoProvider, AllowRegenerate)
        {
            _draw_borders = true;
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

