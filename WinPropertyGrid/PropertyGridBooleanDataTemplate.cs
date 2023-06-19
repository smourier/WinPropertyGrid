namespace WinPropertyGrid
{
    public class PropertyGridBooleanDataTemplate : PropertyGridDataTemplate
    {
        public override bool Matches(PropertyGridProperty property)
        {
            return property.Type == typeof(bool) || property.Type == typeof(bool?);
        }
    }
}
