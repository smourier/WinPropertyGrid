using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using WinPropertyGrid.Utilities;

namespace WinPropertyGrid
{
    public class PropertyGridProperty : DictionaryObject
    {
        public PropertyGridProperty(PropertyGridObject gridObject, Type type, string name)
        {
            ArgumentNullException.ThrowIfNull(gridObject);
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(name);
            GridObject = gridObject;
            Name = name;
            Type = type;
        }

        public PropertyGridObject GridObject { get; }
        public Type Type { get; }
        public string Name { get; }
        public virtual object? Value { get => DictionaryObjectGetPropertyValue<object>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual object? DefaultValue { get => DictionaryObjectGetPropertyValue<object>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual int SortOrder { get => DictionaryObjectGetPropertyValue<int>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual bool IsReadOnly { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool IsReadWrite { get => !IsReadOnly; set => IsReadOnly = !value; }
        public virtual bool IsError { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool IsNotError { get => !IsError; set => IsError = !value; }
        public virtual bool IsEnum { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool IsNotEnum { get => !IsEnum; set => IsEnum = !value; }
        public virtual bool IsFlagsEnum { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool IsNotFlagsEnum { get => !IsFlagsEnum; set => IsFlagsEnum = !value; }
        public virtual string? Category { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual string? DisplayName { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual string? Description { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual bool HasDefaultValue { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual bool IsDefaultValue => HasDefaultValue && GridObject.CompareForEquality(Value, DefaultValue);
        public virtual Type? CollectionItemPropertyType => IsCollection ? Conversions.GetElementType(Type) : null;
        public virtual bool IsCollectionItemValueType => CollectionItemPropertyType?.IsValueType == true;
        public virtual string ActualDisplayName => DisplayName.Nullify() ?? Name;

        // note: can eat performance, use with caution
        public virtual int CollectionCount
        {
            get
            {
                if (Value is IEnumerable enumerable)
                    return enumerable.Cast<object>().Count();

                return 0;
            }
        }

        public virtual bool IsCollection
        {
            get
            {
                if (Type == typeof(string))
                    return false;

                return typeof(IEnumerable).IsAssignableFrom(Type);
            }
        }

        public virtual bool? BooleanValue
        {
            get
            {
                Conversions.TryChangeType<bool?>(Value, out var text);
                return text;
            }
        }

        public virtual string? TextValue
        {
            get
            {
                Conversions.TryChangeType<string>(Value, out var text);
                return text;
            }
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Value):
                    OnPropertyChanged(nameof(TextValue));
                    OnPropertyChanged(nameof(BooleanValue));
                    OnPropertyChanged(nameof(CollectionCount));
                    OnPropertyChanged(nameof(IsDefaultValue));
                    break;

                case nameof(DefaultValue):
                    OnPropertyChanged(nameof(IsDefaultValue));
                    break;

                case nameof(HasDefaultValue):
                    OnPropertyChanged(nameof(IsDefaultValue));
                    break;

                case nameof(IsReadOnly):
                    OnPropertyChanged(nameof(IsReadWrite));
                    break;

                case nameof(IsEnum):
                    OnPropertyChanged(nameof(IsNotEnum));
                    break;

                case nameof(IsFlagsEnum):
                    OnPropertyChanged(nameof(IsNotFlagsEnum));
                    break;

                case nameof(IsError):
                    OnPropertyChanged(nameof(IsNotError));
                    break;
            }
        }

        public virtual bool SetValue(object? value, DictionaryObjectPropertySetOptions options)
        {
            try
            {
                return DictionaryObjectSetPropertyValue(value, options, nameof(Value));
            }
            catch (Exception ex)
            {
                if (Type == typeof(string))
                {
                    Value = ex.GetAllMessages();
                }
                IsError = true;
                return false;
            }
        }

        public override string ToString() => Name;
    }
}