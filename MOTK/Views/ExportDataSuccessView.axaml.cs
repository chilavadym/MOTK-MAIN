using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MOTK.ViewModels;
using ReactiveUI;
using System;

namespace MOTK.Views
{
    public partial class ExportDataSuccessView : ReactiveWindow<ExportDataSuccessViewModel>
    {
        public ExportDataSuccessView()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.WhenActivated(d => d(ViewModel!.OkCommand.Subscribe(Close)));
        }
    }
}
