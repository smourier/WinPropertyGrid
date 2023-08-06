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
using Windows.Foundation;
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
        private bool _flyoutFlagsEnumPropertyGridTemplateCloseOk;
        private PointerPoint? _splitterPoint;
        private ScrollViewer? _namesScroll;
        private ScrollViewer? _valuesScroll;

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
        public virtual string NullEnumName { get; set; } = "<unset>";
        public virtual string ZeroEnumName { get; set; } = "<none>";

        public object? SelectedObject { get => GetValue(SelectedObjectProperty); set => SetValue(SelectedObjectProperty, value); }
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
        }

        protected virtual FrameworkElement CreateErrorElement(PropertyGridObject obj, PropertyGridProperty property, ListViewItemPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentNullException.ThrowIfNull(property);
            ArgumentNullException.ThrowIfNull(presenter);

            var element = new InfoBadge
            {
                Margin = new Thickness(4, 0, 0, 0),
                Background = new SolidColorBrush(Colors.Red),
                Width = 16,
                Value = property.Errors.Count,
            };
            ToolTipService.SetToolTip(element, property.ValueErrors);
            return element;
        }

        protected internal virtual void OnSelectedObjectPropertyErrorsChanged(PropertyGridObject obj, PropertyGridProperty property)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentNullException.ThrowIfNull(property);

            var hasErrors = property.HasErrors;

            // we can have more than one presenter per property
            foreach (var presenter in ValuesList.EnumerateChildren(true).OfType<ListViewItemPresenter>().Where(p => p.DataContext == property))
            {
                var errorElement = CreateErrorElement(obj, property, presenter);
                if (errorElement == null)
                    continue;

                var name = typeof(PropertyGrid).FullName;
                errorElement.Name = name;

                update(null, null);
                void update(FrameworkElement? sender, EffectiveViewportChangedEventArgs? e)
                {
                    var xf = presenter!.TransformToVisual(ValuesList);
                    var pt = xf.TransformPoint(new Point(presenter.ActualOffset.X, presenter.ActualOffset.Y));
                    Canvas.SetLeft(errorElement, pt.X);
                    Canvas.SetTop(errorElement, pt.Y);
                }

                var existing = ErrorsCanvas.Children.OfType<FrameworkElement>().FirstOrDefault(e => e.Name == name);
                if (hasErrors)
                {
                    if (existing == null)
                    {
                        ErrorsCanvas.Children.Add(errorElement);
                        presenter.EffectiveViewportChanged += update;
                    }
                }
                else
                {
                    if (existing != null)
                    {
                        presenter.EffectiveViewportChanged -= update;
                        ErrorsCanvas.Children.Remove(existing);
                    }
                }
            }
        }

        // TODO
        public bool IsGrouped { get => (bool)GetValue(IsGroupedProperty); set => SetValue(IsGroupedProperty, value); }
        protected virtual void OnIsGroupedChanged(DependencyPropertyChangedEventArgs args)
        {
            ViewSource.IsSourceGrouped = (bool)args.NewValue;
        }

        public virtual PropertyGridObject CreateGridObject(object data) => new(this, data);
        public virtual PropertyGridProperty CreateProperty(PropertyGridObject gridObject, Type type, string name) => new(gridObject, type, name);
        public virtual PropertyGridEnum CreateEnum(PropertyGridProperty property) => new(property);
        public virtual PropertyGridEnumItem CreateEnumItem(PropertyGridEnum @enum, string name, object? value) => new(@enum, name, value);

        protected internal virtual bool CompareForEquality(object? o1, object? o2) => Equals(o1, o2);

        protected virtual void SynchronizeListHeights()
        {
            var names = NamesList.EnumerateChildren(true).OfType<ListViewItemPresenter>().GetEnumerator();
            foreach (var value in ValuesList.EnumerateChildren(true).OfType<ListViewItemPresenter>())
            {
                names.MoveNext();
                // poor man's check... could be improved to match names & values but it will take more perf
                if (value.DataContext == names.Current.DataContext)
                {
                    names.Current.Height = value.ActualHeight;
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
            }
        }

        private void ValuesListViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_valuesScroll != null)
            {
                _namesScroll?.ScrollToVerticalOffset(_valuesScroll.VerticalOffset);
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

        private void OnFlyoutFlagsEnumPropertyGridTemplateOpening(object sender, object e)
        {
            _flyoutFlagsEnumPropertyGridTemplateCloseOk = false;
            var menu = (MenuFlyout)sender;
            if (menu.Target.DataContext is not PropertyGridProperty property)
                return;

            menu.Items.Clear();
            var items = property.Enum?.Items;
            if (items == null)
                return;

            foreach (var item in items)
            {
                var menuItem = new ToggleMenuFlyoutItem { Text = item.Name, IsChecked = item.IsChecked };
                menuItem.Click += (s, e) =>
                {
                    item.IsChecked = !item.IsChecked;
                    // refresh all items
                    foreach (var child in menu.Items.OfType<ToggleMenuFlyoutItem>())
                    {
                        var childItem = child.CommandParameter as PropertyGridEnumItem;
                        if (childItem != null)
                        {
                            child.IsChecked = childItem.IsChecked;
                        }
                    }
                };
                menuItem.CommandParameter = item;
                menu.Items.Add(menuItem);
            }

            menu.Items.Add(new MenuFlyoutSeparator());
            var close = new MenuFlyoutItem { Text = "\uE10B", FontFamily = new FontFamily("Segoe MDL2 Assets") }; // accept symbol
            close.Click += (s, e) =>
            {
                _flyoutFlagsEnumPropertyGridTemplateCloseOk = true;
                menu.Hide();
            };
            menu.Items.Add(close);
        }

        private void OnFlyoutFlagsEnumPropertyGridTemplateClosing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
        {
            args.Cancel = !_flyoutFlagsEnumPropertyGridTemplateCloseOk;
        }
    }
}
