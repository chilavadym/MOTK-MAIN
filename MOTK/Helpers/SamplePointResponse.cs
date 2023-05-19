using MOTK.Enums;

namespace MOTK.Helpers;

public class SamplePointResponse
{
    public EDeleteAddOrClose Response { get; set; }
    public string? AssetName { get; set; }
    public string? SamplePointName { get; set; }
}