using MOTK.Enums;
using MOTK.Helpers;
using MOTK.Models;
using MOTK.Services;
using MOTK.Services.Interfaces;
using MOTK.Statics;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace MOTK.ViewModels;

public class AddOrDeleteSamplePointViewModel : ViewModelBase
{
    private readonly Asset? _asset;
    private AssetSamplePoints? _chosenAsset;
    private SamplePoint? _selectedSamplePoint;
    private bool _samplePointSelected;

    private readonly ObservableCollection<AssetSamplePoints>? _assetSamplePointsList;
    private ObservableCollection<SamplePoint>? _samplePoints;

    public AddOrDeleteSamplePointViewModel()
    {
        DeleteCommand = ReactiveCommand.Create(DeleteResponse);
        AddCommand = ReactiveCommand.Create(AddResponse);
        CloseCommand = ReactiveCommand.Create(CloseResponse);

        IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();
    }

    public AddOrDeleteSamplePointViewModel(Asset? asset)
    {
        _asset = asset;

        DeleteCommand = ReactiveCommand.Create(DeleteResponse);
        AddCommand = ReactiveCommand.Create(AddResponse);
        CloseCommand = ReactiveCommand.Create(CloseResponse);

        IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();
        db.ReadFromDatabase();

        _assetSamplePointsList = db.AssetSamplePointsObservable;

        ReadAssetSamplePoints();

        SamplePointSelected = false;
    }

    private SamplePointResponse? DeleteResponse()
    {
        SelectedResponse = new SamplePointResponse
        {
            Response = EDeleteAddOrClose.Delete,
            AssetName = AssetName,
            SamplePointName = SelectedSamplePoint?.Name
        };

        return SelectedResponse;
    }

    private SamplePointResponse? AddResponse()
    {
        SelectedResponse = new SamplePointResponse
        {
            Response = EDeleteAddOrClose.Add,
            AssetName = AssetName,
            SamplePointName = SelectedSamplePoint?.Name
        };

        return SelectedResponse;
    }

    private SamplePointResponse? CloseResponse()
    {
        SelectedResponse = new SamplePointResponse
        {
            Response = EDeleteAddOrClose.Close,
            AssetName = string.Empty
        };

        return SelectedResponse;
    }

    private void ReadAssetSamplePoints()
    {
        if (_asset == null) return;

        if (_assetSamplePointsList == null) return;
        
        foreach (var assetSamplePoints in _assetSamplePointsList)
        {
            if (assetSamplePoints.Asset?.AssetName != null && _asset.AssetName != null && _asset.AssetName.Contains(assetSamplePoints.Asset.AssetName))
            {
                ChosenAssetSamplePoints = assetSamplePoints;
                break;
            }
        }

        AssetName = ChosenAssetSamplePoints?.Asset?.AssetName;

        if (ChosenAssetSamplePoints != null && ChosenAssetSamplePoints.SamplePoints != null)
        {
            SamplePoints = new ObservableCollection<SamplePoint>();

            foreach (var samplePoint in ChosenAssetSamplePoints.SamplePoints)
            {
                SamplePoints?.Add(samplePoint);
            }
        }
    }

    public string? MotBlueColor { get; set; } = Constants.MotBlueColor;

    public string? GridLineColor { get; set; } = Constants.GridLineColor;

    public string? AssetName { get; set; }

    public AssetSamplePoints? ChosenAssetSamplePoints
    {
        get => _chosenAsset;
        set => this.RaiseAndSetIfChanged(ref _chosenAsset, value);
    }

    public ObservableCollection<SamplePoint>? SamplePoints
    {
        get => _samplePoints;
        set => this.RaiseAndSetIfChanged(ref _samplePoints, value);
    }

    public SamplePoint? SelectedSamplePoint
    {
        get => _selectedSamplePoint;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSamplePoint, value);
            SamplePointSelected = true;
        }
    }

    public bool SamplePointSelected
    {
        get => _samplePointSelected;
        set => this.RaiseAndSetIfChanged(ref _samplePointSelected, value);
    }

    public SamplePointResponse? SelectedResponse { get; set; }

    public ReactiveCommand<Unit, SamplePointResponse?> DeleteCommand { get; }

    public ReactiveCommand<Unit, SamplePointResponse?> AddCommand { get; }

    public ReactiveCommand<Unit, SamplePointResponse?> CloseCommand { get; }
}