using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MOTK.ViewModels;
using ReactiveUI;
using System;

namespace MOTK.Views
{
    public partial class AreYouSureDeleteSamplePointView : ReactiveWindow<AreYouSureDeleteSamplePointViewModel>
    {
        public AreYouSureDeleteSamplePointView()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.WhenActivated(d => d(ViewModel!.YesCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.CancelCommand.Subscribe(Close)));
        }
    }
}
