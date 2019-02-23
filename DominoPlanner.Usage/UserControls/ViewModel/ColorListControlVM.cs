using DominoPlanner.Core;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DominoPlanner.Usage.UserControls.ViewModel
{
    public class ColorListControlVM : TabBaseVM
    {
        #region CTOR
        public ColorListControlVM(string filepath) : base()
        {
            ColorList = new ObservableCollection<ColorListEntry>();

            Reload(filepath);
            
            BtnAddColor = new RelayCommand(o => { AddNewColor(); });
            BtnSaveColors = new RelayCommand(o => { Save(); });
            BtnRemove = new RelayCommand(o => { RemoveSelected(); });
            BtnMoveDown = new RelayCommand(o => { MoveDown(); });
            BtnMoveUp = new RelayCommand(o => { MoveUp(); });
            base.UnsavedChanges = false;
            
            ShowProjects = false;
        }

        public ColorListControlVM(DominoAssembly dominoAssembly) : this(dominoAssembly.colorPath)
        {
            this.dominoAssembly = dominoAssembly;
            DifColumns = new ObservableCollection<DataGridColumn>();
            foreach (DocumentNode project in dominoAssembly.children)
            {
                int[] counts2 = Workspace.LoadColorList<FieldParameters>(Workspace.AbsolutePathFromReference(project.relativePath, dominoAssembly));
                for (int i = 0; i < counts2.Count(); i++)
                {
                    ColorList[i].ProjectCount.Add(counts2[i]);
                }
                AddProjectCountsColumn(Path.GetFileNameWithoutExtension(project.relativePath));
            }
        }
        #endregion

        #region Methods
        public void ResetList()
        {
            FilePath = String.Empty;
            colorRepository.Anzeigeindizes.CollectionChanged -= Anzeigeindizes_CollectionChanged;
            colorRepository = new ColorRepository();
            colorRepository.Anzeigeindizes.CollectionChanged += Anzeigeindizes_CollectionChanged;
            _ColorList.Clear();
            refreshList();
        }

        internal void Reload(string fileName)
        {
            FilePath = fileName;
            _ColorList.Clear();
            if (colorRepository != null) colorRepository.Anzeigeindizes.CollectionChanged -= Anzeigeindizes_CollectionChanged;
            colorRepository = Workspace.Load<ColorRepository>(FilePath);
            colorRepository.Anzeigeindizes.CollectionChanged += Anzeigeindizes_CollectionChanged;

            refreshList();
        }

        private void refreshList()
        {
            int counter = 0;
            foreach (DominoColor domino in colorRepository.RepresentionForCalculation.OfType<DominoColor>())
            {
                colorRepository = Workspace.Load<ColorRepository>(FilePath);
                _ColorList.Add(new ColorListEntry() { DominoColor = domino, SortIndex = colorRepository.Anzeigeindizes[counter] });
                counter++;
            }

            if (colorRepository.RepresentionForCalculation.OfType<EmptyDomino>().Count() == 1)
            {
                _ColorList.Add(new ColorListEntry() { DominoColor = colorRepository.RepresentionForCalculation.OfType<EmptyDomino>().First(), SortIndex = -1 });
            }
        }

        public override void Undo()
        {
            throw new NotImplementedException();
        }

        public override void Redo()
        {
            throw new NotImplementedException();
        }
        private void RemoveSelected()
        {
            throw new NotImplementedException();
            UnsavedChanges = true;
        }

        private void MoveUp()
        {
            try
            {
                if (SelectedStone.DominoColor is DominoColor dominoColor)
                {
                    colorRepository.MoveUp(dominoColor);
                }
                RaisePropertyChanged("ColorList");
            }
            catch (Exception) { }
        }

        private void MoveDown()
        {
            try
            {
                if (SelectedStone.DominoColor is DominoColor dominoColor)
                {
                    colorRepository.MoveDown(dominoColor);
                }
                RaisePropertyChanged("ColorList");
            }
            catch (Exception) { }
        }

        private void AddNewColor()
        {
            colorRepository.Add(new DominoColor(System.Windows.Media.Colors.IndianRed, 0, "New Color"));
            _ColorList.Add(new ColorListEntry() { DominoColor = colorRepository.RepresentionForCalculation.Last(), SortIndex = colorRepository.Anzeigeindizes.Last() });
            UnsavedChanges = true;
        }

        public override bool Save()
        {
            if(FilePath == string.Empty)
            {
                SaveFileDialog ofd = new SaveFileDialog();
                ofd.Filter = "domino color files (*.DColor)|*.DColor|All files (*.*)|*.*";
                if (ofd.ShowDialog() == true)
                {
                    if (ofd.FileName != string.Empty)
                    {
                        FilePath = ofd.FileName;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            colorRepository.Save(FilePath);
            UnsavedChanges = false;
            return true;
        }

        private void DominoColor_PropertyChanged(object sender, string e)
        {
            UnsavedChanges = true;
        }

        private void Anzeigeindizes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    _ColorList.Where(x => x.DominoColor is DominoColor).ElementAt(e.NewStartingIndex).SortIndex = (int)e.NewItems[0];
                    break;
            }
            UnsavedChanges = false;
            RaisePropertyChanged("ColorList");
        }
        #endregion

        #region COMMANDS
        private ICommand _BtnSendMail;
        public ICommand BtnSendMail { get { return _BtnSendMail; } set { if (value != _BtnSendMail) { _BtnSendMail = value; } } }

        private ICommand _BtnAddColor;
        public ICommand BtnAddColor { get { return _BtnAddColor; } set { if (value != _BtnAddColor) { _BtnAddColor = value; } } }

        private ICommand _BtnSaveColors;
        public ICommand BtnSaveColors { get { return _BtnSaveColors; } set { if (value != _BtnSaveColors) { _BtnSaveColors = value; } } }

        private ICommand _BtnRemove;
        public ICommand BtnRemove { get { return _BtnRemove; } set { if (value != _BtnRemove) { _BtnRemove = value; } } }

        private ICommand _BtnMoveDown;
        public ICommand BtnMoveDown { get { return _BtnMoveDown; } set { if (value != _BtnMoveDown) { _BtnMoveDown = value; } } }

        private ICommand _BtnMoveUp;
        public ICommand BtnMoveUp { get { return _BtnMoveUp; } set { if (value != _BtnMoveUp) { _BtnMoveUp = value; } } }
        #endregion

        #region prop
        private DominoAssembly dominoAssembly;
        public DominoAssembly DominoAssembly
        {
            get { return dominoAssembly; }
        }
        private ColorListEntry _SelectedStone;
        public ColorListEntry SelectedStone
        {
            get { return _SelectedStone; }
            set
            {
                if (_SelectedStone != value)
                {
                    if (_SelectedStone != null)
                        _SelectedStone.DominoColor.PropertyChanged -= DominoColor_PropertyChanged;
                    _SelectedStone = value;
                    if (_SelectedStone != null)
                        _SelectedStone.DominoColor.PropertyChanged += DominoColor_PropertyChanged;
                    RaisePropertyChanged();
                }
            }
        }
        public override TabItemType tabType
        {
            get
            {
                return TabItemType.ColorList;
            }
        }

        private ColorRepository _ColorRepository;
        public ColorRepository colorRepository
        {
            get { return _ColorRepository; }
            set
            {
                if (_ColorRepository != value)
                {
                    _ColorRepository = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ColorListEntry> _ColorList;
        public ObservableCollection<ColorListEntry> ColorList
        {
            get { return new ObservableCollection<ColorListEntry>(_ColorList.OrderBy(x => x.SortIndex)); }
            set
            {
                if (_ColorList != value)
                {
                    if (_ColorList != null) _ColorList.CollectionChanged -= _ColorList_CollectionChanged;
                    _ColorList = value;
                    _ColorList.CollectionChanged += _ColorList_CollectionChanged;
                    RaisePropertyChanged();
                }
            }
        }

        private void _ColorList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("ColorList");
        }

        private ObservableCollection<DataGridColumn> _DifColumns;
        public ObservableCollection<DataGridColumn> DifColumns
        {
            get { return _DifColumns; }
            set
            {
                if (_DifColumns != value)
                {
                    _DifColumns = value;
                }
            }
        }

        private bool _ShowProjects;
        public bool ShowProjects
        {
            get { return _ShowProjects; }
            set
            {
                if (_ShowProjects != value)
                {
                    _ShowProjects = value;
                    RaisePropertyChanged();
                }
            }
        }


        private void AddProjectCountsColumn(string projectName)
        {
            Binding amountBinding = new Binding(string.Format("ProjectCount[{0}]", DifColumns.Count.ToString()));
            amountBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            DataTemplate amountLabel = new DataTemplate();
            FrameworkElementFactory _textboxFactory = new FrameworkElementFactory(typeof(TextBlock));

            DataGridTemplateColumn c = new DataGridTemplateColumn();
            c.Header = projectName;
            FrameworkElementFactory textFactory = new FrameworkElementFactory(typeof(Label));
            textFactory.SetValue(Label.ContentProperty, amountBinding);

            DataTemplate columnTemplate = new DataTemplate();
            columnTemplate.VisualTree = textFactory;
            c.CellTemplate = columnTemplate;
            DifColumns.Add(c);
        }
        #endregion
    }
}