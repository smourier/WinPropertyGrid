using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private readonly ICommand _nullifyCommand;
        private readonly ICommand _copyCommand;
        private readonly ICommand _pasteCommand;
        private readonly Lazy<PropertyGridEnum?> _enum;

        public PropertyGridProperty(PropertyGridObject gridObject, Type type, string name)
        {
            ArgumentNullException.ThrowIfNull(gridObject);
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(name);
            GridObject = gridObject;
            Name = name;
            Type = type;
            _enum = new Lazy<PropertyGridEnum?>(GetEnum);
            _nullifyCommand = new BaseCommand(Nullify);
            _copyCommand = new BaseCommand(CopyToClipboard);
            _pasteCommand = new BaseCommand(PasteFromClipboard);
            Errors.CollectionChanged += OnErrorsChanged;
        }

        public PropertyGridObject GridObject { get; }
        public Type Type { get; }
        public string Name { get; }
        public virtual PropertyDescriptor? Descriptor { get; set; }
        public ExpandoObject DynamicProperties { get; } = new ExpandoObject();
        public ObservableCollection<PropertyGridPropertyError> Errors { get; } = new ObservableCollection<PropertyGridPropertyError>();
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

        public virtual string? FormattedValue
        {
            get
            {
                if (StringFormat != null)
                    return string.Format(StringFormat, Value);

                return Conversions.ChangeType<string>(Value);
            }
        }

        public string? ValueErrors => GetErrorsText(nameof(Value), Environment.NewLine);
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

        public virtual PropertyGridEnum? Enum => _enum.Value;

        protected virtual PropertyGridEnum? GetEnum() => GridObject.Grid.CreateEnum(this);
        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Value):
                    OnPropertyChanged(nameof(EnumerableCount));
                    OnPropertyChanged(nameof(IsDefaultValue));
                    OnPropertyChanged(nameof(FormattedValue));
                    break;

                case nameof(StringFormat):
                    OnPropertyChanged(nameof(FormattedValue));
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

        public virtual string? GetErrorsText(string? propertyName, string? separator = null) => string.Join(separator, DictionaryObjectGetErrors(propertyName).OfType<PropertyGridPropertyError>().Select(e => e.DisplayName)).Nullify();
        protected override IEnumerable DictionaryObjectGetErrors(string? propertyName)
        {
            if (propertyName == null)
                return Errors;

            return Errors.Where(e => e.PropertyName == propertyName);
        }

        private void OnErrorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var props = new HashSet<string>();
            if (e.Action != NotifyCollectionChangedAction.Reset)
            {
                if (e.NewItems != null)
                {
                    foreach (var name in e.NewItems.OfType<PropertyGridPropertyError>().Where(e => e.PropertyName != null))
                    {
                        props.Add(name.PropertyName!);
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (var name in e.OldItems.OfType<PropertyGridPropertyError>().Where(e => e.PropertyName != null))
                    {
                        props.Add(name.PropertyName!);
                    }
                }
            }

            foreach (var prop in props)
            {
                OnErrorsChanged(this, new DataErrorsChangedEventArgs(prop));
                GridObject.OnSelectedObjectPropertyErrorsChanged(this);
            }
            OnPropertyChanged(nameof(HasErrors));
            OnPropertyChanged(nameof(HasNoError));
            OnPropertyChanged(nameof(ValueErrors));
        }

        protected virtual void SetPropertyError(string text, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName != null)
            {
                var withProperty = Errors.FirstOrDefault(e => e.PropertyName == propertyName);
                if (withProperty != null)
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        Errors.Remove(withProperty);
                        return;
                    }

                    withProperty.Text = text;
                    return;
                }

                Errors.Add(new PropertyGridPropertyError { Text = text, PropertyName = propertyName });
                return;
            }

            if (string.IsNullOrEmpty(text))
                return;

            // don't add same message twice
            var existing = Errors.FirstOrDefault(e => e.Text == text && e.PropertyName == null);
            if (existing != null)
                return;

            Errors.Add(new PropertyGridPropertyError { Text = text });
        }

        protected virtual void ClearPropertyError([CallerMemberName] string? propertyName = null)
        {
            if (propertyName != null)
            {
                var existing = Errors.FirstOrDefault(e => e.PropertyName == propertyName);
                if (existing != null)
                {
                    Errors.Remove(existing);
                }
            }
        }

        public virtual bool SetValue(object? value, DictionaryObjectPropertySetOptions options)
        {
            if (!Conversions.TryChangeType(value, Type, out var changedValue))
            {
                var type = value != null ? value.GetType().FullName : "null";
                SetPropertyError($"Cannot convert value {value} of type '{type}' to type '{Type.FullName}'.", nameof(Value));
                return false;
            }

            if (Descriptor != null && !Descriptor.IsReadOnly)
            {
                try
                {
                    Descriptor.SetValue(GridObject.Data, changedValue);
                    changedValue = Descriptor.GetValue(GridObject.Data);
                }
                catch
                {
                    var type = value != null ? value.GetType().FullName : "null";
                    SetPropertyError($"Cannot set value {value} of type '{type}' to property '{Name}' of object '{this}'.", nameof(Value));
                    return false;
                }
            }

            ClearPropertyError(nameof(Value));
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
                        str = FormattedValue!;
                    }

                    if (str != null)
                    {
                        package.SetText(str);
                    }
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