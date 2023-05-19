using MOTK.Enums;
using MOTK.Helpers;
using MOTK.Statics;
using ReactiveUI;
using System.Reactive;

namespace MOTK.ViewModels;

public class AddNewAssetViewModel : ViewModelBase
{
    public AddNewAssetViewModel()
    {
        SelectedResponse = new NewAssetResponse();

        SaveCommand = ReactiveCommand.Create(PositiveResponse);
        CancelCommand = ReactiveCommand.Create(NegativeResponse);
    }

    private NewAssetResponse PositiveResponse()
    {
        SelectedResponse.Response = ESaveCancel.Save;
        SelectedResponse.Name = NewAssetName;
        SelectedResponse.Description = NewAssetDescription;

        return SelectedResponse;
    }

    private NewAssetResponse NegativeResponse()
    {
        SelectedResponse.Response = ESaveCancel.Cancel;

        return SelectedResponse;
    }

    public string? NewAssetName { get; set; }

    public string? NewAssetDescription { get; set; }

    public string? MotBlueColor { get; set; } = Constants.MotBlueColor;

    public NewAssetResponse SelectedResponse { get; set; }

    public ReactiveCommand<Unit, NewAssetResponse> SaveCommand { get; }

    public ReactiveCommand<Unit, NewAssetResponse> CancelCommand { get; }
}