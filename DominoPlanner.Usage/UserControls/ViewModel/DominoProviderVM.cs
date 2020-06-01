using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class DominoProviderVM : DominoProviderTabItem
    {
        #region CTOR
        public DominoProviderVM(IDominoProvider dominoProvider, bool? AllowRegenerate) : base()
        {
            CurrentProject = dominoProvider;
            CalculationVM = CalculationVM.CalculationVMFactory(CurrentProject.PrimaryCalculation);
            ImageTreatmentVM = ImageTreatmentVM.ImageTreatmentVMFactory(CurrentProject.PrimaryImageTreatment);

            FillColorList();

            BuildtoolsClick = new RelayCommand(o => { OpenBuildTools(); });

            EditClick = new RelayCommand(o => { redoStack = new ObservableStack<PostFilter>();  Editing = false; });
            OpenPopup = new RelayCommand(x => PopupOpen = true);
            ColorColumnConfig = new ColumnConfig();

            var columns = new ObservableCollection<Column>();
            columns.Add(new Column() { DataField = "DominoColor.mediaColor", Header = "" });
            columns.Add(new Column() { DataField = "DominoColor.name", Header = "Name" });
            columns.Add(new Column() { DataField = "DominoColor.count", Header = "Total" });
            columns.Add(new Column() { DataField = "SumAll", Header = "Used", HighlightDataField = "DominoColor.count" });
            columns.Add(new Column() { DataField = "Weight", Header = "Weight" });
            ColorColumnConfig.Columns = columns;

            AllowRegeneration = AllowRegenerate;
        }
        #endregion

        #region fields
        public string[] TargetSizeAffectedProperties;
        public ColumnConfig ColorColumnConfig { get; set; } = new ColumnConfig();
        private ICommand _OpenPopup;
        public ICommand OpenPopup
        {
            get => _OpenPopup;
            set { if (value != _OpenPopup) { _OpenPopup = value; } }
        }
        int refrshCounter = 0;
        Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
        private DominoTransfer _dominoTransfer;

        public bool? AllowRegeneration { get; set; } = false;


        private ImageTreatmentVM _imageTreatmentVM;
        public ImageTreatmentVM ImageTreatmentVM
        {
            get => _imageTreatmentVM;
            set
            {
                if (value != _imageTreatmentVM)
                {
                    _imageTreatmentVM = value;
                    _imageTreatmentVM.ValueChanged = PropertyValueChanged;
                    RaisePropertyChanged();
                }
            }
        }
        private CalculationVM _calculationVM;
        public CalculationVM CalculationVM
        {
            get => _calculationVM;
            set
            {
                if (value != _calculationVM)
                {
                    _calculationVM = value;
                    _calculationVM.ValueChanged = PropertyValueChanged;
                    RaisePropertyChanged();
                }
            }
        }

        public DominoTransfer dominoTransfer
        {
            get { return _dominoTransfer; }
            set
            {
                _dominoTransfer = value;
                refreshPlanPic();

                PhysicalLength = dominoTransfer.physicalLength;
                PhysicalHeight = dominoTransfer.physicalHeight;
            }
        }
        #endregion

        #region prope
        private Cursor _cursorState;
        public Cursor cursor
        {
            get { return _cursorState; }
            set
            {
                if (_cursorState != value)
                {
                    _cursorState = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }

        public override TabItemType tabType
        {
            get
            {
                return TabItemType.CreateField;
            }
        }
        private WriteableBitmap _CurrentPlan;
        public WriteableBitmap CurrentPlan
        {
            get { return _CurrentPlan; }
            set
            {
                if (_CurrentPlan != value)
                {
                    _CurrentPlan = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }
        protected bool _draw_borders;
        public bool draw_borders
        {
            get { return _draw_borders; }
            set
            {
                if (_draw_borders != value)
                {
                    PropertyValueChanged(this, value, producesUnsavedChanges: false);
                    _draw_borders = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }

        private bool _collapsed;
        public bool Collapsed
        {
            get { return _collapsed; }
            set
            {
                if (_collapsed != value)
                {
                    PropertyValueChanged(this, value, producesUnsavedChanges: false);
                    _collapsed = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }
        private System.Windows.Media.Color _backgroundColor = System.Windows.Media.Color.FromArgb(0, 255, 255, 255);
        public System.Windows.Media.Color backgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                if (_backgroundColor != value)
                {
                    PropertyValueChanged(this, value, producesUnsavedChanges: false);
                    _backgroundColor = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }


        private int __dominoCount;
        // wird nach der Berechnung aktualisiert
        public int _dominoCount
        {
            get { return __dominoCount; }
            set
            {
                if (__dominoCount != value)
                {
                    __dominoCount = value;
                    TabPropertyChanged("DominoCount", ProducesUnsavedChanges: false);
                }
            }
        }

        // entspricht Targetcount, daran wird gebunden
        public int DominoCount
        {
            get { return __dominoCount; }
            set
            {
                if (__dominoCount != value)
                {
                    if (CurrentProject is ICountTargetable t)
                    {
                        PropertyValueChanged(this, value, producesUnsavedChanges: false, ChangesSize: true);
                        t.TargetCount = value;
                    }
                    __dominoCount = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }

        public ObservableCollection<ColorListEntry> UsedColors { get; set; }
        private bool _ColorRestrictionFulfilled;

        public bool ColorRestrictionFulfilled
        {
            get { return _ColorRestrictionFulfilled; }
            set { _ColorRestrictionFulfilled = value; TabPropertyChanged(ProducesUnsavedChanges: false); }
        }
        private bool _popupOpen;

        public bool PopupOpen
        {
            get
            {
                return _popupOpen;
            }
            set
            {
                _popupOpen = value; RaisePropertyChanged();
            }
        }
        public System.Windows.Threading.Dispatcher dispatcher;
        protected void refreshPlanPic()
        {
            if (AllowRegeneration == true)
            {
                System.Diagnostics.Debug.WriteLine(progress.ToString());
                if (dispatcher == null)
                {
                    CurrentPlan = ImageConvert.ToWriteableBitmap(dominoTransfer.GenerateImage(backgroundColor, 2000, draw_borders, Collapsed).Bitmap);
                    cursor = null;
                    _dominoCount = dominoTransfer.length;
                    PostCalculationUpdate();
                    RefreshColorAmount();
                }
                else
                {
                    dispatcher.BeginInvoke((Action)(() =>
                    {
                        try
                        {
                            WriteableBitmap newBitmap = ImageConvert.ToWriteableBitmap(dominoTransfer.GenerateImage(backgroundColor, 2000, draw_borders, Collapsed).Bitmap);
                            CurrentPlan = newBitmap;
                            _dominoCount = dominoTransfer.length;
                            PostCalculationUpdate();
                            RefreshColorAmount();
                            cursor = null;
                        }
                        catch { }
                    }));
                }
            }
            else if (AllowRegeneration == null)
            {
                cursor = null;
                _dominoCount = dominoTransfer.length;
                PostCalculationUpdate();
            }
        }
        protected virtual void PostCalculationUpdate()
        {

        }
        CancellationTokenSource cs;
        protected async void Refresh()
        {
            if (AllowRegeneration != false)
            {
                cs?.Cancel();
                cs = new CancellationTokenSource();
                cursor = Cursors.Wait;
                refrshCounter++;
                if (AllowRegeneration == true)
                {
                    // complete regenerate
                    Func<DominoTransfer> function = new Func<DominoTransfer>(() =>
                    {
                        try
                        {
                            return CurrentProject.Generate(cs.Token, progress);
                        }
                        catch { return CurrentProject.last; }
                    });
                    DominoTransfer dt = await Task.Factory.StartNew<DominoTransfer>(function);
                    refrshCounter--;
                    if (refrshCounter == 0)
                    {
                        dominoTransfer = dt;
                    }
                }
                else if (AllowRegeneration == null)
                {
                    // regenerate only the shapes
                    Func<DominoTransfer> function = new Func<DominoTransfer>(() =>
                    {
                        try
                        {
                            CurrentProject.RegenerateShapes();
                        }
                        catch { }
                        return CurrentProject.last;
                    });
                    DominoTransfer dt = await Task.Factory.StartNew(function);
                    refrshCounter--;
                    if (refrshCounter == 0)
                    {
                        dominoTransfer = dt;
                    }
                }
            }
        }
        #endregion

        #region Methods
        

        
        protected void FillColorList()
        {
            UsedColors = new ObservableCollection<ColorListEntry>();

            int counter = 0;
            foreach (DominoColor domino in CurrentProject.colors.RepresentionForCalculation.OfType<DominoColor>())
            {
                UsedColors.Add(new ColorListEntry() { DominoColor = domino, SortIndex = CurrentProject.colors.Anzeigeindizes[counter] });
                counter++;
            }

            if (CurrentProject.colors.RepresentionForCalculation.OfType<EmptyDomino>().Count() == 1)
            {
                UsedColors.Add(new ColorListEntry() { DominoColor = CurrentProject.colors.RepresentionForCalculation.OfType<EmptyDomino>().First(), SortIndex = -1 });
            }
        }
        protected void RefreshColorAmount()
        {
            bool fulfilled = true;
            for (int i = 0; i < UsedColors.Count(); i++)
            {
                UsedColors[i].ProjectCount.Clear();
                if (CurrentProject.Counts.Length > i + 1)
                {
                    UsedColors[i].ProjectCount.Add(CurrentProject.Counts[i + 1]);
                    if ((((UncoupledCalculation)CurrentProject.PrimaryCalculation).IterationInformation).weights != null)
                    {
                        UsedColors[i].Weight = ((UncoupledCalculation)CurrentProject.PrimaryCalculation).IterationInformation.weights[i + 1];
                    }
                    if (UsedColors[i].SumAll > UsedColors[i].DominoColor.count)
                        fulfilled = false;
                }
                else
                {
                    UsedColors[i].ProjectCount.Add(CurrentProject.Counts[0]);
                }
            }
            ColorRestrictionFulfilled = fulfilled;
        }
        protected void OpenBuildTools()
        {
            ProtocolV protocolV = new ProtocolV();
            protocolV.DataContext = new ProtocolVM(CurrentProject, name, assemblyname);
            protocolV.ShowDialog();
        }
        public void RefreshTargetSize()
        {
            bool oldUndoState = undostate;
            undostate = true;
            if (DominoCount > 0)
                DominoCount = DominoCount + 1;
            undostate= oldUndoState;
        }

        public void PropertyValueChanged(object sender, object value_new,
            [CallerMemberName] string membername = "", bool producesUnsavedChanges = true, Action PostAction = null, bool ChangesSize = false, Action PostUndoAction = null)
        {
            if (!undostate)
            {
                try
                {
                    undostate = true;
                    if (producesUnsavedChanges)
                        UnsavedChanges = true;
                    if (ChangesSize)
                    {
                        var filter = new TargetSizeChangedOperation(sender, value_new, membername, PostAction ?? (() => Refresh()), PostUndoAction ?? PostAction ?? (() => Refresh()), TargetSizeAffectedProperties);
                        undoStack.Push(filter);
                        filter.Apply();
                    }
                    else
                    {
                        var filter = new PropertyChangedOperation(sender, value_new, membername, PostAction ?? (() => Refresh()));
                        if (undoStack.Count != 0)
                        {
                            var lastOnStack = undoStack.Peek();
                            if (lastOnStack is PropertyChangedOperation op)
                            {
                                if (op.sender == sender && op.membername == membername)
                                {
                                    // property has been changed multiple times in a row
                                    if (!op.value_old.Equals(value_new))
                                    {
                                        op.value_new = value_new;
                                        undoStack.Pop();
                                        filter = op;
                                    }
                                }
                            }
                        }
                        undoStack.Push(filter);
                        filter.Apply();
                    }
                    redoStack = new ObservableStack<PostFilter>();
                }
                finally
                {
                    undostate = false;
                }
            }
        }
        #endregion

        #region Commands
        private ICommand _EditClick;
        public ICommand EditClick { get { return _EditClick; } set { if (value != _EditClick) { _EditClick = value; } } }

    #endregion

    }
}

