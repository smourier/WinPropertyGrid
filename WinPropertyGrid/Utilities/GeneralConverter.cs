using System;
using Microsoft.UI.Xaml.Data;

namespace WinPropertyGrid.Utilities
{
    public class GeneralConverter : IValueConverter
    {
        public virtual object ConvertBack(object value, Type targetType, object parameter, string language) => Convert(value, targetType, parameter, language);
        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            if (language == null || !Conversions.TryChangeType<IFormatProvider>(language, out var provider))
                return Conversions.ChangeType(value, targetType)!;

            return Conversions.ChangeType(value, targetType, provider)!;
        }
    }
}