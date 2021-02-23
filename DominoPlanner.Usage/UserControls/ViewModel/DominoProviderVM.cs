using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using Avalonia.Collections;
using static DominoPlanner.Usage.ColorControl;
using Avalonia.Controls;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    using static Localizer;

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

            EditClick = new RelayCommand(o => { redoStack = new Stack<PostFilter>(); Editing = false; });
            OpenPopup = new RelayCommand(x => PopupOpen = true);

            ColorColumnConfig.Add(new Column() { DataField = "DominoColor.mediaColor", Header = "", Class = "Color" });
            ColorColumnConfig.Add(new Column() { DataField = "DominoColor.name", Header = _("Name"), Width = new GridLength(100), CanResize = true });
            ColorColumnConfig.Add(new Column() { DataField = "DominoColor.count", Header = GetParticularString("Number of stones available", "Total"), Class="Count", Width = new GridLength(70), CanResize = true });
            ColorColumnConfig.Add(new Column() { DataField = "SumAll", Header = GetParticularString("Number of stones used in current project", "Used"), HighlightDataField = "DominoColor.count" });
            ColorColumnConfig.Add(new Column() { DataField = "Weight", Header = GetParticularString("Emphasis during calculation", "Weight") });

            AllowRegeneration = AllowRegenerate;
        }
        #endregion

        #region fields
        public string[] TargetSizeAffectedProperties;
        public AvaloniaList<Column> ColorColumnConfig { get; set; } = new AvaloniaList<Column>();
        private ICommand _OpenPopup;
        public ICommand OpenPopup
        {
            get => _OpenPopup;
            set { if (value != _OpenPopup) { _OpenPopup = value; } }
        }
        int refrshCounter = 0;
        readonly Progress<String> progress = new Progress<string>(pr => Console.WriteLine(pr));
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

        public DominoTransfer DominoTransfer
        {
            get { return _dominoTransfer; }
            set
            {
                _dominoTransfer = value;
                RefreshPlanPic();

                PhysicalLength = DominoTransfer.PhysicalLength;
                PhysicalHeight = DominoTransfer.PhysicalHeight;
            }
        }
        #endregion

        #region prope
        private Cursor _cursorState;
        public Cursor Cursor
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
        private Bitmap _CurrentPlan;
        public Bitmap CurrentPlan
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
        public bool Draw_borders
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
        private Avalonia.Media.Color _backgroundColor = Avalonia.Media.Color.FromArgb(0, 255, 255, 255);
        public Avalonia.Media.Color BackgroundColor
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
                    if (!isTargetCountUpdating)
                    {
                        __dominoCount = value;
                        TabPropertyChanged(nameof(DominoCount), ProducesUnsavedChanges: false);
                    }
                }
            }
        }

        private bool isTargetCountUpdating = false;
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
                        isTargetCountUpdating = true;
                        PropertyValueChanged(this, value, producesUnsavedChanges: false, ChangesSize: true);
                        t.TargetCount = value;
                        isTargetCountUpdating = false;
                    }
                    __dominoCount = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }
        public ObservableCollection<ColorListEntry> UsedColors { get; set; }
        public ObservableCollection<ColorListEntry> SortedColors
        {
            get
            {
                return new ObservableCollection<ColorListEntry>(UsedColors.OrderBy(x => x.SortIndex));
            }
        }

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
        public Avalonia.Threading.Dispatcher dispatcher;
        protected void RefreshPlanPic()
        {
            void regenerate()
            {
                try
                {
                    var new_img = DominoTransfer.GenerateImage(BackgroundColor, 2000, Draw_borders, Collapsed).Snapshot();
                    CurrentPlan = Bitmap.DecodeToWidth(new_img.Encode().AsStream(), new_img.Width);
                    Cursor = null;
                    _dominoCount = DominoTransfer.Length;
                    PostCalculationUpdate();
                    RefreshColorAmount();
                }
                catch { }
            };
            if (AllowRegeneration == true)
            {
                System.Diagnostics.Debug.WriteLine(progress.ToString());
                if (dispatcher == null)
                {
                    regenerate();
                }
                else
                {
                    dispatcher.InvokeAsync(regenerate);
                }
            }
            else if (AllowRegeneration == null)
            {
                Cursor = null;
                _dominoCount = DominoTransfer.Length;
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
                Cursor = new Cursor(StandardCursorType.Wait);
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
                        catch { return CurrentProject.Last; }
                    });
                    DominoTransfer dt = await Task.Factory.StartNew<DominoTransfer>(function);
                    refrshCounter--;
                    if (refrshCounter == 0)
                    {
                        DominoTransfer = dt;
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
                        return CurrentProject.Last;
                    });
                    DominoTransfer dt = await Task.Factory.StartNew(function);
                    refrshCounter--;
                    if (refrshCounter == 0)
                    {
                        DominoTransfer = dt;
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
            TabPropertyChanged(nameof(SortedColors), false);
            ColorRestrictionFulfilled = fulfilled;
        }
        protected void OpenBuildTools()
        {
            ProtocolV protocolV = new ProtocolV
            {
                DataContext = new ProtocolVM(CurrentProject, name, assemblyname)
            };
            protocolV.Show(MainWindowViewModel.GetWindow());
        }
        public void RefreshTargetSize()
        {
            bool oldUndoState = undostate;
            undostate = true;
            if (DominoCount > 0)
                DominoCount += 1;
            undostate = oldUndoState;
        }
        public void PropertyValueChanged(object sender, object value_new, [CallerMemberName] string membername = "", bool producesUnsavedChanges = true, Action PostAction = null, bool ChangesSize = false, Action PostUndoAction = null)
        {
            if (ChangesSize)
            {
                if (!undostate)
                {
                    undostate = true;
                    if (producesUnsavedChanges)
                        UnsavedChanges = true;
                    try
                    {
                        var filter = new TargetSizeChangedOperation(sender, value_new, membername, PostAction ?? (() => Refresh()), PostUndoAction ?? PostAction ?? (() => Refresh()), TargetSizeAffectedProperties);
                        undoStack.Push(filter);
                        filter.Apply();
                        redoStack = new Stack<PostFilter>();
                    }
                    finally
                    {
                        undostate = false;
                    }
                }
            }
            else
            {
                base.PropertyValueChanged(sender, value_new, membername, producesUnsavedChanges, PostAction ?? (() => Refresh()), PostUndoAction);
            }
        } 


        #endregion

        #region Commands
        private ICommand _EditClick;
        public ICommand EditClick { get { return _EditClick; } set { if (value != _EditClick) { _EditClick = value; } } }

    #endregion

    }
}

