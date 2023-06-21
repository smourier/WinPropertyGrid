using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
                    var dt = DataTemplates.FirstOrDefault(dt => dt != null && dt.Name?.Equals(att.EditorDataTemplateResourceKey) == true)?.DataTemplate;
                    if (dt != null)
                        return dt;

                    foreach (var dic in EnumerateResourceDictionaries(item, container))
                    {
                        if (dic.TryGetValue(att.EditorDataTemplateResourceKey, out var value) && value is DataTemplate dt2)
                            return dt2;
                    }
                }

                foreach (var template in DataTemplates)
                {
                    if (template.Matches(property) && template.DataTemplate != null)
                        return template.DataTemplate;
                }
            }
            return base.SelectTemplateCore(item, container);
        }

        protected virtual IEnumerable<ResourceDictionary> EnumerateResourceDictionaries(object item, DependencyObject container)
        {
            if (container is FrameworkElement fe)
                yield return fe.Resources;

            if (item is PropertyGridProperty property)
                yield return property.GridObject.Grid.Resources;

            yield return Application.Current.Resources;
        }
    }
}
