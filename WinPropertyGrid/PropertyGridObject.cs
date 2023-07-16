using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using WinPropertyGrid.Utilities;

namespace WinPropertyGrid
{
    public class PropertyGridObject : IListSource
    {
        public PropertyGridObject(PropertyGrid grid, object data)
        {
            ArgumentNullException.ThrowIfNull(grid);
            ArgumentNullException.ThrowIfNull(data);
            Grid = grid;
            Data = data;
            Comparer = grid.Comparer;
            ScanProperties();
            if (Data is INotifyPropertyChanged pc)
            {
                pc.PropertyChanged += OnDataPropertyChanged;
            }
        }

        public PropertyGrid Grid { get; }
        public object Data { get; }
        public ObservableCollection<PropertyGridProperty> Properties { get; } = new();
        public ExpandoObject DynamicProperties { get; } = new ExpandoObject();
        public virtual IComparer<PropertyGridProperty>? Comparer { get; set; }

        private void OnDataPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (Grid.DispatcherQueue.HasThreadAccess)
            {
                OnSelectedObjectPropertyChanged(sender, e);
            }
            else
            {
                Grid.DispatcherQueue.TryEnqueue(() => OnSelectedObjectPropertyChanged(sender, e));
            }
        }

        protected virtual void OnSelectedObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var property = Properties.FirstOrDefault(p => p.Name == e.PropertyName);
            if (property == null)
                return;

            RefreshProperty(property, DictionaryObjectPropertySetOptions.ForceRaiseOnPropertyChanged);
        }

        protected internal virtual void OnSelectedObjectPropertyErrorsChanged(PropertyGridProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);
            Grid.OnSelectedObjectPropertyErrorsChanged(this, property);
        }

        protected virtual void Describe(PropertyGridProperty property, PropertyDescriptor descriptor)
        {
            ArgumentNullException.ThrowIfNull(property);
            ArgumentNullException.ThrowIfNull(descriptor);

            property.Descriptor = descriptor;
            property.Category = string.IsNullOrWhiteSpace(descriptor.Category) || descriptor.Category.EqualsIgnoreCase(CategoryAttribute.Default.Category) ? Grid.DefaultCategoryName : descriptor.Category;
            property.IsReadOnly = descriptor.IsReadOnly;
            property.Description = descriptor.Description;
            property.DisplayName = descriptor.DisplayName;
            if (Grid.DecamelizePropertiesDisplayNames)
            {
                property.DisplayName = Conversions.Decamelize(property.DisplayName);
            }

            property.IsEnum = descriptor.PropertyType.IsEnumOrNullableEnum(out var enumType, out var nullable);
            if (property.IsEnum && nullable)
            {
                property.IsFlagsEnum = enumType.IsFlagsEnum();
            }
            else
            {
                property.IsFlagsEnum = descriptor.PropertyType.IsEnum && descriptor.PropertyType.IsFlagsEnum();
            }

            AddDynamicProperties(property.DynamicProperties, descriptor.Attributes);

            var optionsAtt = descriptor.Attributes.OfType<PropertyGridPropertyAttribute>().FirstOrDefault();
            if (optionsAtt != null)
            {
                if (optionsAtt.SortOrder != 0)
                {
                    property.SortOrder = optionsAtt.SortOrder;
                }
            }

            var defaultValueAtt = descriptor.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
            if (defaultValueAtt != null)
            {
                property.HasDefaultValue = true;
                property.DefaultValue = defaultValueAtt.Value;
            }
            else if (optionsAtt != null)
            {
                if (optionsAtt.HasDefaultValue)
                {
                    property.HasDefaultValue = true;
                    property.DefaultValue = optionsAtt.DefaultValue;
                }
            }
        }

        private static void AddDynamicProperties(IDictionary<string, object?> dic, AttributeCollection attributes)
        {
            dic.Clear();
            foreach (var dynamicAtt in attributes.OfType<PropertyGridDynamicPropertyAttribute>())
            {
                if (string.IsNullOrWhiteSpace(dynamicAtt.Name))
                    continue;

                var value = dynamicAtt.Value;
                if (dynamicAtt.Type != null && dynamicAtt.ConvertToType && dynamicAtt.Value != null && !dynamicAtt.Type.IsAssignableFrom(dynamicAtt.Value.GetType()))
                {
                    value = Conversions.ChangeType(value, dynamicAtt.Type);
                }

                dic[dynamicAtt.Name] = value;
            }
        }

        public virtual PropertyGridProperty? CreateProperty(PropertyDescriptor descriptor)
        {
            ArgumentNullException.ThrowIfNull(descriptor);

            var forceReadWrite = false;
            PropertyGridProperty? property = null;
            var options = descriptor.Attributes.OfType<PropertyGridPropertyAttribute>().FirstOrDefault();
            if (options != null)
            {
                if (options.Ignore)
                    return null;

                forceReadWrite = options.ForceReadWrite;
                if (options.Type != null)
                {
                    property = Activator.CreateInstance(options.Type, this) as PropertyGridProperty;
                }
            }

            if (property == null)
            {
                options = descriptor.PropertyType.CustomAttributes.OfType<PropertyGridPropertyAttribute>().FirstOrDefault();
                if (options != null)
                {
                    if (!forceReadWrite)
                    {
                        forceReadWrite = options.ForceReadWrite;
                    }

                    if (options.Type != null)
                    {
                        property = Activator.CreateInstance(options.Type, this) as PropertyGridProperty;
                    }
                }
            }

            property ??= Grid.CreateProperty(this, descriptor.PropertyType, descriptor.Name);
            if (property == null)
                return null;

            Describe(property, descriptor);
            if (forceReadWrite)
            {
                property.IsReadOnly = false;
            }

            RefreshProperty(property, DictionaryObjectPropertySetOptions.DontRaiseOnPropertyChanged);
            return property;
        }

        public virtual void RefreshProperty(PropertyGridProperty property, DictionaryObjectPropertySetOptions options)
        {
            ArgumentNullException.ThrowIfNull(property);
            var descriptor = property.Descriptor;
            if (descriptor != null)
            {
                var value = descriptor.GetValue(Data);
                property.SetValue(value, options);
            }
        }

        public virtual void ScanProperties()
        {
            Properties.Clear();
            AddDynamicProperties(DynamicProperties, TypeDescriptor.GetAttributes(Data));

            var props = new List<PropertyGridProperty>();
            foreach (var descriptor in TypeDescriptor.GetProperties(Data).Cast<PropertyDescriptor>())
            {
                if (!descriptor.IsBrowsable)
                    continue;

                var property = CreateProperty(descriptor);
                if (property != null)
                {
                    props.Add(property);
                }
            }

            if (Comparer != null)
            {
                props.Sort(Comparer);
            }

            foreach (var property in props)
            {
                Properties.Add(property);
            }
        }

        protected internal virtual bool CompareForEquality(object? o1, object? o2) => Grid.CompareForEquality(o1, o2);

        bool IListSource.ContainsListCollection => false;
        IList IListSource.GetList() => Properties;
    }
}