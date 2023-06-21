namespace WinPropertyGrid
{
    public class PropertyGridPropertyError
    {
        public string? PropertyName { get; set; }
        public virtual string? DisplayName => Text;
        public string? Text { get; set; }

        public override string ToString()
        {
            if (PropertyName != null)
                return PropertyName + ": " + Text;

            return Text ?? string.Empty;
        }
    }
}