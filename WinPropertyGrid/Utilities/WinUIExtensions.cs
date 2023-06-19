using System.Collections.Generic;
using System.Reflection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
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
            // come on WinUI3 guys, learn some Windows SDK
            typeof(UIElement).InvokeMember("ProtectedCursor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, element, new object?[] { cursor });
        }

        public static IEnumerable<DependencyObject> EnumerateChildren(this DependencyObject obj, bool recursive = false)
        {
            if (obj == null)
                yield break;

            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                yield return child;
                if (recursive)
                {
                    foreach (var grandChild in EnumerateChildren(child, recursive))
                    {
                        yield return grandChild;
                    }
                }
            }
        }

        public static IEnumerable<DependencyObject> EnumerateParents(this DependencyObject obj)
        {
            if (obj == null)
                yield break;

            var parent = VisualTreeHelper.GetParent(obj);
            if (parent == null)
                yield break;

            yield return parent;
            foreach (var grandParent in EnumerateParents(parent))
            {
                yield return grandParent;
            }
        }
    }
}
