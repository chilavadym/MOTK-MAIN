using System.Collections.Generic;

namespace MOTK.Models;

public class AssetSamplePoints
{
    public AssetSamplePoints()
    {
        Asset = new Asset();
        SamplePoints = new List<SamplePoint>();
    }

    public Asset? Asset { get; set; }
    public List<SamplePoint>? SamplePoints { get; set; }
}