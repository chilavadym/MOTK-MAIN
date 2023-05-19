// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common.Units;
using System.Runtime.InteropServices;

namespace Serial;

[StructLayout(LayoutKind.Sequential)]
public struct DisplayLog
{
    public enum EStatus : ushort
    {

    }

    public uint Address;
    public DateTime DateTime;
    public Temperature OilTemperature;
    public Temperature AmbTemperature;
    public OilCondition OilCondition;
    public TimeSpan RocTimePeriod;
    public OilCondition RocValue;
    public EStatus Status;

    public override string ToString()
    {
        if (IsEmpty)
        {
            return "Empty";
        }

        return $"{DateTime:yyyy-MM-dd HH:mm}, {OilTemperature:0.0u}, {AmbTemperature:0.0u}, {OilCondition:0.0u}, {RocValue}/{RocTimePeriod}, 0x{((ushort)Status):X4}";
    }

    public DisplayLog(DisplayLog copy) : this(
        copy.Address,
        copy.DateTime,
        copy.OilTemperature,
        copy.AmbTemperature,
        copy.OilCondition,
        copy.RocTimePeriod,
        copy.RocValue,
        copy.Status
    ) {
        // Nothing to do
    }

    public DisplayLog(
        uint address,
        DateTime dateTime,
        Temperature oilTemperature,
        Temperature ambTemperature,
        OilCondition oilCondition,
        TimeSpan rocTimePeriod,
        OilCondition rocValue,
        EStatus status
    ) {
        Address = address;
        DateTime = dateTime;
        OilTemperature = oilTemperature;
        AmbTemperature = ambTemperature;
        OilCondition = oilCondition;
        RocTimePeriod = rocTimePeriod;
        RocValue = rocValue;
        Status = status;
    }

    public DisplayLog(
        uint address,
        uint minutesSince2000,
        float oilCelsius,
        float ambCelsius,
        float oilLossFactor,
        float rocTimePeriod,
        float rocLossFactor,
        ushort status
    ) :
        this(
            address,
            new DateTime(2000, 1, 1).AddMinutes(minutesSince2000),
            new Temperature(oilCelsius, Temperature.Celsius),
            new Temperature(ambCelsius, Temperature.Celsius),
            new OilCondition(oilLossFactor, OilCondition.LossFactor),
            new TimeSpan(0, (int)rocTimePeriod, 0),
            new OilCondition(rocLossFactor, OilCondition.LossFactor),
            (EStatus)status
        )
    {
        // Nothing to do
    }
        
    public bool IsEmpty { get => Address == 0; }
}