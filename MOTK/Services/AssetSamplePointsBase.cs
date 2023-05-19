using MOTK.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace MOTK.Services;

public abstract class AssetSamplePointsBase : UserDatabase
{
    protected void AddAsset(string name, string? description)
    {
        var assetSamplePoints = new AssetSamplePoints();
        if (assetSamplePoints.Asset == null) return;

        assetSamplePoints.Asset.AssetName = name;
        assetSamplePoints.Asset.AssetDescription = description;
        AssetSamplePointsList?.Add(assetSamplePoints);

        ConvertToJson();
    }

    protected override void ConvertToJson()
    {
        var serializer = new JsonSerializer();
        using var streamWriter = new StreamWriter(DatabasePath);
        using JsonWriter writer = new JsonTextWriter(streamWriter);
        serializer.Serialize(writer, AssetSamplePointsList);
    }

    public List<AssetSamplePoints>? AssetSamplePointsList { get; set; }
}