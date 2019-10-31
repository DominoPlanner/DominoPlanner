using DominoPlanner.Core;
using DominoPlanner.Usage.UserControls.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Usage
{
    public class PropertyChangedOperation : PostFilter
    {
        public object sender;
        public object value_new;
        public object value_old;
        public string membername;
        public Action PostAction;
        public System.Reflection.PropertyInfo ps;
        public PropertyChangedOperation(object sender, object value_new, string membername, Action PostAction)
        {
            this.membername = membername;
            this.sender = sender;
            this.value_new = value_new;
            this.PostAction = PostAction;
            ps = sender.GetType().GetProperty(membername, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            value_old = ps.GetValue(sender);
        }

        public override void Apply()
        {
            ps.SetValue(sender, value_new);
            PostAction?.Invoke();
        }

        public override void Undo()
        {
            ps.SetValue(sender, value_old);
            PostAction?.Invoke();
        }
    }
    public class TargetSizeChangedOperation : PropertyChangedOperation
    {
        public Action UndoAction;
        public object[] oldAffectedValues;
        public System.Reflection.PropertyInfo[] affectedProperties;
        public TargetSizeChangedOperation(object sender, object value_new, string membername, Action PostAction, Action UndoAction, string[] affectedNames) : base(sender, value_new, membername, PostAction)
        {
            if (affectedNames != null)
            {
                this.UndoAction = UndoAction;
                affectedProperties = new System.Reflection.PropertyInfo[affectedNames.Length];
                oldAffectedValues = new object[affectedNames.Length];
                for (int i = 0; i < affectedNames.Length; i++)
                {
                    affectedProperties[i] = sender.GetType().GetProperty(affectedNames[i], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    oldAffectedValues[i] = affectedProperties[i].GetValue(sender);
                }
            }
        }
        public override void Apply()
        {
            ps.SetValue(sender, value_new);
            PostAction?.Invoke();
        }
        public override void Undo()
        {
            ps.SetValue(sender, value_old);
            if (affectedProperties != null)
            {
                for (int i = 0; i < affectedProperties.Length; i++)
                {
                    affectedProperties[i].SetValue(sender, oldAffectedValues[i]);
                }
            }
            UndoAction?.Invoke();
        }
    }
    public abstract class EditingChangedOperation : PostFilter
    {
        public DominoProviderTabItem OldViewModel { get; set; }
        public DominoProviderTabItem NewViewModel { get; set; }

        public virtual bool NewEditingValue { get; }



        public EditingChangedOperation(DominoProviderTabItem model)
        {
            OldViewModel = model;
        }

        public override void Apply()
        {
            OldViewModel.CurrentProject.Editing = NewEditingValue;
            if (NewViewModel == null)
            {
                NewViewModel = OldViewModel.GetNewViewModel(OldViewModel);
            }
            NewViewModel.undoStack = OldViewModel.undoStack;
            NewViewModel.redoStack = OldViewModel.redoStack;
            NewViewModel.GetNewViewModel = OldViewModel.GetNewViewModel;
            NewViewModel.RegisterNewViewModel = OldViewModel.RegisterNewViewModel;
            OldViewModel.RegisterNewViewModel(OldViewModel, NewViewModel);
            NewViewModel.Save();
        }
        public override void Undo()
        {
            OldViewModel.CurrentProject.Editing = !NewEditingValue;
            OldViewModel.undoStack = NewViewModel.undoStack;
            OldViewModel.redoStack = NewViewModel.redoStack;
            NewViewModel.RegisterNewViewModel(NewViewModel, OldViewModel);
            OldViewModel.Save();
        }
    }
    public class EditingActivatedOperation : EditingChangedOperation
    {
        public override bool NewEditingValue => true;
        public EditingActivatedOperation(DominoProviderVM vm) : base(vm) { }
        public override void Apply()
        {
            base.Apply();
            ((EditProjectVM)NewViewModel).DisplaySettingsTool.ResetCanvas();
        }
        public override void Undo()
        {

            //((DominoProviderVM)OldViewModel).CurrentProject.shapesValid = false;
            ((EditProjectVM)NewViewModel).DisplaySettingsTool.cleanEvents();
            base.Undo();

            //((DominoProviderVM)OldViewModel).Refresh();
        }
    }
    public class EditingDeactivatedOperation : EditingChangedOperation
    {
        public override bool NewEditingValue => false;
        public EditProjectVM cmodel { get => (EditProjectVM)OldViewModel; }
        private int current_width;
        private int current_height;
        private DominoTransfer last;
        public EditingDeactivatedOperation(EditProjectVM editProjectVM) : base(editProjectVM)
        {
        }

        public override void Apply()
        {

            last = (DominoTransfer)cmodel.CurrentProject.last.Clone();
            if (cmodel.CurrentProject is IRowColumnAddableDeletable rowc)
            {
                current_width = rowc.current_width;
                current_height = rowc.current_height;
            }
            ((EditProjectVM)OldViewModel).DisplaySettingsTool.cleanEvents();
            base.Apply();
            //((DominoProviderVM)OldViewModel).Refresh();

        }

        public override void Undo()
        {
            cmodel.CurrentProject.last = last;
            if (cmodel.CurrentProject is IRowColumnAddableDeletable rowc)
            {
                rowc.current_width = current_width;
                rowc.current_height = current_height;
            }
            base.Undo();
            cmodel.DisplaySettingsTool.ResetCanvas();

        }
    }
}
