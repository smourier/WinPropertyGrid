using System;

namespace WinPropertyGrid.Utilities
{
    public class UniversalConverterCase
    {
        private object? _convertedValue;
        private object? _value;

        public virtual object? MinimumValue { get; set; }
        public virtual object? MaximumValue { get; set; }
        public virtual UniversalConverterOptions Options { get; set; }
        public virtual UniversalConverterOperator Operator { get; set; }
        public virtual StringComparison StringComparison { get; set; } = StringComparison.CurrentCultureIgnoreCase;
        public virtual bool Reverse { get; set; }
        public bool HasConvertedValue { get; private set; }
        public bool HasValue { get; private set; }

        public virtual object? Value
        {
            get => _value;
            set
            {
                _value = value;
                HasValue = true;
            }
        }

        public virtual object? ConvertedValue
        {
            get => _convertedValue;
            set
            {
                _convertedValue = value;
                HasConvertedValue = true;
            }
        }

        public virtual bool Matches(object? value, object? parameter, IFormatProvider? provider)
        {
            var input = new UniversalConverterInput
            {
                MaximumValue = MaximumValue,
                MinimumValue = MinimumValue,
                Operator = Operator,
                Options = Options,
                Value = Value,
                ValueToCompare = value,
                Reverse = Reverse,
                StringComparison = StringComparison,
                ConverterParameter = parameter
            };
            return input.Matches(provider);
        }

        public override string ToString() => Operator.ToString();
    }
}
