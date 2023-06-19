using System;
using System.Collections.Generic;

namespace WinPropertyGrid
{
    public class PropertyGridPropertyComparer : IComparer<PropertyGridProperty>
    {
        public static PropertyGridPropertyComparer Instance { get; } = new PropertyGridPropertyComparer();

        public virtual int Compare(PropertyGridProperty? x, PropertyGridProperty? y)
        {
            ArgumentNullException.ThrowIfNull(x);
            ArgumentNullException.ThrowIfNull(y);

            if (x.SortOrder != 0)
                return x.SortOrder.CompareTo(y.SortOrder);

            if (y.SortOrder != 0)
                return -y.SortOrder.CompareTo(0);

            return string.Compare(x.ActualDisplayName, y.ActualDisplayName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
