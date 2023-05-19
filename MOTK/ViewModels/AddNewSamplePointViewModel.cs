using MOTK.Enums;
using MOTK.Helpers;
using MOTK.Statics;
using ReactiveUI;
using System.Reactive;

namespace MOTK.ViewModels;

public class AddNewSamplePointViewModel : ViewModelBase
{
    public AddNewSamplePointViewModel()
    {
        SaveCommand = ReactiveCommand.Create(PositiveResponse);
        CancelCommand = ReactiveCommand.Create(NegativeResponse);
    }
    
    public AddNewSamplePointViewModel(string? selectedAssetName)
    {
        SelectedAssetName = selectedAssetName;
        SelectedResponse = new NewSamplePointResponse();

        SaveCommand = ReactiveCommand.Create(PositiveResponse);
        CancelCommand = ReactiveCommand.Create(NegativeResponse);
    }

    private NewSamplePointResponse? PositiveResponse()
    {
        if (SelectedResponse == null) return null;
        
        SelectedResponse.Response = ESaveCancel.Save;
        SelectedResponse.Name = NewSamplePointName;

        return SelectedResponse;
    }

    private NewSamplePointResponse? NegativeResponse()
    {
        if (SelectedResponse == null) return null;
        
        SelectedResponse.Response = ESaveCancel.Cancel;

        return SelectedResponse;
    }

    public string? SelectedAssetName { get; set; }

    public string? MotBlueColor { get; set; } = Constants.MotBlueColor;

    public string? NewSamplePointName { get; set; }

    public NewSamplePointResponse? SelectedResponse { get; set; }

    public ReactiveCommand<Unit, NewSamplePointResponse?> SaveCommand { get; }

    public ReactiveCommand<Unit, NewSamplePointResponse?> CancelCommand { get; }
}