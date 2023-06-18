using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinPropertyGrid
{
    public partial class PropertyGrid : UserControl
    {
        public readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(PropertyGrid), new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).OnSelectedObjectChanged(e)));

        public PropertyGrid()
        {
            InitializeComponent();
        }

        public object? SelectedObject { get => (string)GetValue(SelectedObjectProperty); set => SetValue(SelectedObjectProperty, value); }
        protected virtual void OnSelectedObjectChanged(DependencyPropertyChangedEventArgs args)
        {
        }
    }
}
