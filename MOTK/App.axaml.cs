using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MOTK.Helpers;
using MOTK.Services;
using MOTK.ViewModels;
using MOTK.Views;
using Splat;

namespace MOTK;

public partial class App : Application
{
    private bool _openSplashWindow = true;

    private OilDatabase? _oilDatabase;
    private AppNumberWrapper? _appNumberWrapper;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();
        Bootstrapper.Register(Locator.CurrentMutable, Locator.Current);
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

        if (_openSplashWindow)
        {
            _oilDatabase = new OilDatabase();
            _appNumberWrapper = new AppNumberWrapper();
            desktop.MainWindow = new SplashView
            {
                DataContext = new SplashViewModel(_oilDatabase, _appNumberWrapper),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false
            };

            _openSplashWindow = false;
        }
        else
        {
            desktop.MainWindow = new MotMainWindowView
            {
                DataContext = new MotMainWindowViewModel(_oilDatabase, _appNumberWrapper),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                MaxHeight = 700,
                MaxWidth = 1000,
                MinHeight = 320,
                MinWidth = 500
            };

            desktop.MainWindow.Show();
        }
    }
}