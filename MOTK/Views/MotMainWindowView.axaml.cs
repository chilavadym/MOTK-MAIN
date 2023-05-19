using Avalonia.ReactiveUI;
using MOTK.Helpers;
using MOTK.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;
using System;

namespace MOTK.Views;
public partial class MotMainWindowView : ReactiveWindow<MotMainWindowViewModel>
{

    public static MotMainWindowView? Instance { get; private set; }
    public MotMainWindowView()
    {
        InitializeComponent();
        Instance = this;
        this.WhenActivated(d => d(ViewModel!.AreYouSureDialog.RegisterHandler(AreYouSureDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.SaveNewSamplePointDialog.RegisterHandler(SaveSamplePointDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.SaveNewAssetDialog.RegisterHandler(SaveAssetDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.AddOrDeleteSamplePointDialog.RegisterHandler(AddOrDeleteSamplePointDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.AreYouSureDeleteSamplePointDialog.RegisterHandler(AreYouSureDeleteSamplePointDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.AreYouSureDeleteAssetDialog.RegisterHandler(AreYouSureDeleteAssetDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.AreYouSureDeleteOilTestResultDialog.RegisterHandler(AreYouSureDeleteOilTestResultDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ExportDataSuccessDialog.RegisterHandler(ExportDataSuccessDialogAsync)));
       

        //ClientSizeProperty.Changed.Subscribe(size =>
        //{
        //    // Do nothing when the window first opens
        //    if (Math.Abs(size.OldValue.Value.Width - 1424) == 0 && Math.Abs(size.OldValue.Value.Height - 720) == 0)
        //    {
        //        return;
        //    }

        //    //Don't resize just because modal dialog has opened up
        //    if (size.OldValue.Value.Width == 0 && size.OldValue.Value.Height == 0)
        //    {
        //        return;
        //    }

        //    // if width has changed, then fix height
        //    if (Math.Abs(Width - size.OldValue.Value.Width) > 0)
        //    {
        //        Height = size.NewValue.Value.Width / size.OldValue.Value.Width * Height;
        //    }

        //    // if height has changed, then fix width
        //    if (Math.Abs(Height - size.OldValue.Value.Height) > 0)
        //    {
        //        Width = size.NewValue.Value.Height / size.OldValue.Value.Height * Width;
        //    }
        //});
    }

    private async Task AreYouSureDialogAsync(InteractionContext<AreYouSureViewModel, AreYouSureResponse?> interaction)
    {
        var dialog = new AreYouSureView
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<AreYouSureResponse?>(this);

        interaction.SetOutput(result);
    }

    private async Task SaveSamplePointDialogAsync(InteractionContext<AddNewSamplePointViewModel, NewSamplePointResponse?> interaction)
    {
        var dialog = new AddNewSamplePointView
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<NewSamplePointResponse?>(this);

        interaction.SetOutput(result);
    }

    private async Task SaveAssetDialogAsync(InteractionContext<AddNewAssetViewModel, NewAssetResponse?> interaction)
    {
        var dialog = new AddNewAssetView
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<NewAssetResponse?>(this);

        interaction.SetOutput(result);
    }

    private async Task AddOrDeleteSamplePointDialogAsync(InteractionContext<AddOrDeleteSamplePointViewModel, SamplePointResponse?> interaction)
    {
        var dialog = new AddOrDeleteSamplePointView
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<SamplePointResponse?>(this);

        interaction.SetOutput(result);
    }

    private async Task AreYouSureDeleteSamplePointDialogAsync(InteractionContext<AreYouSureDeleteSamplePointViewModel, AreYouSureResponse?> interaction)
    {
        var dialog = new AreYouSureDeleteSamplePointView
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<AreYouSureResponse?>(this);

        interaction.SetOutput(result);
    }

    private async Task AreYouSureDeleteAssetDialogAsync(InteractionContext<AreYouSureDeleteAssetViewModel, AreYouSureResponse?> interaction)
    {
        var dialog = new AreYouSureDeleteAssetView
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<AreYouSureResponse?>(this);

        interaction.SetOutput(result);
    }

    private async Task AreYouSureDeleteOilTestResultDialogAsync(InteractionContext<AreYouSureDeleteOilTestResultViewModel, AreYouSureResponse?> interaction)
    {
        var dialog = new AreYouSureDeleteOilTestResultView
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<AreYouSureResponse?>(this);

        interaction.SetOutput(result);
    }

    private async Task ExportDataSuccessDialogAsync(InteractionContext<ExportDataSuccessViewModel, OkResponse?> interaction)
    {
        var dialog = new ExportDataSuccessView
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<OkResponse?>(this);

        interaction.SetOutput(result);
    }

 
}
