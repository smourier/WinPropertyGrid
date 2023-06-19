using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        }

        public PropertyGrid Grid { get; }
        public object Data { get; }
        public ObservableCollection<PropertyGridProperty> Properties { get; } = new();
        public virtual IComparer<PropertyGridProperty>? Comparer { get; set; }

        public virtual PropertyGridProperty GetOrAddProperty(string propertyName, Type type, string name)
        {
            ArgumentNullException.ThrowIfNull(propertyName);
            return Properties.FirstOrDefault(p => p.Name == propertyName) ?? CreateProperty(type, name);
        }

        public virtual DynamicObject CreateDynamicObject() => new();
        public virtual PropertyGridProperty CreateProperty(Type type, string name) => new(this, type, name);

        protected virtual void Describe(PropertyGridProperty property, PropertyDescriptor descriptor)
        {
            ArgumentNullException.ThrowIfNull(property);
            ArgumentNullException.ThrowIfNull(descriptor);

            property.Category = string.IsNullOrWhiteSpace(descriptor.Category) || descriptor.Category.EqualsIgnoreCase(CategoryAttribute.Default.Category) ? Grid.DefaultCategoryName : descriptor.Category;
            property.IsReadOnly = descriptor.IsReadOnly;
            property.Description = descriptor.Description;
            property.DisplayName = descriptor.DisplayName;
            if (Grid.DecamelizePropertiesDisplayNames)
            {
                property.DisplayName = Conversions.Decamelize(property.DisplayName);
            }

            property.IsEnum = descriptor.PropertyType.IsEnum;
            property.IsFlagsEnum = descriptor.PropertyType.IsEnum && descriptor.PropertyType.IsFlagsEnum();

            var options = descriptor.Attributes.OfType<PropertyGridPropertyAttribute>().FirstOrDefault();
            if (options != null)
            {
                if (options.SortOrder != 0)
                {
                    property.SortOrder = options.SortOrder;
                }

                property.IsEnum = options.IsEnum;
                property.IsFlagsEnum = options.IsFlagsEnum;
            }

            var att = descriptor.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
            if (att != null)
            {
                property.HasDefaultValue = true;
                property.DefaultValue = att.Value;
            }
            else if (options != null)
            {
                if (options.HasDefaultValue)
                {
                    property.HasDefaultValue = true;
                    property.DefaultValue = options.DefaultValue;
                }
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

            property ??= CreateProperty(descriptor.PropertyType, descriptor.Name);
            if (property == null)
                return null;

            Describe(property, descriptor);
            if (forceReadWrite)
            {
                property.IsReadOnly = false;
            }

            RefreshProperty(property, descriptor, DictionaryObjectPropertySetOptions.TrackChanges);
            return property;
        }

        public virtual void RefreshProperty(PropertyGridProperty property, PropertyDescriptor descriptor, DictionaryObjectPropertySetOptions options)
        {
            ArgumentNullException.ThrowIfNull(property);
            ArgumentNullException.ThrowIfNull(descriptor);

            var value = descriptor.GetValue(Data);
            property.SetValue(value, options);
        }

        public virtual void ScanProperties()
        {
            Properties.Clear();
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