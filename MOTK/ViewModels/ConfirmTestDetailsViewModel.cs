using MOTK.Models;

namespace MOTK.ViewModels;

public class ConfirmTestDetailsViewModel : ViewModelBase
{
    public ConfirmTestDetailsViewModel() { }

    public ConfirmTestDetailsViewModel(OilTest? oilTest)
    {
        AssetId = oilTest?.SelectedAssetName;
        AssetDescription = oilTest?.SelectedAssetDescription;
        OilHours = oilTest?.OilHours;
        Manufacturer = oilTest?.SelectedOil?.Manufacturer;
        OilName = oilTest?.SelectedOil?.OilName;
        Viscosity = oilTest?.SelectedOil?.Viscosity.Value;
        Application = oilTest?.SelectedOil?.Application;
        SelectedOilVisualCheck = oilTest?.VisualCheck;
        SelectedSamplePointName = oilTest?.SelectedSamplePointName;
        SensorSerialNumber = oilTest?.SensorSerialNumber;
    }

    public string? AssetId { get; set; }
    public string? AssetDescription { get; set; }
    public int? OilHours { get; set; }
    public string? Manufacturer { get; set; }
    public string? OilName { get; set; }
    public string? Viscosity { get; set; }
    public string? Application { get; set; }
    public string? SelectedOilVisualCheck { get; set; }
    public string? SelectedSamplePointName { get; set; }
    public string? SensorSerialNumber { get; set; }
}