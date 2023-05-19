using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MOTK.ViewModels;
using ReactiveUI;
using System;

namespace MOTK.Views
{
    public partial class AddOrDeleteSamplePointView : ReactiveWindow<AddOrDeleteSamplePointViewModel>
    {
        public AddOrDeleteSamplePointView()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.WhenActivated(d => d(ViewModel!.DeleteCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.AddCommand.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel!.CloseCommand.Subscribe(Close)));
        }
    }
}
