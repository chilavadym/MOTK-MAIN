using Avalonia.Controls;
using MOTK.ViewModels;

namespace MOTK.Views
{
    public partial class SplashView : Window
    {
        public SplashView()
        {
            InitializeComponent();

            CanResize = false;

            SplashViewModel.CloseSplashWindowAction = Close;
        }
    }
}
