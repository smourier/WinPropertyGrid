using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using WinPropertyGrid.Utilities;

namespace WinPropertyGrid.SampleApp
{
    // declare this for example on a class
    // [PropertyGridDynamicProperty(Name = "NameForeground", Value = "#FF0000")]
    // and use it like this in PropertyGrid.xaml:
    // <ListView x:Name="NamesList">
    //    <ListView.ItemTemplate>
    //        <DataTemplate>
    //            <TextBlock Foreground="{Binding GridObject.DynamicProperties.NameForeground}" ... />
    //        </DataTemplate>
    //    </ListView.ItemTemplate>
    //</ListView>
    public class Customer : DictionaryObject
    {
        public Customer()
        {
            Id = Guid.NewGuid();
            NullableId = Guid.NewGuid();
            ListOfStrings = new List<string>
            {
                "string 1",
                "string 2"
            };

            TimeOnly = TimeOnly.FromDateTime(DateTime.Now);
            ArrayOfStrings = ListOfStrings.ToArray();
            CreationDateAndTime = DateTime.Now;
            Description = "press button to edit...";
            ByteArray1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            WebSite = "https://www.aelyo.com";
            WebSiteUri = new Uri(WebSite);
            Status = Status.Valid;
            Addresses = new ObservableCollection<Address> { new Address { Line1 = "2018 156th Avenue NE", City = "Bellevue", State = "WA", ZipCode = 98007, Country = "USA" } };
            DaysOfWeek = DaysOfWeek.WeekDays;
            PercentageOfSatisfaction = 50;
            PreferredColorName = "DodgerBlue";
            SampleNullableBooleanDropDownList = false;
            SampleBooleanDropDownList = true;
            MultiEnumString = "First, Second";
            SubObject = Address.Parse("1600 Amphitheatre Pkwy, Mountain View, CA 94043, USA");
        }

        [DisplayName("Guid (see menu on right-click)")]
        public Guid Id { get => DictionaryObjectGetPropertyValue<Guid>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Nullable Guid (see menu on right-click)")]
        public Guid? NullableId { get => DictionaryObjectGetPropertyValue<Guid?>(); set { if (DictionaryObjectSetPropertyValue(value)) OnPropertyChanged(nameof(ReadOnlyNullableId)); } }

        [DisplayName("Nullable Guid (read only)")]
        [ReadOnly(true)]
        public Guid? ReadOnlyNullableId { get => NullableId; set => NullableId = value; }

        //[ReadOnly(true)]
        [Category("Dates and Times")]
        public DateTime CreationDateAndTime { get => DictionaryObjectGetPropertyValue<DateTime>(); set => DictionaryObjectSetPropertyValue(value); }

        public DateOnly DateOnly { get => DictionaryObjectGetPropertyValue<DateOnly>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Sub Object (Address)")]
        public Address? SubObject
        {
            get => DictionaryObjectGetPropertyValue<Address>();
            set
            {
                // because it's a sub object we want to update the property grid
                // when inner properties change
                var so = SubObject;
                if (so != null)
                {
                    so.PropertyChanged -= OnSubObjectPropertyChanged;
                }

                if (DictionaryObjectSetPropertyValue(value))
                {
                    so = SubObject;
                    if (so != null)
                    {
                        so.PropertyChanged += OnSubObjectPropertyChanged;
                    }

                    // these two properties are coupled
                    OnPropertyChanged(nameof(SubObjectAsObject));
                }
            }
        }

        private void OnSubObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SubObject));
            OnPropertyChanged(nameof(SubObjectAsObject));
        }

        [DisplayName("Sub Object (Address as Object)")]
        public Address? SubObjectAsObject { get => SubObject; set => SubObject = value; }

