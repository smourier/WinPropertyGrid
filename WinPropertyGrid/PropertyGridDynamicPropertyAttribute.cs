using System;

namespace WinPropertyGrid
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class PropertyGridDynamicPropertyAttribute : Attribute
    {
        public virtual object? Value { get; set; }
        public virtual string? Name { get; set; }
        public virtual Type Type { get; set; } = typeof(object);
        public virtual bool ConvertToType { get; set; } // default is false because many types are convertible, ex "#FF0000" => Brush is implicit
    }
}