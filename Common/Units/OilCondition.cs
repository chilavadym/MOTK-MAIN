// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace Common.Units
{
    [JsonObject, JsonConverter(typeof(JsonConverter_OilCondition))]
    [StructLayout(LayoutKind.Explicit, Size = sizeof(double))]
    public struct OilCondition : IUnitValue, IComparable<OilCondition>, IFormattable
    {
        static OilCondition()
        {
            LossFactor = new Unit(Branding.StrUnitLfShort, Branding.StrUnitLfLong);
            TanDeltaNumber = new Unit(Branding.StrUnitTdnShort, Branding.StrUnitTdnLong);

            BaseUnit = LossFactor;

            Conversions = new Dictionary<Unit, (Func<double, double> ConvertTo, Func<double, double> ConvertFrom)>
            {
                [LossFactor] = (
                    v => v,
                    v => v
                ),
                [TanDeltaNumber] = (
                    v => 900.0 - (v * 20.0),
                    v => (900.0 - v) / 20.0
                ),
            };
        }

        public OilCondition(double Value = 0, Unit SourceUnit = null)
        {
            baseValue = Conversions[SourceUnit ?? GlobalUnit].ConvertFrom(Value);
        }
        public OilCondition(OilCondition Copy)
        {
            baseValue = Copy.BaseValue;
        }

        public static event EventHandler<ValueChangedEventArgs> GlobalUnitChanged;

        public override bool Equals(object obj)
        {
            if (obj is OilCondition t)
                return BaseValue.Equals(t.BaseValue);
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return BaseValue.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            return obj switch
            {
                OilCondition t => CompareTo(t),
                double d => CompareTo(d),
                _ => Value.CompareTo(obj),
            };
        }
        public int CompareTo(OilCondition other)
        {
            return BaseValue.CompareTo(other.BaseValue);
        }
        public int CompareTo(double other)
        {
            return Value.CompareTo(other);
        }

        public double ValueAs(Unit Unit)
        {
            return Conversions[Unit].ConvertTo(BaseValue);
        }
        public override string ToString()
        {
            return ToString("N1u");
        }
        public string ToString(string format)
        {
            return ToString(format, System.Globalization.CultureInfo.CurrentCulture, null);
        }
        public string ToString(string format, Unit unit)
        {
            return ToString(format, System.Globalization.CultureInfo.CurrentCulture, unit);
        }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString(format, formatProvider, null);
        }
        public string ToString(string format, IFormatProvider formatProvider, Unit unit)
        {
            if (double.IsNaN(BaseValue))
                return string.Empty;
            if (format is null)
                format = string.Empty;
            unit ??= GlobalUnit;
            double val = Conversions[unit].ConvertTo(BaseValue);

            int symbolState = 0;
            if (format.Contains("u"))
                symbolState = 1;
            if (format.Contains("U"))
                symbolState = 2;
            format = format.Replace("u", string.Empty).Replace("U", string.Empty);

            return val.ToString(format, formatProvider) +
                (symbolState == 1 ? unit.Symbol : (symbolState == 2 ? unit.Word : string.Empty));
        }

        public static OilCondition operator +(OilCondition val0, OilCondition val1)
        {
            return new OilCondition(val0.Value + val1.Value);
        }
        public static OilCondition operator -(OilCondition val0, OilCondition val1)
        {
            return new OilCondition(val0.Value - val1.Value);
        }
        public static OilCondition operator +(OilCondition val0, double val1)
        {
            return new OilCondition(val0.Value + val1);
        }
        public static OilCondition operator -(OilCondition val0, double val1)
        {
            return new OilCondition(val0.Value - val1);
        }
        public static OilCondition operator *(OilCondition val0, double val1)
        {
            return new OilCondition(val0.Value * val1);
        }
        public static OilCondition operator /(OilCondition val0, double val1)
        {
            return new OilCondition(val0.Value / val1);
        }
        public static OilCondition operator %(OilCondition val0, double val1)
        {
            return new OilCondition(val0.Value % val1);
        }

        public static bool operator ==(OilCondition val0, OilCondition val1)
        {
            return val0.BaseValue == val1.BaseValue; //-V3024
        }
        public static bool operator !=(OilCondition val0, OilCondition val1)
        {
            return val0.BaseValue != val1.BaseValue; //-V3024
        }
        public static bool operator <(OilCondition val0, OilCondition val1)
        {
            return val0.BaseValue < val1.BaseValue;
        }
        public static bool operator <=(OilCondition val0, OilCondition val1)
        {
            return val0.BaseValue <= val1.BaseValue;
        }
        public static bool operator >(OilCondition val0, OilCondition val1)
        {
            return val0.BaseValue > val1.BaseValue;
        }
        public static bool operator >=(OilCondition val0, OilCondition val1)
        {
            return val0.BaseValue >= val1.BaseValue;
        }

        public static implicit operator double(OilCondition val0)
        {
            return val0.Value;
        }
        public static double Convert(double SourceValue, Unit SourceUnit, Unit TargetUnit)
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
            = new Dictionary<Unit, (Func<double, double> ConvertTo, Func<double, double> ConvertFrom)>();

        public static Unit LossFactor { get; }
        public static Unit TanDeltaNumber { get; }

        public static Unit[] AvailableUnits { get => Conversions.Keys.Where(u => u.Word.Length > 0).ToArray(); }

        public static Unit BaseUnit { get; }

        private static Unit globalUnit;
        public static Unit GlobalUnit
        {
            get => globalUnit ?? BaseUnit;
            set
            {
                if (!(value is null) && !Conversions.ContainsKey(value))
                    throw new ArgumentException("Value must be a valid unit", nameof(GlobalUnit));

                var prev = GlobalUnit;
                globalUnit = value;
                if (!Equals(prev, GlobalUnit))
                    GlobalUnitChanged?.Invoke(GlobalUnit, new ValueChangedEventArgs(nameof(GlobalUnit), prev, GlobalUnit));
            }
        }
        public static OilCondition MinValue
        {
            get
            {
                return new OilCondition(
                    Convert(0, TanDeltaNumber, GlobalUnit),
                    GlobalUnit
                );
            }
        }
        public static OilCondition MaxValue
        {
            get
            {
                return new OilCondition(
                    Convert(1200, TanDeltaNumber, GlobalUnit),
                    GlobalUnit
                );
            }
        }

        [FieldOffset(0)]
        private readonly double baseValue;
        public double BaseValue { get => baseValue; }
        public double Value { get => Conversions[GlobalUnit].ConvertTo(BaseValue); }
    }

    public class JsonConverter_OilCondition : JsonConverter<OilCondition>
    {
        public override OilCondition ReadJson(JsonReader reader, Type objectType, OilCondition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    return new OilCondition(double.Parse(reader.Value.ToString()), OilCondition.BaseUnit);
                default:
                    return new OilCondition(double.NaN, OilCondition.BaseUnit);
            }
        }

        public override void WriteJson(JsonWriter writer, OilCondition value, JsonSerializer serializer)
        {
            writer.WriteValue(value.BaseValue);
        }
    }
}
