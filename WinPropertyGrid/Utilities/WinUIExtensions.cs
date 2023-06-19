using System.Reflection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Windows.UI.Core;

namespace WinPropertyGrid.Utilities
{
    public static class WinUIExtensions
    {
        public static void SetCursor(this UIElement element, CoreCursorType cursorType) => element.SetCursor(InputCursor.CreateFromCoreCursor(new CoreCursor(cursorType, 0)));
        public static void SetCursor(this UIElement element, InputCursor? cursor)
        {
            if (element == null)
                return;

            // this is nuts we have to do this ...
            typeof(UIElement).InvokeMember("ProtectedCursor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, element, new object?[] { cursor });
        }
    }
}
