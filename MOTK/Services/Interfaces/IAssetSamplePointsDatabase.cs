using MOTK.Models;
using System.Collections.ObjectModel;

namespace MOTK.Services.Interfaces;

public interface IAssetSamplePointsDatabase : IUserDatabase
{
    public void AddNewAsset(string name, string? description);

    public void AddNewSamplePoint(string assetName, string samplePointName);

    public void DeleteChosenAsset(Asset? asset);

    public void DeleteChosenSamplePoint(AssetSamplePoints? assetSamplePoints, SamplePoint? samplePoint);

    public ObservableCollection<AssetSamplePoints>? AssetSamplePointsObservable { get; }
}