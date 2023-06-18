using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace WinPropertyGrid.Utilities
{
    public class DynamicObject : ICustomTypeDescriptor, INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly List<Attribute> _attributes = new();
        private readonly List<EventDescriptor> _events = new();
        private readonly List<PropertyDescriptor> _properties = new();
        private readonly Dictionary<Type, object> _editors = new();
        private readonly Dictionary<string, object?> _values = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual DynamicObjectProperty AddProperty(string name, Type type, IEnumerable<Attribute> attributes)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(type);

            if (_properties.Find(x => x.Name == name) != null)
                throw new ArgumentException(null, nameof(name));

            var property = CreateProperty(name, type, attributes);
            _properties.Add(property);
            return property;
        }

        public virtual DynamicObjectProperty AddProperty(string name, Type type, object? defaultValue, bool readOnly, int sortOrder, Attribute[] attributes)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(type);
            if (_properties.Find(x => x.Name == name) != null)
                throw new ArgumentException(null, nameof(name));

            List<Attribute> newAtts;
            if (attributes != null)
            {
                newAtts = new List<Attribute>(attributes);
            }
            else
            {
                newAtts = new List<Attribute>();
            }

            newAtts.RemoveAll(a => a is ReadOnlyAttribute);
            newAtts.RemoveAll(a => a is DefaultValueAttribute);

            if (readOnly)
            {
                newAtts.Add(new ReadOnlyAttribute(true));
            }
            newAtts.Add(new DefaultValueAttribute(defaultValue));

            var property = CreateProperty(name, type, newAtts.ToArray());
            property.SortOrder = sortOrder;
            _properties.Add(property);
            return property;
        }

        public void SortProperties(IComparer<PropertyDescriptor> comparer)
        {
            ArgumentNullException.ThrowIfNull(comparer);
            _properties.Sort(comparer);
        }

        public virtual object? GetPropertyValue(string name, Type type, object? defaultValue)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(type);

            defaultValue = Conversions.ChangeType(defaultValue, type);
            if (_values.TryGetValue(name, out var obj))
                return Conversions.ChangeType(obj, type, defaultValue);

            return defaultValue;
        }

        public virtual T? GetPropertyValue<T>(string name, T? defaultValue = default)
        {
            ArgumentNullException.ThrowIfNull(name);
            if (_values.TryGetValue(name, out var obj))
                return Conversions.ChangeType(obj, defaultValue);

            return defaultValue;
        }

        public virtual bool TryGetPropertyValue(string name, out object? value)
        {
            ArgumentNullException.ThrowIfNull(name);
            return _values.TryGetValue(name, out value);
        }

        public virtual object? GetPropertyValue(string name, object? defaultValue)
        {
            ArgumentNullException.ThrowIfNull(name);
            if (_values.TryGetValue(name, out var obj))
                return obj;

            return defaultValue;
        }

        public virtual void SetPropertyValue(string name, object? value)
        {
            ArgumentNullException.ThrowIfNull(name);
            var exists = _values.TryGetValue(name, out var existing);
            if (!exists)
            {
                _values.Add(name, value);
            }
            else
            {
                if (value == null)
                {
                    if (existing == null)
                        return;

                }
                else if (value.Equals(existing))
                    return;

                _values[name] = value;
            }
            OnPropertyChanged(name);
        }

        protected virtual DynamicObjectProperty CreateProperty(string name, Type type, IEnumerable<Attribute> attributes)
        {
            ArgumentNullException.ThrowIfNull(name);
            return new DynamicObjectProperty(name, type, attributes);
        }

        public override string ToString() => ToStringName ?? base.ToString() ?? string.Empty;

        public virtual string? ToStringName { get; set; }
        public virtual string? ClassName { get; set; }
        public virtual TypeConverter? Converter { get; set; }
        public virtual EventDescriptor? DefaultEvent { get; set; }
        public virtual string? ComponentName { get; set; }
        public virtual PropertyDescriptor? DefaultProperty { get; set; }
        public virtual IDictionary<Type, object>? Editors => _editors;
        public virtual IList<EventDescriptor>? Events => _events;
        public virtual IList<PropertyDescriptor>? Properties => _properties;
        public virtual IList<Attribute>? Attributes => _attributes;

        string ICustomTypeDescriptor.GetClassName() => ClassName ?? string.Empty;
        string ICustomTypeDescriptor.GetComponentName() => ComponentName ?? string.Empty;

        string IDataErrorInfo.Error => Validate()!;
        string IDataErrorInfo.this[string columnName] => ValidateMember(columnName)!;
        TypeConverter ICustomTypeDescriptor.GetConverter() => Converter!;
        AttributeCollection ICustomTypeDescriptor.GetAttributes() => new(_attributes.ToArray());
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() => DefaultEvent!;
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() => DefaultProperty!;
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() => new(_events.ToArray());
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() => new(_properties.ToArray());
        object? ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor? pd) => this;
        object? ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            if (editorBaseType == null)
                throw new ArgumentNullException(nameof(editorBaseType));

            _editors.TryGetValue(editorBaseType, out var editor);
            return editor;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[]? attributes)
        {
            if (attributes == null || attributes.Length == 0)
                return ((ICustomTypeDescriptor)this).GetEvents();

            var list = new List<EventDescriptor>();
            foreach (var evt in _events)
            {
                if (evt.Attributes.Count == 0)
                    continue;

                var cont = false;
                foreach (var att in attributes)
                {
                    if (!HasMatchingAttribute(evt, att))
                    {
                        cont = true;
                        break;
                    }
                }

                if (!cont)
                {
                    list.Add(evt);
                }
            }
            return new EventDescriptorCollection(list.ToArray());
        }

        private static bool HasMatchingAttribute(MemberDescriptor member, Attribute attribute)
        {
            var att = member.Attributes[attribute.GetType()];
            if (att == null)
                return attribute.IsDefaultAttribute();

            return attribute.Match(att);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[]? attributes)
        {
            if (attributes == null || attributes.Length == 0)
                return ((ICustomTypeDescriptor)this).GetProperties();

            var list = new List<PropertyDescriptor>();
            foreach (var prop in _properties)
            {
                if (prop.Attributes.Count == 0)
                    continue;

                var cont = false;
                foreach (var att in attributes)
                {
                    if (!HasMatchingAttribute(prop, att))
                    {
                        cont = true;
                        break;
                    }
                }

                if (!cont)
                {
                    list.Add(prop);
                }
            }
            return new PropertyDescriptorCollection(list.ToArray());
        }

        protected virtual void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string? Validate() => ValidateMember(null);
        public string? ValidateMember(string? memberName = null) => ValidateMember(null, memberName);
        public virtual string? ValidateMember(CultureInfo? culture, string? memberName = null, string? separator = null)
        {
            culture ??= Thread.CurrentThread.CurrentUICulture;
            separator ??= Environment.NewLine;
            var list = new List<ValidationException>();
            ValidateMember(culture, list, memberName);
            if (list.Count == 0)
                return null;

            return string.Join(separator, list.Select(i => i.GetAllMessages(separator)));
        }

        public virtual void ValidateMember(CultureInfo culture, IList<ValidationException> list, string? memberName)
        {
            // by default, do nothing
        }
    }
}
