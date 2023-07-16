using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using WinPropertyGrid.Utilities;

namespace WinPropertyGrid
{
    public class PropertyGridEnum : INotifyPropertyChanged
    {
        private readonly IReadOnlyList<PropertyGridEnumItem> _items;
        private PropertyGridEnumItem? _valueItem;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PropertyGridEnum(PropertyGridProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);
            Property = property;
            _items = GetItems();
            _valueItem = _items.FirstOrDefault(i => i.IsChecked);
        }

        public PropertyGridProperty Property { get; }
        public IReadOnlyList<PropertyGridEnumItem> Items => _items;
        public virtual PropertyGridEnumItem? ValueItem
        {
            get => _valueItem;
            set
            {
                if (_valueItem == value)
                    return;

                Property.SetValue(value?.Value, DictionaryObjectPropertySetOptions.None);
                _valueItem = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) => OnPropertyChanged(this, new PropertyChangedEventArgs(name));
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

        protected virtual bool ShowEnumField(Type type, string name, out string displayName)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(name);

            var fi = type.GetField(name, BindingFlags.Static | BindingFlags.Public);
            if (fi == null)
                throw new ArgumentException(null, nameof(type));

            displayName = fi.Name;
            var ba = fi.GetCustomAttribute<BrowsableAttribute>();
            if (ba != null && !ba.Browsable)
                return false;

            var da = fi.GetCustomAttribute<DescriptionAttribute>();
            if (da != null && !string.IsNullOrWhiteSpace(da.Description))
            {
                displayName = da.Description;
            }
            return true;
        }

        protected virtual IReadOnlyList<PropertyGridEnumItem> GetItems()
        {
            var isEnumOrNullableEnum = Property.Type.IsEnumOrNullableEnum(out var enumType, out var nullable);
            if (!isEnumOrNullableEnum)
                return Array.Empty<PropertyGridEnumItem>();

            var items = new List<PropertyGridEnumItem>();
            PropertyGridEnumItem? zeroItem = null;
            PropertyGridEnumItem? nullItem = null;
            if (nullable)
            {
                nullItem = Property.GridObject.Grid.CreateEnumItem(this, Property.GridObject.Grid.NullEnumName, null);
                items.Add(nullItem);
            }

            var uvalue = Conversions.EnumToUInt64(Property.Value);
            if (!uvalue.HasValue && !nullable) // wrong value in property
            {
                uvalue = 0;
            }

            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType);
            if (enumType.IsFlagsEnum())
            {
                for (var i = 0; i < names.Length; i++)
                {
                    var nameValue = Conversions.EnumToUInt64(values.GetValue(i)!);
                    if (!ShowEnumField(enumType, names[i], out var displayName))
                        continue;

                    var item = Property.GridObject.Grid.CreateEnumItem(this, displayName, nameValue);
                    items.Add(item);

                    if (nameValue == 0)
                    {
                        zeroItem = item;
                    }

                    item.IsChecked = uvalue.HasValue && (uvalue.Value & nameValue) != 0;
                }
            }
            else
            {
                for (var i = 0; i < names.Length; i++)
                {
                    if (!ShowEnumField(enumType, names[i], out var displayName))
                        continue;

                    var item = Property.GridObject.Grid.CreateEnumItem(this, displayName, values.GetValue(i));
                    items.Add(item);

                    var nameValue = Conversions.EnumToUInt64(values.GetValue(i)!);
                    item.IsChecked = uvalue.HasValue && uvalue.Value == nameValue;
                }
            }

            // we don't want an empty list
            if (items.Count == 0)
            {
                var item = Property.GridObject.Grid.CreateEnumItem(this, Property.GridObject.Grid.ZeroEnumName, 0);
                items.Add(item);
            }

            if (uvalue.HasValue)
            {
                if (uvalue.Value == 0 && zeroItem != null)
                {
                    zeroItem.IsChecked = true;
                }
            }
            else
            {
                if (nullItem != null)
                {
                    nullItem.IsChecked = true;
                }
                else if (zeroItem != null)
                {
                    zeroItem.IsChecked = true;
                }
            }

            return items.AsReadOnly();
        }
    }
}
