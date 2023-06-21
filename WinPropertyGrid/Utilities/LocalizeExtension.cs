using System;
using System.Globalization;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using WinPropertyGrid.Strings;

namespace WinPropertyGrid.Utilities
{
    [ContentProperty(Name = nameof(Key))]
    public class LocalizeExtension : MarkupExtension
    {
        public string Key { get; set; } = string.Empty;

        // from here we can query for IRootObjectProvider, IXamlTypeResolver, IProvideValueTarget, IUriContext
        // https://github.com/microsoft/microsoft-ui-xaml/issues/741
        protected override object? ProvideValue(IXamlServiceProvider serviceProvider)
        {
            if (Key == null)
                return null;

            var culture = CultureInfo.CurrentUICulture;

            // come up with .NET culture from WinRT/WinUI3/UWP mess
            if (!string.IsNullOrEmpty(Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride) &&
                Windows.Globalization.ApplicationLanguages.Languages.Contains(Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride) &&
                Conversions.TryChangeType<CultureInfo>(Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride, out var ci))
            {
                culture = ci!;
            }

            return ProvideValue(serviceProvider, culture);
        }

        public virtual string? ProvideValue(IXamlServiceProvider serviceProvider, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(culture);
            // local to this assembly
            if ((serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider)?.RootObject is PropertyGrid && Resources.ResourceManager.GetString(Key, culture) is string str)
                return str;

            return "?" + Key + "?";
        }
    }
}
