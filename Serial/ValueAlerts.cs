// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common.Units;

namespace Serial;

public class ValueAlerts
{
    static ValueAlerts()
    {
        Presets = new Dictionary<EPresets, double[]>()
        {
            [EPresets.Engine] = new[] { 25.0, 30.0, -7.5, -10.0, 110.0, 115.0, -15.0, -20.0, 5.0, 7.5, -2.0, -4.0 },
            [EPresets.Gear] = new[] { 25.0, 30.0, -7.5, -10.0, 70.0, 80.0, -15.0, -20.0, 1.5, 2.0, -2.0, -4.0 },
            [EPresets.Hydraulic] = new[] { 12.5, 15.0, -5.0, -7.5, 70.0, 80.0, -15.0, -20.0, 1.5, 2.0, -2.0, -4.0 },
            [EPresets.Compressor] = new[] { 12.5, 15.0, -5.0, -7.5, 70.0, 80.0, -15.0, -20.0, 1.5, 2.0, -3.0, -4.0 },
            [EPresets.Transformer] = new[] { 25.0, 30.0, -7.5, -10.0, 50.0, 60.0, -15.0, -20.0, 1.5, 2.0, -2.0, -3.0 },
        };
    }
    public ValueAlerts()
    {
        OilConditionAlertGroup = new AlertGroup<OilCondition>();
        OilTemperature = new AlertGroup<Temperature>();
        OilConditionRate = new AlertGroup<RateOfCondition>();
    }
    public ValueAlerts(
        AlertGroup<OilCondition> conditionAlertGroupAlerts,
        AlertGroup<Temperature> temperatureAlerts,
        AlertGroup<RateOfCondition> conditionRateAlerts)
    {

        OilConditionAlertGroup = conditionAlertGroupAlerts;
        OilTemperature = temperatureAlerts;
        OilConditionRate = conditionRateAlerts;
    }
    public ValueAlerts(
        OilCondition conditionPositiveWarning,
        OilCondition conditionPositiveAlarm,
        OilCondition conditionNegativeWarning,
        OilCondition conditionNegativeAlarm,
        Temperature temperaturePositiveWarning,
        Temperature temperaturePositiveAlarm,
        Temperature temperatureNegativeWarning,
        Temperature temperatureNegativeAlarm,
        RateOfCondition conditionRatePositiveWarning,
        RateOfCondition conditionRatePositiveAlarm,
        RateOfCondition conditionRateNegativeWarning,
        RateOfCondition conditionRateNegativeAlarm)
    {

        OilConditionAlertGroup = new AlertGroup<OilCondition>(
            conditionPositiveWarning,
            conditionPositiveAlarm,
            conditionNegativeWarning,
            conditionNegativeAlarm);
        OilTemperature = new AlertGroup<Temperature>(
            temperaturePositiveWarning,
            temperaturePositiveAlarm,
            temperatureNegativeWarning,
            temperatureNegativeAlarm);
        OilConditionRate = new AlertGroup<RateOfCondition>(
            conditionRatePositiveWarning,
            conditionRatePositiveAlarm,
            conditionRateNegativeWarning,
            conditionRateNegativeAlarm);
    }
    public ValueAlerts(
        Unit conditionUnit,
        Unit temperatureUnit,
        double conditionPositiveWarning,
        double conditionPositiveAlarm,
        double conditionNegativeWarning,
        double conditionNegativeAlarm,
        double temperaturePositiveWarning,
        double temperaturePositiveAlarm,
        double temperatureNegativeWarning,
        double temperatureNegativeAlarm,
        double conditionRatePositiveWarning,
        double conditionRatePositiveAlarm,
        double conditionRateNegativeWarning,
        double conditionRateNegativeAlarm)
    {

        OilConditionAlertGroup = new AlertGroup<OilCondition>(
            new OilCondition(conditionPositiveWarning, conditionUnit),
            new OilCondition(conditionPositiveAlarm, conditionUnit),
            new OilCondition(conditionNegativeWarning, conditionUnit),
            new OilCondition(conditionNegativeAlarm, conditionUnit));
        OilTemperature = new AlertGroup<Temperature>(
            new Temperature(temperaturePositiveWarning, temperatureUnit),
            new Temperature(temperaturePositiveAlarm, temperatureUnit),
            new Temperature(temperatureNegativeWarning, temperatureUnit),
            new Temperature(temperatureNegativeAlarm, temperatureUnit));
        OilConditionRate = new AlertGroup<RateOfCondition>(
            new RateOfCondition(conditionRatePositiveWarning, conditionUnit),
            new RateOfCondition(conditionRatePositiveAlarm, conditionUnit),
            new RateOfCondition(conditionRateNegativeWarning, conditionUnit),
            new RateOfCondition(conditionRateNegativeAlarm, conditionUnit));
    }
    public ValueAlerts(EPresets preset)
    {
        var values = Presets[preset];
        
        OilConditionAlertGroup = new AlertGroup<OilCondition>(
            new OilCondition(values[0], OilCondition.BaseUnit),
            new OilCondition(values[1], OilCondition.BaseUnit),
            new OilCondition(values[2], OilCondition.BaseUnit),
            new OilCondition(values[3], OilCondition.BaseUnit));
        OilTemperature = new AlertGroup<Temperature>(
            new Temperature(values[4], Temperature.BaseUnit),
            new Temperature(values[5], Temperature.BaseUnit),
            new Temperature(values[6], Temperature.BaseUnit),
            new Temperature(values[7], Temperature.BaseUnit));
        OilConditionRate = new AlertGroup<RateOfCondition>(
            new RateOfCondition(values[8], OilCondition.BaseUnit),
            new RateOfCondition(values[9], OilCondition.BaseUnit),
            new RateOfCondition(values[10], OilCondition.BaseUnit),
            new RateOfCondition(values[11], OilCondition.BaseUnit));
    }

    public AlertGroup<OilCondition> OilConditionAlertGroup { get; }
    public AlertGroup<Temperature> OilTemperature { get; }
    public AlertGroup<RateOfCondition> OilConditionRate { get; }

    public enum EPresets
    {
        Engine,
        Gear,
        Hydraulic,
        Compressor,
        Transformer,
    }
    private static Dictionary<EPresets, double[]> Presets { get; }
}