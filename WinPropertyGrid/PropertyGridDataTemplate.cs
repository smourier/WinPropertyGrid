using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using WinPropertyGrid.Utilities;

namespace WinPropertyGrid
{
    [ContentProperty(Name = nameof(DataTemplate))]
    public class PropertyGridDataTemplate
    {
        private readonly Lazy<IReadOnlyList<Type>> _resolvedPropertyTypes;

        public PropertyGridDataTemplate()
        {
            _resolvedPropertyTypes = new Lazy<IReadOnlyList<Type>>(GetTypes);
        }

        public virtual DataTemplate? DataTemplate { get; set; }
        public virtual string? Name { get; set; } // can be compared to property's EditorDataTemplateResourceKey
        public virtual bool? IsReadOnly { get; set; }
        public virtual bool? IsEnum { get; set; }
        public virtual bool? IsFlagsEnum { get; set; }
        public virtual bool? IsEnumerable { get; set; }
        public virtual string? Category { get; set; }
        public virtual string? TypeNames { get; set; }

        public IReadOnlyList<Type> Types => _resolvedPropertyTypes.Value;
        private IReadOnlyList<Type> GetTypes()
        {
            var types = new List<Type>();
            var names = TypeNames.SplitToList<string>('|');
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var type = Type.GetType(name, false, true);
                if (type != null)
                {
                    types.Add(type);
                }
            }
            return types;
        }

        public virtual bool Matches(PropertyGridProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);
            if (IsReadOnly.HasValue && property.IsReadOnly != IsReadOnly.Value)
                return false;

            if (IsEnum.HasValue && property.IsEnum != IsEnum.Value)
                return false;

            if (IsFlagsEnum.HasValue && property.IsFlagsEnum != IsFlagsEnum.Value)
                return false;

            if (IsEnumerable.HasValue && property.IsEnumerable != IsEnumerable.Value)
                return false;

            if (Category != null && property.Category != Category)
                return false;

            if (!string.IsNullOrWhiteSpace(TypeNames) && !Types.Contains(property.Type))
                return false;

            return true;
        }
    }
}
