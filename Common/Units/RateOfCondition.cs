// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Runtime.InteropServices;

namespace Common.Units;

[StructLayout(LayoutKind.Explicit, Size = sizeof(double))]
public struct RateOfCondition : IUnitValue, IComparable<RateOfCondition>
{
    static RateOfCondition()
    {
        OilCondition.GlobalUnitChanged += OilCondition_GlobalUnitChanged;
    }

    public RateOfCondition(double value = 0, Unit? sourceUnit = null)
    {
        baseValue = Convert(value, sourceUnit ?? GlobalUnit, BaseUnit);
    }
    public RateOfCondition(OilCondition Copy)
    {
        baseValue = Copy.BaseValue;
    }

    public static event EventHandler<ValueChangedEventArgs>? GlobalUnitChanged;

    public override bool Equals(object? obj)
    {
        if (obj is RateOfCondition t)
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
            RateOfCondition t => CompareTo(t),
            double d => CompareTo(d),
            _ => Value.CompareTo(obj),
        };
    }
    public int CompareTo(RateOfCondition other)
    {
        return BaseValue.CompareTo(other.BaseValue);
    }
    public int CompareTo(double other)
    {
        return Value.CompareTo(other);
    }

    public double ValueAs(Unit? Unit)
    {
        return Convert(BaseValue, BaseUnit, Unit);
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
    public string ToString(string format, IFormatProvider formatProvider)
    {
        return ToString(format, formatProvider, null);
    }
    public string ToString(string format, IFormatProvider formatProvider, Unit? unit)
    {
        if (double.IsNaN(BaseValue))
            return string.Empty;
        if (format is null)
            format = string.Empty;
        unit ??= GlobalUnit;
        double val = Convert(BaseValue, BaseUnit, unit);

        int symbolState = 0;
        if (format.Contains("u"))
            symbolState = 1;
        if (format.Contains("U"))
            symbolState = 2;
        format = format.Replace("u", string.Empty).Replace("U", string.Empty);

        return val.ToString(format, formatProvider) +
               (symbolState == 1 ? unit?.Symbol : (symbolState == 2 ? unit?.Word : string.Empty));
    }

    public static RateOfCondition operator +(RateOfCondition val0, RateOfCondition val1)
    {
        return new RateOfCondition(val0.Value + val1.Value);
    }
    public static RateOfCondition operator -(RateOfCondition val0, RateOfCondition val1)
    {
        return new RateOfCondition(val0.Value - val1.Value);
    }
    public static RateOfCondition operator +(RateOfCondition val0, double val1)
    {
        return new RateOfCondition(val0.Value + val1);
    }
    public static RateOfCondition operator -(RateOfCondition val0, double val1)
    {
        return new RateOfCondition(val0.Value - val1);
    }
    public static RateOfCondition operator *(RateOfCondition val0, double val1)
    {
        return new RateOfCondition(val0.Value * val1);
    }
    public static RateOfCondition operator /(RateOfCondition val0, double val1)
    {
        return new RateOfCondition(val0.Value / val1);
    }
    public static RateOfCondition operator %(RateOfCondition val0, double val1)
    {
        return new RateOfCondition(val0.Value % val1);
    }

    public static bool operator ==(RateOfCondition val0, RateOfCondition val1)
    {
        return val0.BaseValue == val1.BaseValue; //-V3024
    }
    public static bool operator !=(RateOfCondition val0, RateOfCondition val1)
    {
        return val0.BaseValue != val1.BaseValue; //-V3024
    }
    public static bool operator <(RateOfCondition val0, RateOfCondition val1)
    {
        return val0.BaseValue < val1.BaseValue;
    }
    public static bool operator <=(RateOfCondition val0, RateOfCondition val1)
    {
        return val0.BaseValue <= val1.BaseValue;
    }
    public static bool operator >(RateOfCondition val0, RateOfCondition val1)
    {
        return val0.BaseValue > val1.BaseValue;
    }
    public static bool operator >=(RateOfCondition val0, RateOfCondition val1)
    {
        return val0.BaseValue >= val1.BaseValue;
    }

    public static implicit operator double(RateOfCondition val0)
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

        var value = SourceValue;
            
        // Convert to base value
        if (SourceUnit != BaseUnit)
            value = OilCondition.Convert(value + OilCondition.Convert(0, BaseUnit, SourceUnit), SourceUnit, BaseUnit);
            
        // Convert from base value
        if(TargetUnit != BaseUnit)
            value = OilCondition.Convert(value, BaseUnit, TargetUnit) - OilCondition.Convert(0, BaseUnit, TargetUnit);
            
        return value;
    }
    private static void OilCondition_GlobalUnitChanged(object? sender, ValueChangedEventArgs e)
    {
        calculateMinMax = true;
        GlobalUnitChanged?.Invoke(sender, e);
    }


    public static IEnumerable<Unit?> AvailableUnits { get => OilCondition.AvailableUnits; }

    private static readonly Unit? baseUnit;
    public static Unit? BaseUnit { get => OilCondition.BaseUnit; }
    public static Unit? GlobalUnit {
        get => OilCondition.GlobalUnit;
        set => OilCondition.GlobalUnit = value;
    }
    public static RateOfCondition MinValue {
        get {
            CalculateMinMax();
            return minValue;
        }
    }
    public static RateOfCondition MaxValue {
        get {
            CalculateMinMax();
            return maxValue;
        }
    }
    public static TimeSpan[] StandardTimePeriods {
        get => new TimeSpan[] {
            new TimeSpan(30, 0, 0, 0),
            new TimeSpan(7, 0, 0, 0),
            new TimeSpan(1, 0, 0, 0),
        };
    }

    private static void CalculateMinMax()
    {
        if (!calculateMinMax)
            return;

        var min = OilCondition.MinValue.Value - OilCondition.Convert(0.0, BaseUnit, GlobalUnit);
        var max = OilCondition.MaxValue.Value - OilCondition.Convert(0.0, BaseUnit, GlobalUnit);

        var magnitude = Math.Abs(Math.Abs(min) > Math.Abs(max) ? min : max);

        if (OilCondition.MinValue.BaseValue <= OilCondition.MaxValue.BaseValue)
        {
            minValue = new RateOfCondition(0);
            maxValue = new RateOfCondition(magnitude);
        }
        else
        {
            minValue = new RateOfCondition(-magnitude);
            maxValue = new RateOfCondition(0);
        }

        calculateMinMax = false;
    }

    private static bool calculateMinMax = true;
    private static RateOfCondition minValue;
    private static RateOfCondition maxValue;

    [FieldOffset(0)]
    private readonly double baseValue;
    public double BaseValue { get => baseValue; }
    public double Value { get => Convert(BaseValue, BaseUnit, GlobalUnit); }
}