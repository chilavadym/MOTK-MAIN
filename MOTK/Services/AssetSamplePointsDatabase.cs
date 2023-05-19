using MOTK.Models;
using MOTK.Services.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace MOTK.Services;

public class AssetSamplePointsDatabase : AssetSamplePointsBase, IAssetSamplePointsDatabase
{
    public AssetSamplePointsDatabase()
    {
        DatabaseFileName = "AssetSamplePoints.json";

        AssetSamplePointsObservable = new ObservableCollection<AssetSamplePoints>();
    }

    public void ReadFromDatabase()
    {
        var fileInfo = new FileInfo(DatabasePath);

        if (fileInfo.Exists)
        {
            var jsonData = File.ReadAllText(DatabasePath);
            AssetSamplePointsList = JsonConvert.DeserializeObject<List<AssetSamplePoints>>(jsonData);

            if (AssetSamplePointsList != null)
            {
                foreach (var assetSamplePoints in AssetSamplePointsList)
                {
                    AssetSamplePointsObservable?.Add(assetSamplePoints);
                }
            }
        }
    }

    public void AddNewAsset(string name, string? description)
    {
        if (NoUnwantedCharacters(name))
        {
            var fileInfo = new FileInfo(DatabasePath);

            if (fileInfo.Exists)
            {
                var jsonData = File.ReadAllText(DatabasePath);
                AssetSamplePointsList = JsonConvert.DeserializeObject<List<AssetSamplePoints>>(jsonData);

                var assetExists = false;

                if (AssetSamplePointsList != null)
                {
                    foreach (var assetSamplePoints in AssetSamplePointsList)
                    {
                        if (assetSamplePoints.Asset?.AssetName != name) continue;
                        
                        assetExists = true;
                        break;
                    }
                }

                if (!assetExists)
                {
                    AddAsset(name, description);

                    if (AssetSamplePointsList == null) return;

                    foreach (var assetSamplePoints in AssetSamplePointsList)
                    {
                        AssetSamplePointsObservable?.Add(assetSamplePoints);
                    }
                }
            }
            else
            {
                AssetSamplePointsList = new List<AssetSamplePoints>();

                AddAsset(name, description);

                foreach (var assetSamplePoints in AssetSamplePointsList)
                {
                    AssetSamplePointsObservable?.Add(assetSamplePoints);
                }
            }
        }
    }

    public void AddNewSamplePoint(string assetName, string samplePointName)
    {
        if (NoUnwantedCharacters(assetName) && NoUnwantedCharacters(samplePointName))
        {
            var fileInfo = new FileInfo(DatabasePath);

            if (!fileInfo.Exists) return;

            var jsonData = File.ReadAllText(DatabasePath);
            AssetSamplePointsList = JsonConvert.DeserializeObject<List<AssetSamplePoints>>(jsonData);

            if (AssetSamplePointsList == null) return;

            foreach (var assetSamplePoints in AssetSamplePointsList)
            {
                if (assetSamplePoints.Asset?.AssetName != assetName) continue;

                var samplePointExists = false;

                if (assetSamplePoints.SamplePoints != null)
                {
                    foreach (var samplePoint in assetSamplePoints.SamplePoints)
                    {
                        if (samplePoint.Name != samplePointName) continue;

                        samplePointExists = true;
                        break;
                    }

                    if (!samplePointExists)
                    {
                        assetSamplePoints.SamplePoints?.Add(new SamplePoint(samplePointName));
                        ConvertToJson();

                        foreach (var assetsAndSamplePoints in AssetSamplePointsList)
                        {
                            AssetSamplePointsObservable?.Add(assetsAndSamplePoints);
                        }
                    }
                }

                break;
            }
        }
    }

    public void DeleteChosenAsset(Asset? asset)
    {
        if (asset == null) return;

        if (AssetSamplePointsObservable != null)
        {
            foreach (var assetSamplePoints in AssetSamplePointsObservable)
            {
                if (assetSamplePoints.Asset?.AssetName != null && asset.AssetName != null && asset.AssetName.Contains(assetSamplePoints.Asset.AssetName))
                {
                    AssetSamplePointsObservable.Remove(assetSamplePoints);

                    if (AssetSamplePointsList != null)
                    {
                        AssetSamplePointsList.Remove(assetSamplePoints);
                    }
                    
                    break;
                }
            }

            ConvertToJson();
        }
    }

    public void DeleteChosenSamplePoint(AssetSamplePoints? assetSamplePoints, SamplePoint? samplePoint)
    {
        if (assetSamplePoints == null || samplePoint == null) return;

        if (AssetSamplePointsList == null) return;
        
        foreach (var asps in AssetSamplePointsList)
        {
            if (asps.Asset?.AssetName != null && assetSamplePoints.Asset?.AssetName != null && assetSamplePoints.Asset.AssetName.Contains(asps.Asset.AssetName))
            {
                if (asps.SamplePoints == null) continue;
                
                foreach (var sp in asps.SamplePoints)
                {
                    if (sp.Name != samplePoint.Name) continue;
                    asps.SamplePoints.Remove(sp);

                    break;
                }

                break;
            }
        }
            
        ConvertToJson();
    }

    public ObservableCollection<AssetSamplePoints>? AssetSamplePointsObservable { get; set; }
}