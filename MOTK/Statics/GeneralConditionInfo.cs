using Common.Units;
using MOTK.Enums;
using MOTK.Models;
using MOTK.Services.Interfaces;
using MOTK.Services;
using System.Collections.Generic;

namespace MOTK.Statics;

public static class GeneralConditionInfo
{
    private const int LossFactorAirAlarm = -10;
    private const int LossFactorAirWarning = -5;

    private static double[]? _alertPreset;
    private static List<string>? _availableApplications;

    public static EOilCondition GetGeneralConditionInfo(OilTestResult? oilTestResult)
    {
        double lossFactor = -100;

        if (_alertPreset is null)
        {
            _alertPreset = AlertPresets.GetAlertLevels(oilTestResult?.OilTest?.SelectedOil?.Application);
            _availableApplications = AlertPresets.AvailableApplications;
        }

        IGeneralSettingsDatabase settingsDb = new GeneralSettingsDatabase();
        settingsDb.ReadFromDatabase();

        var generalSettings = settingsDb.GeneralSettings;
        if (generalSettings?.TanDeltaNumber == true)
        {
            OilCondition.GlobalUnit = OilCondition.TanDeltaNumber;

            // Connecting so that the value is displayed in the graph
            Tdn = OilCondition.TanDeltaNumber?.ToString();
            LossFactor = null;
        }
        else if (generalSettings?.LossFactor == true)
        {
            OilCondition.GlobalUnit = OilCondition.LossFactor;

            // Connecting so that the value is displayed in the graph
            LossFactor = OilCondition.LossFactor?.ToString();
            Tdn = null;
        }

        if (oilTestResult?.SensorCondition is not null)
        {
            TdnOrLossFactor = (oilTestResult.SensorCondition.Value.OilCond ?? OilCondition.MaxValue).ToString("0 u");

            MaxValue = OilCondition.MaxValue.ToString("0 u");
            MinValue = OilCondition.MinValue.ToString("0 u");
            
            if (OilCondition.GlobalUnit == OilCondition.TanDeltaNumber)
            {
                lossFactor = OilCondition.Convert(oilTestResult.SensorCondition.Value.OilCond ?? OilCondition.MaxValue, OilCondition.TanDeltaNumber, OilCondition.LossFactor);
            }
            else
            {
                lossFactor = oilTestResult.SensorCondition.Value.OilCond ?? OilCondition.MaxValue;
            }
        }

        if (_alertPreset is not null)
        {
            if (lossFactor <= LossFactorAirAlarm)
            {
                return EOilCondition.Alert;
            }

            if (lossFactor < LossFactorAirWarning)
            {
                return EOilCondition.Caution;
            }

            if (lossFactor < _alertPreset[0]) // Warning
            {
                return EOilCondition.Okay;
            }

            if (lossFactor < _alertPreset[1]) // Alarm
            {
                return EOilCondition.Caution;
            }
        }

        return EOilCondition.Alert;
    }

    public static string? TdnOrLossFactor { get; set; }
    public static string? MaxValue { get; set;}
    public static string? MinValue { get; set; }
    public static List<string>? AvailableApplications => _availableApplications;
    public static string? Tdn { get; set; }
    public static string? LossFactor { get; set; }
}