using MOTK.Events;
using MOTK.Models;
using MOTK.Services;
using MOTK.Services.Interfaces;
using Prism.Events;
using ReactiveUI;
using Splat;
using System;
using System.Collections.ObjectModel;

namespace MOTK.ViewModels;

public class AssetDetailsViewModel : ViewModelBase
{
    private ObservableCollection<AssetSamplePoints>? _assetSamplePointsList;

    private string? _selectedAssetName = string.Empty;
    private string? _selectedAssetDescription = string.Empty;
    private string? _selectedSamplePointName = string.Empty;
    private int _oilHours;

    private ObservableCollection<SamplePoint>? _samplePoints;
    private ObservableCollection<string>? _samplePointNames;

    private readonly OilTest? _oilTestModel;


    private bool _assetSelected;
    private bool _samplePointSelected;

    private IEventAggregator @event = Bootstrapper.GetRequiredService<IEventAggregator>(Locator.Current);
    public AssetDetailsViewModel()
    {
    }

    public AssetDetailsViewModel(bool assetSelected, bool samplePointSelected, OilTest? oilTestModel)
    {
        _oilTestModel = oilTestModel;

        _assetSelected = assetSelected;

        RestoreAnyPreviousState(samplePointSelected);

        if (TestReferenceName == null)
        {
            TestReferenceName = $"{DateTime.Now:s}";
            if (_oilTestModel != null) _oilTestModel.TestReferenceName = TestReferenceName;
        }

        IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();
        db.ReadFromDatabase();
        @event.GetEvent<NewAssetArgs>().Subscribe(UpdateDropDown);
        @event.GetEvent<SamplePointsEventArgs>().Subscribe(UpdateSamplePoints);
        AssetSamplePointsList = db.AssetSamplePointsObservable;

        Assets = new ObservableCollection<Asset>();

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

        AssetNames = new ObservableCollection<string>();

        if (Assets == null) return;

        foreach (var asset in Assets)
        {
            if (asset.AssetName != null) AssetNames.Add(asset.AssetName);
        }
    }

    private void UpdateSamplePoints(string obj)
    {
        //SamplePointNames.Add((string)obj);

        //IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();
        //db.ReadFromDatabase();
        //AssetSamplePointsList = db.AssetSamplePointsObservable;
    }

    private void UpdateDropDown(object obj)
    {
        AssetNames.Add(((dynamic)obj).Name);

        IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();
        db.ReadFromDatabase();
        AssetSamplePointsList = db.AssetSamplePointsObservable;

    }

    private void RestoreAnyPreviousState(bool samplePointSelected)
    {
        if (_oilTestModel == null) return;

        TestReferenceName = _oilTestModel.TestReferenceName;
        OilHours = _oilTestModel.OilHours;
        SelectedAssetName = _oilTestModel.SelectedAssetName;
        SelectedAssetDescription = _oilTestModel.SelectedAssetDescription;

        if (samplePointSelected)
        {
            SelectedSamplePointName = _oilTestModel.SelectedSamplePointName;
        }
    }

    public ObservableCollection<AssetSamplePoints>? AssetSamplePointsList
    {
        get => _assetSamplePointsList;
        set => this.RaiseAndSetIfChanged(ref _assetSamplePointsList, value);
    }

    public ObservableCollection<Asset>? Assets { get; }

    public ObservableCollection<string>? AssetNames { get; set; }

    public ObservableCollection<SamplePoint>? SamplePoints
    {
        get => _samplePoints;
        set => this.RaiseAndSetIfChanged(ref _samplePoints, value);
    }

    public ObservableCollection<string>? SamplePointNames
    {
        get => _samplePointNames;
        set => this.RaiseAndSetIfChanged(ref _samplePointNames, value);
    }

    public string? SelectedAssetName
    {
        get => _selectedAssetName;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedAssetName, value);

            if (_oilTestModel != null) _oilTestModel.SelectedAssetName = _selectedAssetName;

            if (_selectedAssetName != null)
            {
                AssetSelected = true;
            }

            if (AssetSamplePointsList != null)
            {
                SamplePoints = new ObservableCollection<SamplePoint>();

                foreach (var assetSamplePoints in AssetSamplePointsList)
                {
                    if (assetSamplePoints.Asset?.AssetName != _selectedAssetName) continue;

                    SelectedAssetDescription = assetSamplePoints.Asset?.AssetDescription;

                    if (assetSamplePoints.SamplePoints != null)
                    {
                        foreach (var samplePoint in assetSamplePoints.SamplePoints)
                        {
                            SamplePoints?.Add(samplePoint);
                        }
                    }

                    break;
                }

                SamplePointNames = new ObservableCollection<string>();

                if (SamplePoints?.Count == 0) return;

                if (SamplePoints != null)
                {
                    foreach (var samplePoint in SamplePoints)
                    {
                        if (samplePoint.Name != null) SamplePointNames.Add(samplePoint.Name);
                    }
                }
            }
        }
    }

    public string? SelectedAssetDescription
    {
        get => _selectedAssetDescription;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedAssetDescription, value);
            if (_oilTestModel != null) _oilTestModel.SelectedAssetDescription = _selectedAssetDescription;
        }
    }

    public string? SelectedSamplePointName
    {
        get => _selectedSamplePointName;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSamplePointName, value);
            if (_oilTestModel != null) _oilTestModel.SelectedSamplePointName = _selectedSamplePointName;

            if (_selectedSamplePointName != null)
            {
                SamplePointSelected = true;
            }
        }
    }

    public string? TestReferenceName { get; set; }

    public int OilHours
    {
        get => _oilHours;
        set
        {
            this.RaiseAndSetIfChanged(ref _oilHours, value);
            if (_oilTestModel != null) _oilTestModel.OilHours = _oilHours;
        }
    }

    public bool AssetSelected
    {
        get => _assetSelected;
        set => this.RaiseAndSetIfChanged(ref _assetSelected, value);
    }

    public bool SamplePointSelected
    {
        get => _samplePointSelected;
        set => this.RaiseAndSetIfChanged(ref _samplePointSelected, value);
    }
}