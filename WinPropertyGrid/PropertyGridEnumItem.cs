using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinPropertyGrid
{
    public class PropertyGridEnumItem : INotifyPropertyChanged
    {
        private bool _isChecked;
        private bool _isEnabled = true;
        internal ulong _value;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PropertyGridEnumItem(PropertyGridEnum @enum, string name, object? value)
        {
            ArgumentNullException.ThrowIfNull(@enum);
            ArgumentNullException.ThrowIfNull(name);
            Enum = @enum;
            Name = name;
            Value = value;
        }

        public PropertyGridEnum Enum { get; }
        public object? Value { get; }
        public string Name { get; }
        public bool IsNull { get; internal set; }
        public bool IsNotNull => !IsNull;

        public bool IsUnchecked { get => !IsChecked; set => IsChecked = !value; }
        public virtual bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value)
                    return;

                _isChecked = value;
                OnPropertyChanged();
            }
        }

        public bool IsDisabled { get => !IsEnabled; set => IsEnabled = !value; }
        public virtual bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                    return;

                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public override string ToString() => Name + " (" + Value + ") " + IsChecked;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => OnPropertyChanged(this, new PropertyChangedEventArgs(name));
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
    }
}
