using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using WinPropertyGrid.Utilities;

namespace WinPropertyGrid
{
    public class PropertyGridProperty : DictionaryObject
    {
        public readonly ICommand _nullifyCommand;
        public readonly ICommand _copyCommand;
        public readonly ICommand _pasteCommand;

        public PropertyGridProperty(PropertyGridObject gridObject, Type type, string name)
        {
            ArgumentNullException.ThrowIfNull(gridObject);
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(name);
            GridObject = gridObject;
            Name = name;
            Type = type;
            _nullifyCommand = new BaseCommand(Nullify);
            _copyCommand = new BaseCommand(CopyToClipboard);
            _pasteCommand = new BaseCommand(PasteFromClipboard);
        }

        public PropertyGridObject GridObject { get; }
        public Type Type { get; }
        public string Name { get; }
        public virtual PropertyDescriptor? Descriptor { get; set; }
        public ExpandoObject DynamicProperties { get; } = new ExpandoObject();
        public ICommand NullifyCommand() => _nullifyCommand;
        public ICommand CopyCommand() => _copyCommand;
        public ICommand PasteCommand() => _pasteCommand;

        public virtual object? DefaultValue { get => DictionaryObjectGetPropertyValue<object>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual int SortOrder { get => DictionaryObjectGetPropertyValue<int>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual bool IsReadOnly { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool IsReadWrite { get => !IsReadOnly; set => IsReadOnly = !value; }
        public virtual bool IsEnum { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool IsNotEnum { get => !IsEnum; set => IsEnum = !value; }
        public virtual bool IsFlagsEnum { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public bool IsNotFlagsEnum { get => !IsFlagsEnum; set => IsFlagsEnum = !value; }
        public virtual string? Category { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual string? DisplayName { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual string? Description { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual bool HasDefaultValue { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
        public virtual string? StringFormat { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        public virtual bool IsNullableType => Type.IsNullable();
        public virtual bool IsDefaultValue => HasDefaultValue && GridObject.CompareForEquality(Value, DefaultValue);
        public virtual Type? EnumerableItemPropertyType => IsEnumerable ? Conversions.GetElementType(Type) : null;
        public virtual string ActualDisplayName => DisplayName.Nullify() ?? Name;
        public virtual string ActualDescription => Description.Nullify() ?? ActualDisplayName;

        public virtual string FormattedValue
        {
            get
            {
                if (StringFormat != null)
                    return string.Format(FormattedValue, Value);

                return string.Format("{0}", Value);
            }
        }

        public virtual object? Value { get => DictionaryObjectGetPropertyValue<object>(); set => SetValue(value, DictionaryObjectPropertySetOptions.None); }

        // note: can eat performance, use with caution
        public virtual int EnumerableCount
        {
            get
            {
                if (Value is IEnumerable enumerable)
                    return enumerable.Cast<object>().Count();

                return 0;
            }
        }

        public virtual bool IsEnumerable
        {
            get
            {
                if (Type == typeof(string))
                    return false;

                return typeof(IEnumerable).IsAssignableFrom(Type);
            }
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Value):
                    OnPropertyChanged(nameof(EnumerableCount));
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
            }
            base.OnPropertyChanged(sender, e);
        }

        public virtual bool SetValue(object? value, DictionaryObjectPropertySetOptions options)
        {
            if (!Conversions.TryChangeType(value, Type, out var changedValue))
            {
                var type = value != null ? value.GetType().FullName : "null";
                throw new ArgumentException($"Cannot convert value {value} of type '{type}' to type '{Type.FullName}'.");
            }

            if (Descriptor != null && !Descriptor.IsReadOnly)
            {
                try
                {
                    Descriptor.SetValue(GridObject.Data, changedValue);
                    changedValue = Descriptor.GetValue(GridObject.Data);
                }
                catch (Exception e)
                {
                    var type = value != null ? value.GetType().FullName : "null";
                    throw new ArgumentException($"Cannot set value {value} of type '{type}' to property '{Name}' of object '{this}'.", e);
                }
            }
            return DictionaryObjectSetPropertyValue(changedValue, options, nameof(Value));
        }

        public virtual void Nullify(object? parameter)
        {
            if (IsNullableType)
            {
                Value = null;
            }
        }

        public virtual void CopyToClipboard(object? parameter)
        {
            var value = Value;
            if (value == null)
                return;

            var package = new DataPackage();
            if (parameter is string formatId)
            {
                package.SetData(formatId, value);
            }
            else
            {
                if (value is Uri uri)
                {
                    package.SetUri(uri);
                }
                else if (value is RandomAccessStreamReference bitmap)
                {
                    package.SetBitmap(bitmap);
                }
                else if (value is IEnumerable<IStorageItem> items)
                {
                    package.SetStorageItems(items);
                }
                else
                {
                    if (value is not string str)
                    {
                        str = FormattedValue;
                    }
                    package.SetText(str);
                }
            }

            Clipboard.SetContent(package);
        }

        public virtual void PasteFromClipboard(object? parameter)
        {
            var content = Clipboard.GetContent();
            if (parameter is string formatId && content.Contains(formatId))
            {
                Task.Run(async () =>
                {
                    var value = await content.GetDataAsync(formatId);
                    if (Conversions.TryChangeType(value, Type, out var convertedValue))
                    {
                        Value = convertedValue;
                    }
                });
                return;
            }

            if (typeof(RandomAccessStreamReference).IsAssignableFrom(Type))
            {
                Task.Run(async () =>
                {
                    Value = await content.GetBitmapAsync();
                });
                return;
            }

            if (typeof(IEnumerable<IStorageItem>).IsAssignableFrom(Type))
            {
                Task.Run(async () =>
                {
                    Value = await content.GetStorageItemsAsync();
                });
                return;
            }

            if (typeof(Uri).IsAssignableFrom(Type))
            {
                Task.Run(async () =>
                {
                    Value = await content.GetUriAsync();
                });
                return;
            }

            Task.Run(async () =>
            {
                var value = await content.GetTextAsync();
                if (Conversions.TryChangeType(value, Type, out var convertedValue))
                {
                    Value = convertedValue;
                }
            });
        }

        public override string ToString() => Name;
    }
}