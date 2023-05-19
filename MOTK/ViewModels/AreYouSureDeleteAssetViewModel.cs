using MOTK.Enums;
using MOTK.Helpers;
using MOTK.Statics;
using ReactiveUI;
using System.Reactive;

namespace MOTK.ViewModels;

public class AreYouSureDeleteAssetViewModel : ViewModelBase
{
    private readonly string? _assetName;

    public AreYouSureDeleteAssetViewModel()
    {
        YesCommand = ReactiveCommand.Create(YesResponse);
        CancelCommand = ReactiveCommand.Create(CancelResponse);
    }

    public AreYouSureDeleteAssetViewModel(string? assetName)
    {
        _assetName = assetName; 

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
    public string? AssetNameString => $"Asset {_assetName}?";

    public AreYouSureResponse? SelectedResponse { get; set; }

    public ReactiveCommand<Unit, AreYouSureResponse?> YesCommand { get; }

    public ReactiveCommand<Unit, AreYouSureResponse?> CancelCommand { get; }
}