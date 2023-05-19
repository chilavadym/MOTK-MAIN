using MOTK.Helpers;
using MOTK.Statics;
using ReactiveUI;
using System.Reactive;

namespace MOTK.ViewModels;

public class ExportDataSuccessViewModel : ViewModelBase
{
    public ExportDataSuccessViewModel()
    {
        OkCommand = ReactiveCommand.Create(PositiveResponse);
    }

    public ExportDataSuccessViewModel(bool weAreExporting)
    {
        WeAreExporting = weAreExporting;
        OkCommand = ReactiveCommand.Create(PositiveResponse);
    }

    private OkResponse? PositiveResponse()
    {
        OkResponse = new OkResponse
        {
            Response = true
        };

        return OkResponse;
    }

    public string? MotBlueColor { get; set; } = Constants.MotBlueColor;

    public OkResponse? OkResponse { get; set; }

    public ReactiveCommand<Unit, OkResponse?> OkCommand { get; }

    public bool WeAreExporting { get; }
}