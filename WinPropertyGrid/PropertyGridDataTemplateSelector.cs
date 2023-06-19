using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using WinPropertyGrid.Utilities;

namespace WinPropertyGrid
{
    [ContentProperty(Name = nameof(DataTemplates))]
    public class PropertyGridDataTemplateSelector : DataTemplateSelector
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ObservableCollection<PropertyGridDataTemplate> DataTemplates { get; } = new();

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is PropertyGridProperty property)
            {
                var att = property.Descriptor?.GetAttribute<PropertyGridPropertyAttribute>();
                if (att?.EditorDataTemplateResourceKey != null)
                {
                    if (Application.Current.Resources.TryGetValue(att.EditorDataTemplateResourceKey, out var value) && value is DataTemplate dt)
                        return dt;

                    if (container is FrameworkElement fe && fe.Resources.TryGetValue(att.EditorDataTemplateResourceKey, out value) && value is DataTemplate dt2)
                        return dt2;
                }

                foreach (var template in DataTemplates)
                {
                    if (template.Matches(property) && template.DataTemplate != null)
                        return template.DataTemplate;
                }
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}
