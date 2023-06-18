using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace WinPropertyGrid.Utilities
{
    public interface IDictionaryObject
    {
        IDictionary<string, DictionaryObjectProperty?> Properties { get; }

        T? GetPropertyValue<T>(T? defaultValue, [CallerMemberName] string? name = null);
        void SetPropertyValue(object? value, DictionaryObjectPropertySetOptions options, [CallerMemberName] string? name = null);
    }
}
