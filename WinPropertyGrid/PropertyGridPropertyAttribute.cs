using System;

namespace WinPropertyGrid
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyGridPropertyAttribute : Attribute
    {
        public virtual bool Ignore { get; set; }
        public virtual bool IsEnum { get; set; }
        public virtual bool IsFlagsEnum { get; set; }
        public virtual int SortOrder { get; set; }
        public virtual Type? Type { get; set; }
        public virtual bool ForceReadWrite { get; set; }
        public virtual bool HasDefaultValue { get; set; }
        public virtual object? DefaultValue { get; set; }
        public virtual object? EditorDataTemplateResourceKey { get; set; }
    }
}