        public string? FirstName { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public string? LastName { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [Category("Dates and Times")]
        public DateTime DateOfBirth { get => DictionaryObjectGetPropertyValue<DateTime>(); set => DictionaryObjectSetPropertyValue(value); }

        [Category("Enums")]
        public Gender Gender { get => DictionaryObjectGetPropertyValue<Gender>(); set => DictionaryObjectSetPropertyValue(value); }

        [Category("Enums")]
        public Status Status
        {
            get => DictionaryObjectGetPropertyValue<Status>();
            set
            {
                if (DictionaryObjectSetPropertyValue(value))
                {
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(StatusColorString));
                }
            }
        }

        [DisplayName("Status (colored enum)")]
        [ReadOnly(true)]
        [Category("Enums")]
        public Status StatusColor { get => Status; set => Status = value; }

        [DisplayName("Status (enum as string list)")]
        [Category("Enums")]
        public string StatusColorString { get => Status.ToString(); set => Status = (Status)Enum.Parse(typeof(Status), value); }

        [Category("Enums")]
        public string? MultiEnumString { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [Category("Enums")]
        public string? MultiEnumStringWithDisplay { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [Category("Dates and Times")]
        [Description("This is the timespan tooltip")]
        public TimeSpan TimeSpan { get => DictionaryObjectGetPropertyValue<TimeSpan>(); set { if (DictionaryObjectSetPropertyValue(value)) OnPropertyChanged(nameof(TimeOnly)); } }

        [Category("Dates and Times")]
        public TimeOnly TimeOnly { get => TimeOnly.FromTimeSpan(TimeSpan); set => TimeSpan = value.ToTimeSpan(); }

        [Browsable(false)]
        public string? NotBrowsable { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Description (multi-line)")]
        public string? Description { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [ReadOnly(true)]
        [DisplayName("Byte Array (hex format)")]
        public byte[]? ByteArray1 { get => DictionaryObjectGetPropertyValue<byte[]>(); set => DictionaryObjectSetPropertyValue(value); }

        [ReadOnly(true)]
        [DisplayName("Byte Array (press button for hex dump)")]
        public byte[]? ByteArray2 { get => ByteArray1; set => ByteArray1 = value; }

        [DisplayName("Web Site (custom sort order)")]
        public string? WebSite { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Web Site (Uri)")]
        public Uri? WebSiteUri { get => DictionaryObjectGetPropertyValue<Uri>(); set => DictionaryObjectSetPropertyValue(value); }

        [Category("Collections")]
        [Description("This is the description of ArrayOfStrings")]
        public string[]? ArrayOfStrings { get => DictionaryObjectGetPropertyValue<string[]>(); set => DictionaryObjectSetPropertyValue(value); }

        [Category("Collections")]
        public List<string>? ListOfStrings { get => DictionaryObjectGetPropertyValue<List<string>>(); set => DictionaryObjectSetPropertyValue(value); }

        // declare this for example on a property
        // [PropertyGridDynamicProperty(Name = "NameForeground", Value = "#FF0000")]
        // and use it like this in PropertyGrid.xaml:
        // <ListView x:Name="NamesList">
        //    <ListView.ItemTemplate>
        //        <DataTemplate>
        //            <TextBlock Foreground="{Binding DynamicProperties.NameForeground}" ... />
        //        </DataTemplate>
        //    </ListView.ItemTemplate>
        //</ListView>
        [DisplayName("Addresses (custom editor)")]
        [Category("Collections")]
        [PropertyGridDynamicProperty(Name = "NameForeground", Value = "#FF0000")]
        public ObservableCollection<Address>? Addresses { get => DictionaryObjectGetPropertyValue<ObservableCollection<Address>>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Days Of Week (multi-valued enum)")]
        [Category("Enums")]
        public DaysOfWeek DaysOfWeek { get => DictionaryObjectGetPropertyValue<DaysOfWeek>(); set => DictionaryObjectSetPropertyValue(value); }

        [PropertyGridProperty(EditorDataTemplateResourceKey = "PercentEditor")]
        [DisplayName("Percentage Of Satisfaction (int)")]
        public int PercentageOfSatisfactionInt { get => (int)PercentageOfSatisfaction; set => PercentageOfSatisfaction = value; }

        [PropertyGridProperty(EditorDataTemplateResourceKey = "PercentEditor")]
        [DisplayName("Percentage Of Satisfaction (double)")]
        public double PercentageOfSatisfaction
        {
            get => DictionaryObjectGetPropertyValue<double>();
            set
            {
                if (DictionaryObjectSetPropertyValue(value))
                {
                    OnPropertyChanged(nameof(PercentageOfSatisfactionInt));
                }
            }
        }

        [DisplayName("Preferred Color Name (custom editor)")]
        public string? PreferredColorName { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Point (auto type converter)")]
        public Point Point { get => DictionaryObjectGetPropertyValue<Point>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Nullable Int32 (supports empty string)")]
        public int? NullableInt32 { get => DictionaryObjectGetPropertyValue<int?>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Boolean (Checkbox)")]
        [Category("Booleans")]
        public bool SampleBoolean
        {
            get => DictionaryObjectGetPropertyValue<bool>();
            set
            {
                if (DictionaryObjectSetPropertyValue(value))
                {
                    OnPropertyChanged(nameof(SampleReadOnlyBoolean));
                }
            }
        }

        [DisplayName("Boolean ReadOnly (Checkbox)")]
        [Category("Booleans")]
        [ReadOnly(true)]
        public bool SampleReadOnlyBoolean { get => SampleBoolean; set => SampleBoolean = value; }

        [DisplayName("Boolean (Checkbox three states)")]
        [Category("Booleans")]
        public bool? SampleNullableBoolean { get => DictionaryObjectGetPropertyValue<bool?>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Boolean (DropDownList)")]
        [Category("Booleans")]
        public bool SampleBooleanDropDownList { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Boolean (DropDownList 3 states)")]
        [Category("Booleans")]
        public bool? SampleNullableBooleanDropDownList { get => DictionaryObjectGetPropertyValue<bool?>(); set => DictionaryObjectSetPropertyValue(value); }

        public double DoubleValue { get => DictionaryObjectGetPropertyValue<double>(); set => DictionaryObjectSetPropertyValue(value); }

        protected override IEnumerable DictionaryObjectGetErrors(string? propertyName)
        {
            if (propertyName == null || propertyName == nameof(DoubleValue))
            {
                if (DoubleValue < 10)
                {
                    yield return nameof(DoubleValue) + " must be >= 10";
                }
            }
        }
    }

    [TypeConverter(typeof(AddressConverter))]
    public class Address : DictionaryObject
    {
        public string? Line1 { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public string? Line2 { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public int? ZipCode { get => DictionaryObjectGetPropertyValue<int?>(); set => DictionaryObjectSetPropertyValue(value); }
        public string? City { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public string? State { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }
        public string? Country { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        // poor man's one line comma separated USA postal address parser
        public static Address Parse(string text)
        {
            var address = new Address();
            if (text != null)
            {
                var split = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 0)
                {
                    var zip = 0;
                    var index = -1;
                    string? state = null;
                    for (var i = 0; i < split.Length; i++)
                    {
                        if (TryFindStateZip(split[i], out state, out zip))
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index < 0)
                    {
                        address.DistributeOverProperties(split, 0, int.MaxValue, nameof(Line1), nameof(Line2), nameof(City), nameof(Country));
                    }
                    else
                    {
                        address.ZipCode = zip;
                        address.State = state;
                        address.DistributeOverProperties(split, 0, index, nameof(Line1), nameof(Line2), nameof(City));
                        if (string.IsNullOrWhiteSpace(address.City) && address.Line2 != null)
                        {
                            address.City = address.Line2;
                            address.Line2 = null;
                        }
                        address.DistributeOverProperties(split, index + 1, int.MaxValue, nameof(Country));
                    }
                }
            }
            return address;
        }

        private static bool TryFindStateZip(string text, out string? state, out int zip)
        {
            state = null;
            var zipText = text;
            var pos = text.LastIndexOfAny(new[] { ' ' });
            if (pos >= 0)
            {
                zipText = text.Substring(pos + 1).Trim();
            }

            if (!int.TryParse(zipText, out zip) || zip <= 0)
                return false;

            state = text.Substring(0, pos).Trim();
            return true;
        }

        private void DistributeOverProperties(string[] split, int offset, int max, params string[] properties)
        {
            for (var i = 0; i < properties.Length; i++)
            {
                if ((offset + i) >= split.Length || (offset + i) >= max)
                    return;

                var s = split[offset + i].Trim();
                if (s.Length == 0)
                    continue;

                DictionaryObjectSetPropertyValue((object)s, properties[i]);
            }
        }

        public override string ToString()
        {
            const string sep = ", ";
            var sb = new StringBuilder();
            AppendJoin(sb, Line1, string.Empty);
            AppendJoin(sb, Line2, sep);
            AppendJoin(sb, City, sep);
            AppendJoin(sb, State, sep);
            if (ZipCode.HasValue)
            {
                AppendJoin(sb, ZipCode.Value.ToString(), " ");
            }
            AppendJoin(sb, Country, sep);
            return sb.ToString();
        }

        private static void AppendJoin(StringBuilder sb, string? value, string sep)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            var s = sb.ToString();
            if (!s.EndsWith(" ") && !s.EndsWith(",") && !s.EndsWith(Environment.NewLine))
            {
                sb.Append(sep);
            }
            sb.Append(value);
        }
    }

    [Flags]
    public enum DaysOfWeek
    {
        NoDay = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64,
        WeekDays = Monday | Tuesday | Wednesday | Thursday | Friday
    }

    public enum Gender
    {
        Male,
        Female
    }

    public enum Status
    {
        Unknown,
        Invalid,
        Valid
    }

    public class AddressConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
        {
            if (value is string s)
                return Address.Parse(s);

            return base.ConvertFrom(context, culture, value!);
        }
    }

    public class PointConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
        {
            if (value is string s)
            {
                var v = s.Split(new[] { ';' });
                return new Point(int.Parse(v[0]), int.Parse(v[1]));
            }

            return base.ConvertFrom(context, culture, value!);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string))
                return ((Point)value!).X + ";" + ((Point)value!).Y;

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    [TypeConverter(typeof(PointConverter))]
    public struct Point
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Point(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }
    }
}
