using Common;
using MOTK.Enums;

namespace MOTK.Models;

public class OilTest
{
    public string? TestReferenceName { get; set; }
    public int OilHours { get; set; }
    public string? SelectedAssetName { get; set; }
    public string? SelectedAssetDescription { get; set; }
    public string? SelectedSamplePointName { get; set; }
    public OilInfo? SelectedOil { get; set; }
    public EOilVisualCheck SelectedOilVisualCheck { get; set; }
    public string? VisualCheck { get; set; }
    public string? SensorSerialNumber { get; set; }
}