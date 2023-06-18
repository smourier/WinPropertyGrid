using System;

namespace WinPropertyGrid
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyGridPropertyAttribute : Attribute
    {
        public virtual string[]? EnumNames { get; set; }
        public virtual object[]? EnumValues { get; set; }
        public virtual bool IsEnum { get; set; }
        public virtual bool IsFlagsEnum { get; set; }
        public virtual int EnumMaxPower { get; set; }
        public virtual bool CollectionEditorHasOnlyOneColumn { get; set; }
        public virtual int SortOrder { get; set; }
        public virtual string? EditorDataTemplatePropertyPath { get; set; }
        public virtual string? EditorDataTemplateSelectorPropertyPath { get; set; }
        public virtual Type? EditorType { get; set; }
        public virtual string? EditorResourceKey { get; set; }
        public virtual object? EditorDataTemplateResourceKey { get; set; }
        public virtual Type? Type { get; set; }
        public virtual bool ForceReadWrite { get; set; }
        public virtual bool HasDefaultValue { get; set; }
        public virtual bool ForcePropertyChanged { get; set; }
        public virtual object? DefaultValue { get; set; }
        public virtual string? EnumSeparator { get; set; } = ", ";
    }
}