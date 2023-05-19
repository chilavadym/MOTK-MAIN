using MOTK.Enums;
using MOTK.Models;
using MOTK.Services.Interfaces;
using MOTK.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace MOTK.ViewModels;

public class AssetsViewModel : ViewModelBase
{
    private ObservableCollection<AssetSamplePoints>? _assetSamplePointsList;
    private List<Asset>? _assets;

    private Asset? _selectedAsset;
    private bool _assetSelected;
    private bool _searchButtonClicked;
    private string? _assetToSearch;

    public AssetsViewModel()
    {

    }

    public AssetsViewModel(ESortOrder assetSortOrder)
    {
        AssetSelected = false;

        IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();
        db.ReadFromDatabase();

        AssetSamplePointsList = db.AssetSamplePointsObservable;

        Assets = new List<Asset>();

        if (AssetSamplePointsList != null)
        {
            foreach (var assetSamplePoints in AssetSamplePointsList)
            {
                var asset = new Asset
                {
                    AssetName = assetSamplePoints.Asset?.AssetName,
                    AssetDescription = assetSamplePoints.Asset?.AssetDescription
                };

                Assets.Add(asset);
            }
        }

        if (assetSortOrder == ESortOrder.Ascending)
        {
            Assets = Assets.OrderBy(o => o.AssetName).ToList();
        }
        else if (assetSortOrder == ESortOrder.Descending)
        {
            Assets = Assets.OrderByDescending(o => o.AssetName).ToList();
        }

        AssetCount = $"{Assets.Count} Results";
    }

    public void SearchClicked()
    {
        SearchButtonClicked = true;

        if (AssetSamplePointsList != null)
        {
            var tempAssetList = new List<Asset>();

            foreach (var assetSamplePoints in AssetSamplePointsList)
            {
                if (AssetToSearch is not null && assetSamplePoints.Asset?.AssetName is not null && assetSamplePoints.Asset.AssetName.Contains(AssetToSearch))
                {
                    var asset = new Asset
                    {
                        AssetName = assetSamplePoints.Asset?.AssetName,
                        AssetDescription = assetSamplePoints.Asset?.AssetDescription
                    };

                    tempAssetList.Add(asset);
                }
            }

            Assets = new List<Asset>();

            foreach (var asset in tempAssetList)
            {
                Assets.Add(asset);
            }
        }
    }

    public void SearchCleared()
    {
        SearchButtonClicked = false;

        if (AssetSamplePointsList != null)
        {
            Assets = new List<Asset>();

            foreach (var assetSamplePoints in AssetSamplePointsList)
            {
                var asset = new Asset
                {
                    AssetName = assetSamplePoints.Asset?.AssetName,
                    AssetDescription = assetSamplePoints.Asset?.AssetDescription
                };

                Assets.Add(asset);
            }
        }

        AssetToSearch = string.Empty;
    }

    public string? AssetToSearch
    {
        get => _assetToSearch;
        set => this.RaiseAndSetIfChanged(ref _assetToSearch, value);
    }

    public List<Asset>? Assets
    {
        get => _assets;
        set => this.RaiseAndSetIfChanged(ref _assets, value);
    }

    public ObservableCollection<AssetSamplePoints>? AssetSamplePointsList
    {
        get => _assetSamplePointsList;
        set => this.RaiseAndSetIfChanged(ref _assetSamplePointsList, value);
    }

    public Asset? SelectedAsset
    {
        get => _selectedAsset;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedAsset, value);
            AssetSelected = true;
        }
    }

    public bool AssetSelected
    {
        get => _assetSelected;
        set => this.RaiseAndSetIfChanged(ref _assetSelected, value);
    }

    public bool SearchButtonClicked
    {
        get => _searchButtonClicked;
        set => this.RaiseAndSetIfChanged(ref _searchButtonClicked, value);
    }

    public string? AssetCount { get; set; }
}