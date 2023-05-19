using MOTK.Enums;

namespace MOTK.Models;

public class SavedTestRow
{
    public string? DateOfTest { get; set; }
    public EOilCondition Result { get; set; }
    public string? AssetId { get; set; }
    public string? AssetDescription { get; set; }
    public string? TestId { get; set; }
    public bool ResultIsAlert { get; set; }
    public bool ResultIsCaution { get; set; }
    public bool ResultIsOkay { get; set; }
    public string OilCondition { get; set; }
}