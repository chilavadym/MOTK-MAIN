using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MOTK.ViewModels;
using ReactiveUI;
using System;

namespace MOTK.Views;

public partial class AreYouSureView : ReactiveWindow<AreYouSureViewModel>
{
    public AreYouSureView()
    {
        InitializeComponent();

        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        this.WhenActivated(d => d(ViewModel!.BackCommand.Subscribe(Close)));
        this.WhenActivated(d => d(ViewModel!.YesCommand.Subscribe(Close)));
    }
}