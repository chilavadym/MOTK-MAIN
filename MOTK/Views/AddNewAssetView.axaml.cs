using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MOTK.ViewModels;
using ReactiveUI;
using System;

namespace MOTK.Views;

public partial class AddNewAssetView : ReactiveWindow<AddNewAssetViewModel>
{
    public AddNewAssetView()
    {
        InitializeComponent();

        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        this.WhenActivated(d => d(ViewModel!.SaveCommand.Subscribe(Close)));
        this.WhenActivated(d => d(ViewModel!.CancelCommand.Subscribe(Close)));
    }
}