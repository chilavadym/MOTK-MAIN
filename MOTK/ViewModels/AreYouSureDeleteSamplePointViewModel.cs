using MOTK.Helpers;
using MOTK.Enums;
using MOTK.Statics;
using ReactiveUI;
using System.Reactive;

namespace MOTK.ViewModels;

public class AreYouSureDeleteSamplePointViewModel : ViewModelBase
{
    private readonly string? _assetName;
    private readonly string? _samplePointName;

    public AreYouSureDeleteSamplePointViewModel()
    {
        YesCommand = ReactiveCommand.Create(YesResponse);
        CancelCommand = ReactiveCommand.Create(CancelResponse);
    }

    public AreYouSureDeleteSamplePointViewModel(string? assetName, string? samplePointName)
    {
        _assetName = assetName;
        _samplePointName = samplePointName;

        YesCommand = ReactiveCommand.Create(YesResponse);
        CancelCommand = ReactiveCommand.Create(CancelResponse);
    }

    public AreYouSureResponse? YesResponse()
    {
        SelectedResponse = new AreYouSureResponse
        {
            Response = EYesNo.Yes
        };

        return SelectedResponse;
    }

    public AreYouSureResponse? CancelResponse()
    {
        SelectedResponse = new AreYouSureResponse
        {
            Response = EYesNo.No
        };

        return SelectedResponse;
    }

    public string? MotBlueColor { get; set; } = Constants.MotBlueColor;
    public string? AssetNameString => $"Asset {_assetName}";
    public string SamplePointNameString => $"Sample Point {_samplePointName}?";
    
    public AreYouSureResponse? SelectedResponse { get; set; }

    public ReactiveCommand<Unit, AreYouSureResponse?> YesCommand { get; }

    public ReactiveCommand<Unit, AreYouSureResponse?> CancelCommand { get; }
}