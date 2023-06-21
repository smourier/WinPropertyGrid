using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Core;
using WinPropertyGrid.Utilities;

namespace WinPropertyGrid
{
    public partial class PropertyGrid : UserControl
    {
        public readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(PropertyGrid), new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).OnSelectedObjectChanged(e)));
        public readonly DependencyProperty IsGroupedProperty = DependencyProperty.Register(nameof(IsGrouped), typeof(bool), typeof(PropertyGrid), new PropertyMetadata(true, (d, e) => ((PropertyGrid)d).OnIsGroupedChanged(e)));

        private Brush? _splitterBrush;
        private double _openPaneLength;
        private PointerPoint? _splitterPoint;
        private ScrollViewer? _namesScroll;
        private ScrollViewer? _valuesScroll;
        private ScrollViewer? _errorsScroll;

        public PropertyGrid()
        {
            InitializeComponent();
            DefaultCategoryName = CategoryAttribute.Default.Category;
            Splitter.PointerEntered += OnSplitterPointerEntered;
            Splitter.PointerMoved += OnSplitterPointerMoved;
            Splitter.PointerExited += OnSplitterPointerExited;
            Splitter.PointerPressed += OnSplitterPointerPressed;
            Splitter.PointerReleased += OnSplitterPointerReleased;
            Comparer = PropertyGridPropertyComparer.Instance;
            NamesList.Loaded += OnNamesListLoaded;
            ValuesList.Loaded += OnValuesListLoaded;
            ErrorsList.Loaded += OnErrorListLoaded;
            NamesList.LayoutUpdated += (s, e) =>
            {
                if (SynchronizeNamesListHeights)
                {
                    SynchronizeListHeights();
                }
            };
        }

        public CollectionViewSource ViewSource { get; } = new CollectionViewSource();
        public PropertyGridObject? SelectedGridObject { get; private set; }
        public virtual bool SynchronizeNamesListHeights { get; set; } = true; // set to false if names & values list heights are always the same (better perf)
        public virtual string DefaultCategoryName { get; set; }
        public virtual int MinSplitterWidth { get; set; } = 100;
        public virtual bool DecamelizePropertiesDisplayNames { get; set; }
        public virtual IComparer<PropertyGridProperty>? Comparer { get; set; }

        public object? SelectedObject { get => (string)GetValue(SelectedObjectProperty); set => SetValue(SelectedObjectProperty, value); }
        protected virtual void OnSelectedObjectChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null)
            {
                ViewSource.Source = null;
                return;
            }

            SelectedGridObject = CreateGridObject(args.NewValue);
            ViewSource.Source = SelectedGridObject?.Properties;
            NamesList.ItemsSource = ViewSource.View;
            ValuesList.ItemsSource = ViewSource.View;
            ErrorsList.ItemsSource = ViewSource.View;
        }

        // TODO
        public bool IsGrouped { get => (bool)GetValue(IsGroupedProperty); set => SetValue(IsGroupedProperty, value); }
        protected virtual void OnIsGroupedChanged(DependencyPropertyChangedEventArgs args)
        {
            ViewSource.IsSourceGrouped = (bool)args.NewValue;
        }

        public virtual PropertyGridObject CreateGridObject(object data) => new(this, data);
        protected internal virtual bool CompareForEquality(object? o1, object? o2) => Equals(o1, o2);

        protected virtual void SynchronizeListHeights()
        {
            var names = NamesList.EnumerateChildren(true).OfType<ListViewItemPresenter>().GetEnumerator();
            var errors = ErrorsList.EnumerateChildren(true).OfType<ListViewItemPresenter>().GetEnumerator();
            foreach (var value in ValuesList.EnumerateChildren(true).OfType<ListViewItemPresenter>())
            {
                names.MoveNext();
                // poor man's check... could be improved to match names & values but it will take more perf
                if (value.DataContext == names.Current.DataContext)
                {
                    names.Current.Height = value.ActualHeight;
                }
            }

            foreach (var value in ErrorsList.EnumerateChildren(true).OfType<ListViewItemPresenter>())
            {
                errors.MoveNext();
                if (value.DataContext == errors.Current.DataContext)
                {
                    errors.Current.Height = value.ActualHeight;
                }
            }
        }

        private void OnValuesListLoaded(object sender, RoutedEventArgs e)
        {
            _valuesScroll = ValuesList.EnumerateChildren(true).FirstOrDefault(c => c is ScrollViewer) as ScrollViewer;
            if (_valuesScroll != null)
            {
                _valuesScroll.ViewChanged += ValuesListViewChanged;
            }
        }

        private void OnErrorListLoaded(object sender, RoutedEventArgs e)
        {
            _errorsScroll = ErrorsList.EnumerateChildren(true).FirstOrDefault(c => c is ScrollViewer) as ScrollViewer;
        }

        private void OnNamesListLoaded(object sender, RoutedEventArgs e)
        {
            _namesScroll = NamesList.EnumerateChildren(true).FirstOrDefault(c => c is ScrollViewer) as ScrollViewer;
            if (_namesScroll != null)
            {
                _namesScroll.ViewChanged += NamesListViewChanged;
            }
        }

        private void NamesListViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_namesScroll != null)
            {
                _valuesScroll?.ScrollToVerticalOffset(_namesScroll.VerticalOffset);
                _errorsScroll?.ScrollToVerticalOffset(_namesScroll.VerticalOffset);
            }
        }

        private void ValuesListViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_valuesScroll != null)
            {
                _namesScroll?.ScrollToVerticalOffset(_valuesScroll.VerticalOffset);
                _errorsScroll?.ScrollToVerticalOffset(_valuesScroll.VerticalOffset);
            }
        }

        private void OnSplitterPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Splitter.CapturePointer(e.Pointer);
            _splitterPoint = e.GetCurrentPoint(this);
        }

        private void OnSplitterPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Splitter.ReleasePointerCapture(e.Pointer);
        }

        private void OnSplitterPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _splitterBrush = Splitter.Background;
            Splitter.SetCursor(CoreCursorType.SizeWestEast);
        }

        private void OnSplitterPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_splitterPoint != null && Splitter.PointerCaptures?.Any(p => p.PointerId == e.Pointer.PointerId) == true)
            {
                PropertiesSplit.OpenPaneLength = Math.Max(Math.Max(10, MinSplitterWidth), _openPaneLength + e.GetCurrentPoint(this).Position.X - _splitterPoint.Position.X);
                return;
            }

            Splitter.Background = new SolidColorBrush(Colors.Gray);
            _openPaneLength = PropertiesSplit.OpenPaneLength;
        }

        private void OnSplitterPointerExited(object sender, PointerRoutedEventArgs e)
        {
            Splitter.Background = _splitterBrush;
            Splitter.SetCursor(null);
        }

        private void OnEmptyGuid(object sender, RoutedEventArgs e)
        {
            var prop = e.GetFromDatacontext<PropertyGridProperty>();
            if (prop == null)
                return;

            if (prop.Type == typeof(Guid) || prop.Type == typeof(Guid?))
            {
                prop.Value = Guid.Empty;
            }
        }

        private void OnIncrementGuid(object sender, RoutedEventArgs e)
        {
            var prop = e.GetFromDatacontext<PropertyGridProperty>();
            if (prop == null)
                return;

            if (prop.Type == typeof(Guid) || prop.Type == typeof(Guid?))
            {
                if (prop.Value is Guid g)
                {
                    var bytes = g.ToByteArray();
                    bytes[15]++;
                    prop.Value = new Guid(bytes);
                }
            }
        }

        private void OnNewGuid(object sender, RoutedEventArgs e)
        {
            var prop = e.GetFromDatacontext<PropertyGridProperty>();
            if (prop == null)
                return;

            if (prop.Type == typeof(Guid) || prop.Type == typeof(Guid?))
            {
                prop.Value = Guid.NewGuid();
            }
        }
    }
}
