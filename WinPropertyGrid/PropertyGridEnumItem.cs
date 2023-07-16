using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinPropertyGrid
{
    public class PropertyGridEnumItem : INotifyPropertyChanged
    {
        private bool _isChecked;

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
        public virtual bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value)
                    return;

                _isChecked = value;
            }
        }

        public override string ToString() => Name + " (" + Value + ") " + IsChecked;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => OnPropertyChanged(this, new PropertyChangedEventArgs(name));
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
    }
}
