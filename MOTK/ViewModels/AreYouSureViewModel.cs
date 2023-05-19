using MOTK.Enums;
using MOTK.Helpers;
using MOTK.Statics;
using ReactiveUI;
using System.Reactive;

namespace MOTK.ViewModels;

public class AreYouSureViewModel : ViewModelBase
{
    public AreYouSureViewModel()
    {
        SelectedResponse = new AreYouSureResponse();

        BackCommand = ReactiveCommand.Create(NegativeResponse);
        YesCommand = ReactiveCommand.Create(PositiveResponse);
    }

    private AreYouSureResponse PositiveResponse()
    {
        SelectedResponse.Response = EYesNo.Yes;
        return SelectedResponse;
    }

    private AreYouSureResponse NegativeResponse()
    {
        SelectedResponse.Response = EYesNo.No;
        return SelectedResponse;
    }

    public string? MotBlueColor { get; set; } = Constants.MotBlueColor;

    public AreYouSureResponse SelectedResponse { get; set; }

    public ReactiveCommand<Unit, AreYouSureResponse> BackCommand { get; }

    public ReactiveCommand<Unit, AreYouSureResponse> YesCommand { get; }
}