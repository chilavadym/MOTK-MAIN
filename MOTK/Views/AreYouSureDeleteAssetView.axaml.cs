using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MOTK.ViewModels;
using ReactiveUI;
using System;

namespace MOTK.Views
{
    public partial class AreYouSureDeleteAssetView : ReactiveWindow<AreYouSureDeleteAssetViewModel>
    {
        public AreYouSureDeleteAssetView()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.WhenActivated(d => d(ViewModel!.YesCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.CancelCommand.Subscribe(Close)));
        }
    }
}
