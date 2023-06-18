using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace WinPropertyGrid
{
    public partial class PropertyGrid : UserControl
    {
        public readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(PropertyGrid), new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).OnSelectedObjectChanged(e)));
        public readonly DependencyProperty IsGroupedProperty = DependencyProperty.Register(nameof(IsGrouped), typeof(bool), typeof(PropertyGrid), new PropertyMetadata(true, (d, e) => ((PropertyGrid)d).OnIsGroupedChanged(e)));

        public PropertyGrid()
        {
            InitializeComponent();
            DefaultCategoryName = CategoryAttribute.Default.Category;
        }

        public CollectionViewSource ViewSource { get; } = new CollectionViewSource();
        public virtual string DefaultCategoryName { get; set; }
        public virtual bool DecamelizePropertiesDisplayNames { get; set; }

        public object? SelectedObject { get => (string)GetValue(SelectedObjectProperty); set => SetValue(SelectedObjectProperty, value); }
        protected virtual void OnSelectedObjectChanged(DependencyPropertyChangedEventArgs args)
        {
            ViewSource.Source = args.NewValue;
        }

        public bool IsGrouped { get => (bool)GetValue(IsGroupedProperty); set => SetValue(IsGroupedProperty, value); }
        protected virtual void OnIsGroupedChanged(DependencyPropertyChangedEventArgs args)
        {
            ViewSource.IsSourceGrouped = (bool)args.NewValue;
        }

        protected internal virtual bool CompareForEquality(object? o1, object? o2) => Equals(o1, o2);
    }
}
