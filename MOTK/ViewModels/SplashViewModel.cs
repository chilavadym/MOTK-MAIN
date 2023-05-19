using Avalonia;
using Avalonia.Threading;
using MOTK.Helpers;
using MOTK.Services;
using MOTK.Services.Interfaces;
using MOTK.Statics;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace MOTK.ViewModels;

public class SplashViewModel : ViewModelBase
{
    private string? _appNumberString = Constants.AppVersion;
    private string? _versionStatus = "Checking For Updates";
    private bool _driversChecked;
    private bool _settingsLoaded;
    private bool _databaseLoaded;
    private bool _registryUpdated;

    private readonly ISplashWindowGenerator? _splashWindowGenerator;
    private VersionArgs? _versionArgs;

    private int _numOfBooleanChecks;

    public SplashViewModel() { }

    public SplashViewModel(OilDatabase oilDatabase, AppNumberWrapper? appNumberWrapper)
    {
        OilDatabase = oilDatabase;
        AppNumberWrapper = appNumberWrapper;

        _splashWindowGenerator = new SplashWindowGenerator(OilDatabase, this);

        Dispatcher.UIThread.InvokeAsync(GenerateSplashWindow);
    }

    #region Subsciptions
    public string? AppNumberString
    {
        get => _appNumberString;
        set { Dispatcher.UIThread.InvokeAsync(() => this.RaiseAndSetIfChanged(ref _appNumberString, value)); }
    }

    public string? VersionStatus
    {
        get => _versionStatus;
        set { Dispatcher.UIThread.InvokeAsync(() => this.RaiseAndSetIfChanged(ref _versionStatus, value)); }
    }

    public bool DriversChecked
    {
        get => _driversChecked;
        set
        {
            Dispatcher.UIThread.InvokeAsync(() => this.RaiseAndSetIfChanged(ref _driversChecked, value));
            CheckIfAllChecksAreComplete();
        }
    }

    public bool SettingsLoaded
    {
        get => _settingsLoaded;
        set
        {
            Dispatcher.UIThread.InvokeAsync(() => this.RaiseAndSetIfChanged(ref _settingsLoaded, value));
            CheckIfAllChecksAreComplete();
        }
    }

    public bool DatabaseLoaded
    {
        get => _databaseLoaded;
        set
        {
            Dispatcher.UIThread.InvokeAsync(() => this.RaiseAndSetIfChanged(ref _databaseLoaded, value));
            CheckIfAllChecksAreComplete();
        }
    }

    public bool RegistryUpdated
    {
        get => _registryUpdated;
        set 
        {
            Dispatcher.UIThread.InvokeAsync(() => this.RaiseAndSetIfChanged(ref _registryUpdated, value));
            CheckIfAllChecksAreComplete();
        }
    }
    #endregion

    private void GenerateSplashWindow()
    {
        _splashWindowGenerator?.RunSetupAsync().ConfigureAwait(false);
    }

    private async Task CloseSplashAndOpenMotMainWindow()
    {
        await Task.Delay(3000).ConfigureAwait(true);

        // Open MOT Main Window
        Application.Current?.OnFrameworkInitializationCompleted();

        // Now close the Splash Window
        CloseSplashWindowAction?.Invoke();
    }

    private void CheckIfAllChecksAreComplete()
    {
        _numOfBooleanChecks++;

        if (_numOfBooleanChecks != 4) return;

        if (VersionStatus == Constants.YouAlreadyHaveTheLatestVersion || VersionStatus == Constants.FailedToCheckDriverUpdates || VersionStatus == Constants.ArchiveHasHigherDriverVersionNumber)
        {
            Dispatcher.UIThread.InvokeAsync(CloseSplashAndOpenMotMainWindow);
        }
        else
        {
            // Need to handle failure to check updates or non UpToDate OS version
        }
    }

    public OilDatabase? OilDatabase { get; set; }
    public AppNumberWrapper? AppNumberWrapper { get; set; }

    public static Action? CloseSplashWindowAction { get; set; }

    public VersionArgs? VersionArgs
    {
        get => _versionArgs;
        set
        {
            _versionArgs = value;

            if (_versionArgs?.Value != string.Empty)
            {
               // AppVersion = _versionArgs?.Value;
                AppNumberString = $"Software v: {AppVersion}";

                if (AppNumberWrapper != null)
                {
                    AppNumberWrapper.AppNumber = AppVersion;
                }
            }

            if (_versionArgs?.PropertyName == Constants.FailedToCheckDriverUpdates)
            {
                VersionStatus = Constants.FailedToCheckDriverUpdates;
            }
            else
            {
                VersionStatus = _versionArgs is { IsUpToDate: true } ? Constants.YouAlreadyHaveTheLatestVersion : Constants.ArchiveHasHigherDriverVersionNumber;
            }
        }
    }

    public string? AppVersion => Constants.AppVersion;
}