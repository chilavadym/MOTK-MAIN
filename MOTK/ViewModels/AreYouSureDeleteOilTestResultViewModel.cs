using MOTK.Enums;
using MOTK.Helpers;
using MOTK.Statics;
using ReactiveUI;
using System.Reactive;

namespace MOTK.ViewModels;

public class AreYouSureDeleteOilTestResultViewModel : ViewModelBase
{
    private readonly string? _testId;
    public AreYouSureDeleteOilTestResultViewModel()
    {
        YesCommand = ReactiveCommand.Create(YesResponse);
        CancelCommand = ReactiveCommand.Create(CancelResponse);
    }

    public AreYouSureDeleteOilTestResultViewModel(string? testId)
    {
        _testId = testId;

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
    public string? TestIdString => $"Test {_testId}?";

    public AreYouSureResponse? SelectedResponse { get; set; }

    public ReactiveCommand<Unit, AreYouSureResponse?> YesCommand { get; }

    public ReactiveCommand<Unit, AreYouSureResponse?> CancelCommand { get; }
}