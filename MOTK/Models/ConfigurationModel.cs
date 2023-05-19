using Common;
using Serial;
using static Serial.Sensor;

namespace MOTK.Models;

public class ConfigurationModel
{
    public string? DeviceName { get; set; }
    public OilInfo? Oil { get; set; }
    public string? SensorName { get; set; }
    public Sensor? Device { get; set; }
    public Cable? Cable { get; set; }
    public string? ConnectedTo { get; set; }
    public int NodeId { get; set; }
    public ESerialType SerialType { get; set; }
    public string SerialTypeName => SerialType.ToString();
    public bool SelfStarting { get; set; }
    public string? BitRate { get; set; }
}