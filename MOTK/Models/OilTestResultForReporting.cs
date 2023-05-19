using Common.Units;

namespace MOTK.Models;

public class OilTestResultForReporting
{
    public OilTestResult? OilTestResult { get; set; }
    public string? SiteName { get; set; }
    public string? AssetName { get; set; }
    public string? AssetDescription { get; set; }
    public string? SamplePointName { get; set; }
    public string? TestReferenceName { get; set; }
    public string? Application { get; set; }
    public int? Hours { get; set; }
    public string? Manufacturer { get; set; }
    public string? OilName { get; set; }
    public string? Viscosity { get; set; }
    public string? SensorSerialNumber { get; set; }
    public string? TdnOrLossFactor { get; set; }
    public string? RemainingUsefulLife { get; set; }
    public string? VisualIndication { get; set; }

}