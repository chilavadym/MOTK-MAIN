// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace Common.Units
{
    [JsonObject, JsonConverter(typeof(JsonConverter_Temperature))]
    [StructLayout(LayoutKind.Explicit, Size = sizeof(double))]
    public struct Temperature : IUnitValue, IComparable<Temperature>, IFormattable
    {
        static Temperature()
        {
            Celsius = new Unit(symbol: "°C", word: "Celsius");
            Fahrenheit = new Unit(symbol: "°F", word: "Fahrenheit");

            BaseUnit = Celsius;

            Conversions = new Dictionary<Unit, (Func<double, double> ConvertTo, Func<double, double> ConvertFrom)>
            {
                [Celsius] = (
                    v => v,
                    v => v
                ),
                [Fahrenheit] = (
                    v => (v * 1.8) + 32,
                    v => (v - 32) / 1.8
                ),
            };
        }
        public Temperature(double value = 0, Unit? sourceUnit = null)
        {
            if (GlobalUnit != null) baseValue = Conversions[sourceUnit ?? GlobalUnit].ConvertFrom(value);
        }
        public Temperature(Temperature copy)
        {
            baseValue = copy.BaseValue;
        }

        public static event EventHandler<ValueChangedEventArgs>? GlobalUnitChanged;

        public override bool Equals(object? obj)
        {
            if (obj is Temperature t)
                return BaseValue.Equals(t.BaseValue);
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return BaseValue.GetHashCode();
        }

        public int CompareTo(object? obj)
        {
            return obj switch
            {
                Temperature t => CompareTo(t),
                double d => CompareTo(d),
                _ => Value.CompareTo(obj),
            };
        }
        public int CompareTo(Temperature other)
        {
            return BaseValue.CompareTo(other.BaseValue);
        }
        public int CompareTo(double other)
        {
            return Value.CompareTo(other);
        }

        public double ValueAs(Unit? unit)
        {
            if (unit != null) return Conversions[unit].ConvertTo(BaseValue);

            return 0.0;
        }
        public override string ToString()
        {
            return ToString("N1u");
        }
        public string ToString(string format)
        {
            return ToString(format, System.Globalization.CultureInfo.CurrentCulture, null);
        }
        public string ToString(string format, Unit? unit)
        {
            return ToString(format, System.Globalization.CultureInfo.CurrentCulture, unit);
        }
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if ((format != null) && (formatProvider != null))
            {
                return ToString(format, formatProvider, null);
            }

            return string.Empty;
        }
        public string ToString(string format, IFormatProvider formatProvider, Unit? unit)
        {
            if (double.IsNaN(BaseValue))
                return string.Empty;
            if (format is null)
                format = string.Empty;

            unit ??= GlobalUnit;
            if (unit != null)
            {
                double val = Conversions[unit].ConvertTo(BaseValue);

                int symbolState = 0;
                if (format.Contains("u"))
                    symbolState = 1;
                if (format.Contains("U"))
                    symbolState = 2;
                format = format.Replace("u", string.Empty).Replace("U", string.Empty);

                return val.ToString(format, formatProvider) +
                       (symbolState == 1 ? unit?.Symbol : (symbolState == 2 ? unit?.Word : string.Empty));
            }

            return string.Empty;
        }

        public static Temperature operator +(Temperature val0, Temperature val1)
        {
            return new Temperature(val0.Value + val1.Value);
        }
        public static Temperature operator -(Temperature val0, Temperature val1)
        {
            return new Temperature(val0.Value - val1.Value);
        }
        public static Temperature operator +(Temperature val0, double val1)
        {
            return new Temperature(val0.Value + val1);
        }
        public static Temperature operator -(Temperature val0, double val1)
        {
            return new Temperature(val0.Value - val1);
        }
        public static Temperature operator *(Temperature val0, double val1)
        {
            return new Temperature(val0.Value * val1);
        }
        public static Temperature operator /(Temperature val0, double val1)
        {
            return new Temperature(val0.Value / val1);
        }
        public static Temperature operator %(Temperature val0, double val1)
        {
            return new Temperature(val0.Value % val1);
        }

        public static bool operator ==(Temperature val0, Temperature val1)
        {
            return val0.BaseValue == val1.BaseValue; //-V3024
        }
        public static bool operator !=(Temperature val0, Temperature val1)
        {
            return val0.BaseValue != val1.BaseValue; //-V3024
        }
        public static bool operator <(Temperature val0, Temperature val1)
        {
            return val0.BaseValue < val1.BaseValue;
        }
        public static bool operator <=(Temperature val0, Temperature val1)
        {
            return val0.BaseValue <= val1.BaseValue;
        }
        public static bool operator >(Temperature val0, Temperature val1)
        {
            return val0.BaseValue > val1.BaseValue;
        }
        public static bool operator >=(Temperature val0, Temperature val1)
        {
            return val0.BaseValue >= val1.BaseValue;
        }

        public static implicit operator double(Temperature val0)
        {
            return val0.Value;
        }
        public static double Convert(double SourceValue, Unit? SourceUnit, Unit? TargetUnit)
        {
            if (SourceUnit is null)
                throw new ArgumentNullException(nameof(SourceUnit));
            if (TargetUnit is null)
                throw new ArgumentNullException(nameof(TargetUnit));

            if (!AvailableUnits.Any(u => u == SourceUnit))
                throw new ArgumentException("No conversion found for the provided unit.", nameof(SourceUnit));
            if (!AvailableUnits.Any(u => u == TargetUnit))
                throw new ArgumentException("No conversion found for the provided unit.", nameof(TargetUnit));

            if (SourceUnit == TargetUnit)
                return SourceValue;

            return Conversions[TargetUnit].ConvertTo(Conversions[SourceUnit].ConvertFrom(SourceValue));
        }

        private static Dictionary<Unit, (Func<double, double> ConvertTo, Func<double, double> ConvertFrom)> Conversions { get; }

        public static Unit? Celsius { get; }
        public static Unit Fahrenheit { get; }
        public static Unit?[] AvailableUnits { get => Conversions.Keys.Where(u => u?.Word != null && u.Word.Length > 0).ToArray(); }

        public static Unit? BaseUnit { get; }

        private static Unit? globalUnit;
        public static Unit? GlobalUnit {
            get => globalUnit ?? BaseUnit;
            set {
                if (!(value is null) && !Conversions.ContainsKey(value))
                    throw new ArgumentException("Value must be a valid unit", nameof(GlobalUnit));

                var prev = GlobalUnit;
                globalUnit = value;
                if (!Equals(prev, GlobalUnit))
                    GlobalUnitChanged?.Invoke(GlobalUnit, new ValueChangedEventArgs(nameof(GlobalUnit), prev, GlobalUnit));
            }
        }
        public static Temperature MinValue { get => new Temperature(-20, BaseUnit); }
        public static Temperature MaxValue { get => new Temperature(120, BaseUnit); }

        [FieldOffset(0)]
        private readonly double baseValue;
        public double BaseValue { get => baseValue; }
        public double Value
        {
            get
            {
                if (GlobalUnit != null) return Conversions[GlobalUnit].ConvertTo(BaseValue);

                return 0.0;
            }
        }
    }

    public class JsonConverter_Temperature : JsonConverter<Temperature>
    {
        public override Temperature ReadJson(JsonReader reader, Type objectType, Temperature existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    return new Temperature(double.Parse(reader.Value?.ToString() ?? throw new InvalidOperationException()), Temperature.BaseUnit);
                default:
                    return new Temperature(double.NaN, Temperature.BaseUnit);
            }
        }

        public override void WriteJson(JsonWriter writer, Temperature value, JsonSerializer serializer)
        {
            writer.WriteValue(value.BaseValue);
        }
    }
}
