using DominoPlanner.Core;
using DominoPlanner.Usage.HelperClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class DominoProviderVM : TabBaseVM
    {
        #region CTOR
        public DominoProviderVM(IDominoProvider dominoProvider, bool? AllowRegenerate) : base()
        {
            CurrentProject = dominoProvider;
            CalculationVM = CalculationVM.CalculationVMFactory(CurrentProject.PrimaryCalculation);
            CalculationVM.Refresh = Refresh;
            ImageTreatmentVM = ImageTreatmentVM.ImageTreatmentVMFactory(CurrentProject.PrimaryImageTreatment);
            ImageTreatmentVM.Refresh = Refresh;

            FillColorList();

            BuildtoolsClick = new RelayCommand(o => { OpenBuildTools(); });

            EditClick = new RelayCommand(o => { CurrentProject.Editing = true; });
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
        public ColumnConfig ColorColumnConfig { get; set; } = new ColumnConfig();
        private ICommand _OpenPopup;
        public ICommand OpenPopup
        {
            get => _OpenPopup;
            set { if (value != _OpenPopup) { _OpenPopup = value; } }
        }

        string name;
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
        private IDominoProvider _CurrentProject;
        public override IDominoProvider CurrentProject
        {
            get { return _CurrentProject; }
            set
            {
                if (_CurrentProject != value)
                {
                    _CurrentProject = value;
                    if (CurrentProject != null)
                    {
                        if (CurrentProject.HasProtocolDefinition)
                            VisibleFieldplan = Visibility.Visible;
                        else
                            VisibleFieldplan = Visibility.Hidden;
                    }
                }
            }
        }
        private Visibility _VisibleFieldplan;
        public Visibility VisibleFieldplan
        {
            get { return _VisibleFieldplan; }
            set
            {
                if (_VisibleFieldplan != value)
                {
                    _VisibleFieldplan = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _physicalLength;
        public int PhysicalLength
        {
            get
            {
                return _physicalLength;
            }
            set
            {
                if (_physicalLength != value)
                {
                    _physicalLength = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }

        private int _physicalHeight;
        public int PhysicalHeight
        {
            get { return _physicalHeight; }
            set
            {
                if (_physicalHeight != value)
                {
                    _physicalHeight = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }
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
                    _draw_borders = value;
                    Refresh();
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
                    _collapsed = value;
                    Refresh();
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                }
            }
        }
        private Visibility _collapsible;
        public virtual Visibility Collapsible
        {
            get => _collapsible;
            set
            {
                if (_collapsible != value)
                {
                    _collapsible = value;
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
                    _backgroundColor = value;
                    TabPropertyChanged(ProducesUnsavedChanges: false);
                    Refresh();
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
                    __dominoCount = value;
                    if (CurrentProject is ICountTargetable t)
                    {
                        t.TargetCount = __dominoCount;
                        Refresh();
                    }
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
                        catch {  }
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
        public override void Undo()
        {
            throw new NotImplementedException();
        }

        public override void Redo()
        {
            throw new NotImplementedException();
        }

        public override bool Save()
        {
            try
            {
                CurrentProject.Save();
                UnsavedChanges = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
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
            protocolV.DataContext = new ProtocolVM(CurrentProject, name);
            protocolV.ShowDialog();
        }
        public void RefreshTargetSize()
        {
            if (DominoCount > 0)
                DominoCount = DominoCount + 1;
            else
            {
                Refresh();
            }
        }
        #endregion

        #region Commands
        private ICommand _EditClick;
        public ICommand EditClick { get { return _EditClick; } set { if (value != _EditClick) { _EditClick = value; } } }

        private ICommand _BuildtoolsClick;
        public ICommand BuildtoolsClick { get { return _BuildtoolsClick; } set { if (value != _BuildtoolsClick) { _BuildtoolsClick = value; } } }
        #endregion

    }

}

