using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

        public PropertyGrid()
        {
            InitializeComponent();
            DefaultCategoryName = CategoryAttribute.Default.Category;
            Splitter.PointerEntered += OnSplitterPointerEntered;
            Splitter.PointerMoved += OnSplitterPointerMoved;
            Splitter.PointerExited += OnSplitterPointerExited;
            Splitter.PointerPressed += OnSplitterPointerPressed;
            Splitter.PointerReleased += OnSplitterPointerReleased;
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

        protected CollectionViewSource ViewSource { get; } = new CollectionViewSource();
        public PropertyGridObject? SelectedGridObject { get; private set; }
        public virtual string DefaultCategoryName { get; set; }
        public virtual int MinSplitterWidth { get; set; } = 100;
        public virtual bool DecamelizePropertiesDisplayNames { get; set; }

        public object? SelectedObject { get => (string)GetValue(SelectedObjectProperty); set => SetValue(SelectedObjectProperty, value); }
        protected virtual void OnSelectedObjectChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null)
            {
                ViewSource.Source = null;
                return;
            }

            SelectedGridObject = new PropertyGridObject(this, args.NewValue);
            ViewSource.Source = SelectedGridObject.Properties;
            NamesList.ItemsSource = ViewSource.View;
            ValuesList.ItemsSource = ViewSource.View;
        }

        public bool IsGrouped { get => (bool)GetValue(IsGroupedProperty); set => SetValue(IsGroupedProperty, value); }
        protected virtual void OnIsGroupedChanged(DependencyPropertyChangedEventArgs args)
        {
            ViewSource.IsSourceGrouped = (bool)args.NewValue;
        }

        protected internal virtual bool CompareForEquality(object? o1, object? o2) => Equals(o1, o2);
    }
}
