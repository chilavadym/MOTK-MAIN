// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Newtonsoft.Json;

namespace Common;

[JsonObject(MemberSerialization.OptIn)]
public struct Reading
{
    [JsonProperty]
    public Units.Temperature OilTemp { get; set; }
    [JsonProperty]
    public Units.Temperature AmbTemp { get; set; }
    [JsonProperty]
    public Units.OilCondition? OilCond { get; set; }
    [JsonProperty]
    public float? AlertState { get; set; }
    [JsonProperty]
    public DateTime Time { get; set; }

    public double TimeStamp2000 { get => (Time - new DateTime(2000, 1, 1)).TotalMinutes; }
    public string TimeStringFile { get => Time.ToString("yyyy-MM-dd' 'HH:mm:ss"); }


    [JsonConstructor]
    public Reading(DateTime time, Units.Temperature oilTemp, Units.Temperature ambTemp, Units.OilCondition? oilCond, float? alertState)
    {
        Time = time;
        OilTemp = oilTemp;
        AmbTemp = ambTemp;
        OilCond = oilCond;
        AlertState = alertState;
    }

    public Reading(Reading copy)
    {
        Time = copy.Time;
        OilTemp = copy.OilTemp;
        AmbTemp = copy.AmbTemp;
        OilCond = copy.OilCond;
        AlertState = copy.AlertState;
                    
    }

    public override string ToString()
    {
        return $"{{{OilTemp}, {AmbTemp}, {OilCond?.ToString() ?? "null"}, {AlertState?.ToString() ?? "null"}}}";
    }
}