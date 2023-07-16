using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using WinPropertyGrid.Utilities;

namespace WinPropertyGrid
{
    public class PropertyGridEnum : INotifyPropertyChanged
    {
        private PropertyGridEnumItem? _valueItem;
        private Type? _enumType;
        private bool _nullable;
        private bool _flags;
        private bool _blocked;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PropertyGridEnum(PropertyGridProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);
            Property = property;
            BuildItems();
            _valueItem = Items.FirstOrDefault(i => i.IsChecked);
        }

        public PropertyGridProperty Property { get; }
        public ObservableCollection<PropertyGridEnumItem> Items { get; } = new ObservableCollection<PropertyGridEnumItem>();

        // only valid for non flags enums
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

        public override string ToString()
        {
            var list = new List<string>();
            foreach (var item in Items)
            {
                if (item._value == 0)
                {
                    // zero is on => display only zero
                    if (item.IsChecked)
                        return item.Name;

                    // otherwise don't display it
                    continue;
                }

                if (item.IsChecked)
                {
                    list.Add(item.Name);
                }
            }

            return string.Join(", ", list);
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
            if (!string.IsNullOrWhiteSpace(da?.Description))
            {
                displayName = da.Description;
            }
            return true;
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_blocked)
                return;

            _blocked = true;
            var eventItem = (PropertyGridEnumItem)sender!;

            // 0 is special
            if (eventItem._value == 0)
            {
                if (eventItem.IsChecked)
                {
                    foreach (var item in Items)
                    {
                        item.IsChecked = item._value == 0;
                        item.IsEnabled = item._value != 0;
                    }

                    Property.SetValue(0, DictionaryObjectPropertySetOptions.None);
                    _blocked = false;
                    return;
                }

                eventItem.IsChecked = true;
                _blocked = false;
                return;
            }

            var value = 0UL;
            foreach (var item in Items)
            {
                if (item.IsChecked)
                {
                    if (item.IsNull)
                    {
                        Property.SetValue(null, DictionaryObjectPropertySetOptions.None);
                        return;
                    }

                    value |= item._value;
                }
            }

            if (!eventItem.IsChecked)
            {
                value &= ~eventItem._value;
            }

            foreach (var item in Items)
            {
                item.IsEnabled = item._value != 0 || value != 0;
                if (item._value == 0)
                {
                    item.IsChecked = value == 0;
                }
                else
                {
                    item.IsChecked = (value & item._value) == item._value;
                }
            }

            var enumValue = Conversions.ChangeType(value, _enumType!);
            Property.SetValue(enumValue, DictionaryObjectPropertySetOptions.None);
            _blocked = false;
        }

        protected virtual void BuildItems()
        {
            foreach (var item in Items)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }
            Items.Clear();

            var isEnumOrNullableEnum = Property.Type.IsEnumOrNullableEnum(out _enumType, out _nullable);
            if (!isEnumOrNullableEnum)
                return;

            _flags = _enumType.IsFlagsEnum();

            PropertyGridEnumItem? zeroItem = null;
            PropertyGridEnumItem? nullItem = null;
            if (_nullable)
            {
                nullItem = Property.GridObject.Grid.CreateEnumItem(this, Property.GridObject.Grid.NullEnumName, null);
                if (nullItem != null)
                {
                    nullItem.IsNull = true;
                    Items.Add(nullItem);
                }
            }

            var uvalue = Conversions.EnumToUInt64(Property.Value);
            if (!uvalue.HasValue && !_nullable) // wrong value in property
            {
                uvalue = 0;
            }

            var names = Enum.GetNames(_enumType);
            var values = Enum.GetValues(_enumType);
            if (_flags)
            {
                for (var i = 0; i < names.Length; i++)
                {
                    var nameValue = Conversions.EnumToUInt64(values.GetValue(i)!)!.Value;
                    if (!ShowEnumField(_enumType, names[i], out var displayName))
                        continue;

                    var item = Property.GridObject.Grid.CreateEnumItem(this, displayName, nameValue);
                    if (item == null)
                        continue;

                    item._value = nameValue;
                    Items.Add(item);

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
                    if (!ShowEnumField(_enumType, names[i], out var displayName))
                        continue;

                    var item = Property.GridObject.Grid.CreateEnumItem(this, displayName, values.GetValue(i));
                    if (item == null)
                        continue;

                    Items.Add(item);

                    var nameValue = Conversions.EnumToUInt64(values.GetValue(i)!);
                    item.IsChecked = uvalue.HasValue && uvalue.Value == nameValue;
                }
            }

            // we don't want an empty list
            if (Items.Count == 0)
            {
                var item = Property.GridObject.Grid.CreateEnumItem(this, Property.GridObject.Grid.ZeroEnumName, 0);
                if (item != null)
                {
                    Items.Add(item);
                }
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

            if (_flags)
            {
                foreach (var item in Items)
                {
                    item.PropertyChanged += OnItemPropertyChanged;
                }
            }
        }
    }
}
