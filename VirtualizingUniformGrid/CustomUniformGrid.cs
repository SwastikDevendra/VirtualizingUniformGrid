using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VirtualizingUniformGrid
{
    [TemplatePart(Name = "PART_FirstPageButton", Type = typeof(Button)),
    TemplatePart(Name = "PART_PreviousPageButton", Type = typeof(Button)),
    TemplatePart(Name = "PART_PageTextBox", Type = typeof(TextBox)),
    TemplatePart(Name = "PART_NextPageButton", Type = typeof(Button)),
    TemplatePart(Name = "PART_LastPageButton", Type = typeof(Button)),
    TemplatePart(Name = "PART_PageSizesCombobox", Type = typeof(ComboBox))]
    public class CustomUniformGrid : ListView
    {

        #region CUSTOM CONTROL VARIABLES

        protected Button btnFirstPage, btnPreviousPage, btnNextPage, btnLastPage;
        protected TextBox txtPage;
        protected ComboBox cmbPageSizes;

        #endregion

        #region PROPERTIES

        public static readonly DependencyProperty ActualItemsSourceProperty;
        public static readonly DependencyProperty PageProperty;
        public static readonly DependencyProperty TotalPagesProperty;
        public static readonly DependencyProperty PageSizesProperty;
        public static readonly DependencyProperty PageContractProperty;
        public static readonly DependencyProperty FilterTagProperty;

        public IEnumerable<object> ActualItemsSource
        {
            get { return (IEnumerable<object>)GetValue(ActualItemsSourceProperty); }
            set { SetValue(ActualItemsSourceProperty, value); }
        }


        public uint Page
        {
            get
            {
                return (uint)GetValue(PageProperty);
            }
            set
            {
                SetValue(PageProperty, value);
            }
        }

        public uint TotalPages
        {
            get
            {
                return (uint)GetValue(TotalPagesProperty);
            }
            protected set
            {
                SetValue(TotalPagesProperty, value);
            }
        }

        public ObservableCollection<uint> PageSizes
        {
            get
            {
                return GetValue(PageSizesProperty) as ObservableCollection<uint>;
            }
        }

        public object FilterTag
        {
            get
            {
                return GetValue(FilterTagProperty);
            }
            set
            {
                SetValue(FilterTagProperty, value);
            }
        }

        #endregion

        #region EVENTS

        public delegate void PageChangedEventHandler(object sender, PageChangedEventArgs args);

        public static readonly RoutedEvent PreviewPageChangeEvent;
        public static readonly RoutedEvent PageChangedEvent;

        public static ObservableCollection<uint> visibleCells = new ObservableCollection<uint> { 1, 4, 16, 25 };

        public event PageChangedEventHandler PreviewPageChange
        {
            add
            {
                AddHandler(PreviewPageChangeEvent, value);
            }
            remove
            {
                RemoveHandler(PreviewPageChangeEvent, value);
            }
        }

        public event PageChangedEventHandler PageChanged
        {
            add
            {
                AddHandler(PageChangedEvent, value);
            }
            remove
            {
                RemoveHandler(PageChangedEvent, value);
            }
        }

        #endregion

        #region CONTROL CONSTRUCTORS

        static CustomUniformGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
           typeof(CustomUniformGrid),
           new FrameworkPropertyMetadata(typeof(CustomUniformGrid)));
            ActualItemsSourceProperty = DependencyProperty.Register("ActualItemsSource", typeof(IEnumerable<object>), typeof(CustomUniformGrid), 
                new FrameworkPropertyMetadata
                    (
                    new ObservableCollection<object>(),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(ActualItemsSourceChanged))
                    );
            PageProperty = DependencyProperty.Register("Page", typeof(uint), typeof(CustomUniformGrid));
            TotalPagesProperty = DependencyProperty.Register("TotalPages", typeof(uint), typeof(CustomUniformGrid));
            PageSizesProperty = DependencyProperty.Register("PageSizes", typeof(ObservableCollection<uint>), typeof(CustomUniformGrid), new PropertyMetadata(visibleCells));
            FilterTagProperty = DependencyProperty.Register("FilterTag", typeof(object), typeof(CustomUniformGrid));
            PreviewPageChangeEvent = EventManager.RegisterRoutedEvent("PreviewPageChange", RoutingStrategy.Bubble, typeof(PageChangedEventHandler), typeof(CustomUniformGrid));
            PageChangedEvent = EventManager.RegisterRoutedEvent("PageChanged", RoutingStrategy.Bubble, typeof(PageChangedEventHandler), typeof(CustomUniformGrid));
        }

        private static void ActualItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomUniformGrid uniformGrid = d as CustomUniformGrid;
            if (uniformGrid != null)
            {
                uniformGrid.BindProperties();
                uniformGrid.Navigate(PageChanges.Current);
            }
        }

        public CustomUniformGrid()
        {
            this.Loaded += new RoutedEventHandler(PaggingControl_Loaded);
        }

        ~CustomUniformGrid()
        {
            UnregisterEvents();
        }

        #endregion

        #region EVENTS

        void PaggingControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Template == null)
            {
                throw new Exception("Control template not assigned.");
            }

            RegisterEvents();
            SetDefaultValues();
            BindProperties();
        }

        void btnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            Navigate(PageChanges.First);
        }

        void btnPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            Navigate(PageChanges.Previous);
        }

        void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            Navigate(PageChanges.Next);
        }

        void btnLastPage_Click(object sender, RoutedEventArgs e)
        {
            Navigate(PageChanges.Last);
        }

        void txtPage_LostFocus(object sender, RoutedEventArgs e)
        {
            Navigate(PageChanges.Current);
        }

        void cmbPageSizes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Navigate(PageChanges.Current);
        }

        #endregion

        #region INTERNAL METHODS

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            btnFirstPage = this.Template.FindName("PART_FirstPageButton", this) as Button;
            btnPreviousPage = this.Template.FindName("PART_PreviousPageButton", this) as Button;
            txtPage = this.Template.FindName("PART_PageTextBox", this) as TextBox;
            btnNextPage = this.Template.FindName("PART_NextPageButton", this) as Button;
            btnLastPage = this.Template.FindName("PART_LastPageButton", this) as Button;
            cmbPageSizes = this.Template.FindName("PART_PageSizesCombobox", this) as ComboBox;

            if (btnFirstPage == null ||
                btnPreviousPage == null ||
                txtPage == null ||
                btnNextPage == null ||
                btnLastPage == null ||
                cmbPageSizes == null)
            {
                throw new Exception("Invalid Control template.");
            }
        }

        private void RegisterEvents()
        {
            btnFirstPage.Click += new RoutedEventHandler(btnFirstPage_Click);
            btnPreviousPage.Click += new RoutedEventHandler(btnPreviousPage_Click);
            btnNextPage.Click += new RoutedEventHandler(btnNextPage_Click);
            btnLastPage.Click += new RoutedEventHandler(btnLastPage_Click);
            txtPage.LostFocus += new RoutedEventHandler(txtPage_LostFocus);
            cmbPageSizes.SelectionChanged += new SelectionChangedEventHandler(cmbPageSizes_SelectionChanged);
        }

        private void UnregisterEvents()
        {
            btnFirstPage.Click -= btnFirstPage_Click;
            btnPreviousPage.Click -= btnPreviousPage_Click;
            btnNextPage.Click -= btnNextPage_Click;
            btnLastPage.Click -= btnLastPage_Click;
            txtPage.LostFocus -= txtPage_LostFocus;
            cmbPageSizes.SelectionChanged -= cmbPageSizes_SelectionChanged;
        }

        private void SetDefaultValues()
        {
            cmbPageSizes.IsEditable = false;
            cmbPageSizes.SelectedIndex = 0;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e != null)
            {
                if (e.Delta < 0)
                    Navigate(PageChanges.Next);
                else
                    Navigate(PageChanges.Previous);
            }
        }

        private void BindProperties()
        {
            Binding propBinding;
            propBinding = new Binding("Page");
            propBinding.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
            propBinding.Mode = BindingMode.TwoWay;
            propBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            txtPage.SetBinding(TextBox.TextProperty, propBinding);

            propBinding = new Binding("PageSizes");
            propBinding.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
            propBinding.Mode = BindingMode.TwoWay;
            propBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            cmbPageSizes.SetBinding(ComboBox.ItemsSourceProperty, propBinding);
            cmbPageSizes.SelectedItem = cmbPageSizes.Items.CurrentItem;
        }

        private void RaisePageChanged(uint OldPage, uint NewPage, ICollection<object> ItemsDisplayed)
        {
            PageChangedEventArgs args = new PageChangedEventArgs(PageChangedEvent, OldPage, NewPage, TotalPages, ItemsDisplayed);
            RaiseEvent(args);
        }

        private void RaisePreviewPageChange(uint OldPage, uint NewPage, ICollection<object> ItemsDisplayed)
        {
            PageChangedEventArgs args = new PageChangedEventArgs(PreviewPageChangeEvent, OldPage, NewPage, TotalPages, ItemsDisplayed);
            RaiseEvent(args);
        }

        private void Navigate(PageChanges change)
        {
            uint totalRecords;
            uint newPageSize;

            totalRecords = (uint)GetTotalCount();
            newPageSize = (uint)cmbPageSizes.SelectedItem;

            if (totalRecords == 0)
            {
                Items.Clear();
                TotalPages = 1;
                Page = 1;
            }
            else
            {
                TotalPages = (totalRecords / newPageSize) + (uint)((totalRecords % newPageSize == 0) ? 0 : 1);
            }

            uint newPage = 1;
            switch (change)
            {
                case PageChanges.First:
                    if (Page == 1)
                    {
                        return;
                    }
                    break;
                case PageChanges.Previous:
                    newPage = (Page - 1 > TotalPages) ? TotalPages : (Page - 1 < 1) ? 1 : Page - 1;
                    break;
                case PageChanges.Current:
                    newPage = (Page > TotalPages) ? TotalPages : (Page < 1) ? 1 : Page;
                    break;
                case PageChanges.Next:
                    newPage = (Page + 1 > TotalPages) ? TotalPages : Page + 1;
                    break;
                case PageChanges.Last:
                    if (Page == TotalPages)
                    {
                        return;
                    }
                    newPage = TotalPages;
                    break;
                default:
                    break;
            }

            uint StartingIndex = (newPage - 1) * newPageSize;

            uint oldPage = Page;
            ICollection<object> fetchData = GetRecordsBy(StartingIndex, newPageSize, FilterTag);
            RaisePreviewPageChange(Page, newPage, fetchData);

            Page = newPage;
            Items.Clear();

            foreach (object row in fetchData)
            {
                Items.Add(row);
            }

            RaisePageChanged(oldPage, Page, fetchData);
        }

        public uint GetTotalCount()
        {
            return (uint)ActualItemsSource.Count();
        }

        public ICollection<object> GetRecordsBy(uint StartingIndex, uint NumberOfRecords, object FilterTag)
        {
            if (StartingIndex >= ActualItemsSource.Count())
            {
                return new List<object>();
            }

            List<object> result = new List<object>();

            for (int i = (int)StartingIndex; i < ActualItemsSource.Count() && i < StartingIndex + NumberOfRecords; i++)
            {
                result.Add(ActualItemsSource.ElementAt(i));
            }

            return result.ToList<object>();
        }

        #endregion
    }

    internal enum PageChanges
    {
        First,
        Previous,
        Current,
        Next,
        Last
    }

    public class PageChangedEventArgs : RoutedEventArgs
    {
        #region PRIVATE VARIABLES

        private uint _oldPage, _newPage, _totalPages;
        private ICollection<object> _itemsInDisplay;

        #endregion

        #region PROPERTIES

        public uint OldPage
        {
            get
            {
                return _oldPage;
            }
        }

        public uint NewPage
        {
            get
            {
                return _newPage;
            }
        }

        public uint TotalPages
        {
            get
            {
                return _totalPages;
            }
        }

        public ICollection<object> ItemsInDisplay
        {
            get
            {
                return _itemsInDisplay;
            }
        }

        #endregion

        #region CONSTRUCTOR

        public PageChangedEventArgs(RoutedEvent EventToRaise, uint OldPage, uint NewPage, uint TotalPages, ICollection<object> itemsInDisplay)
            : base(EventToRaise)
        {
            _oldPage = OldPage;
            _newPage = NewPage;
            _totalPages = TotalPages;
            _itemsInDisplay = itemsInDisplay;
        }

        #endregion
    }
}
