using Common.Units;
using Microsoft.Win32;
using MOTK.ViewModels;
using MOTK.Statics;
using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using MOTK.Helpers;
using MOTK.Services.Interfaces;
using System.Reflection;

namespace MOTK.Services;

internal sealed class SplashWindowGenerator : ISplashWindowGenerator
{
    private static OilDatabase? _oilDatabase;
    private readonly SplashViewModel _splashViewModel;

    internal static string AlertPresetsPath
    {
        get
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Tan Delta Systems",
                "Mobile Oil Testing Kit",
                "rsc",
                "AlertPresets.txt");

            var directory = Path.GetDirectoryName(path);

            if (Directory.Exists(directory)) return path;

            if (directory != null) Directory.CreateDirectory(directory);

            return path;
        }
    }

    internal static string AlertPresetsInstallPath =>
        Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            "rsc",
            "AlertPresets.txt");

    internal SplashWindowGenerator(OilDatabase? oilDatabase, SplashViewModel splashViewModel)
    {
        _oilDatabase = oilDatabase;
        _splashViewModel = splashViewModel;
    }

    public async Task RunSetupAsync()
    {
        await Task.Run(CheckUpdatesAsync).ConfigureAwait(false);
        await Task.Run(CheckDriversAsync).ConfigureAwait(false);
        await Task.Run(LoadDatabaseAsync).ConfigureAwait(false);
        await Task.Run(LoadSettingsAsync).ConfigureAwait(false);
        await Task.Run(UpdateRegistry).ConfigureAwait(false);
        await Task.Run(LoadAlertPresets).ConfigureAwait(false);
    }

    private async Task CheckUpdatesAsync()
    {
#if DEBUG
        await Task.Delay(100);
#else
            var evtUpdateCheck = new AutoResetEvent(false);

            void UpdateCheckDone(object sender, EventArgs e)
            {
                evtUpdateCheck.Set();
            }

            autoUpdater.UpToDate += UpdateCheckDone;
            autoUpdater.ReadyToBeInstalled += UpdateCheckDone;
            autoUpdater.CheckingFailed += UpdateCheckDone;
            autoUpdater.DownloadingOrExtractingFailed += UpdateCheckDone;

            autoUpdater.ForceCheckForUpdate(false);

            await Task.Factory.StartNew(evtUpdateCheck.WaitOne, TaskCreationOptions.LongRunning).ConfigureAwait(false);
#endif
    }

    private async Task CheckDriversAsync()
    {
        VersionArgs versionArgs;

        if (File.Exists(ZipPath))
        {
            //ManagementObjectSearcher is used to query OS information
            ManagementObjectCollection results;

#pragma warning disable CA1416 // Validate platform compatibility
            using (var searcher = new ManagementObjectSearcher(DriverQuery))
#pragma warning restore CA1416 // Validate platform compatibility
            {
                // WMI contains 2 databases on x64 systems (one for x86, one for x64)
                // The context parameters here specify to search the relevent database for the OS
                // By default WMI automatically selects a database by using the app architecture (CADS is currently x86 only)
#pragma warning disable CA1416 // Validate platform compatibility
                searcher.Options.Context.Add("__ProviderArchitecture", Environment.Is64BitOperatingSystem ? 64 : 32);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
                searcher.Options.Context.Add("__RequiredArchitecture", true);
#pragma warning restore CA1416 // Validate platform compatibility

                // Perform the search
#pragma warning disable CA1416 // Validate platform compatibility
                results = searcher.Get();
#pragma warning restore CA1416 // Validate platform compatibility
            }

            // Find the most up-to-date driver in the operating system and get its version number
            var version = new Version(0, 0);

            foreach (var result in results)
            {
                try
                {
#pragma warning disable CA1416 // Validate platform compatibility
                    var ver = new Version(result.GetPropertyValue("DriverVersion").ToString() ?? string.Empty);
#pragma warning disable CA1416 // Validate platform compatibility
                    if (ver > version)
                    {
                        version = ver;
                    }
                }
                catch
                {
                    // ignored
                }
            }

            // Look in the ZIP archive to see if there is a higher
            // version in the ZIP archive
            var newVersion = new Version();
            var localDriver = string.Empty;

            using (var archive = System.IO.Compression.ZipFile.Open(ZipPath, System.IO.Compression.ZipArchiveMode.Read))
            {
                // Search through each file for "0.0.0.0" filename
                foreach (var entry in archive.Entries)
                {
                    if (Version.TryParse(entry.Name, out Version? ver) && ver > version && ver > newVersion)
                    {
                        localDriver = entry.FullName;
                        newVersion = ver;
                    }
                }

                // If localDriver was not set above then the ZIP archive did not
                // contain a higher driver version. Raise appropriate events
                if (string.IsNullOrEmpty(localDriver))
                {
                    versionArgs = new VersionArgs(Constants.VersionNumber, version.ToString(), true);
                    _splashViewModel.VersionArgs = versionArgs;

                    _splashViewModel.DriversChecked = true;
                }
                else
                {
                    // ZIP archive has a higher version number for driver.
                    // Raise appropriate events
                    versionArgs = new VersionArgs(Constants.ArchiveHasHigherDriverVersionNumber, version.ToString(), false);
                    _splashViewModel.VersionArgs = versionArgs;
                    
                    _splashViewModel.DriversChecked = true;
                }
            } // using
        }
        else
        {
            // If we can't install a new driver, there's no point searching for one as we can't rectify on failure
            // Raise appropriate events

            versionArgs = new VersionArgs(Constants.FailedToCheckDriverUpdates, string.Empty, false);
            _splashViewModel.VersionArgs = versionArgs;
        }

        await Task.Delay(0);
    }

    private async Task LoadDatabaseAsync()
    {
        if (_oilDatabase?.ActiveDatabase != null
            && _oilDatabase?.DefaultDatabase.Version > _oilDatabase?.ActiveDatabase.Version
            && _oilDatabase?.DefaultDatabase.Version > ApplicationStuff.GlobalSettings.DbPromptGen2)
        {
            try
            {
                OilDatabase.Update(OilDatabase.DatabaseInstallPath);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        if (_oilDatabase?.DefaultDatabase.Version != null)
        {
            ApplicationStuff.GlobalSettings.DbPromptGen2 = _oilDatabase?.DefaultDatabase.Version;
        }

        ApplicationStuff.GlobalSettings.SaveAsync(ApplicationStuff.GlobalSettingsSaveFile).Wait();

        _splashViewModel.DatabaseLoaded = true;

        await Task.Delay(0);
    }

    private async Task UpdateRegistry()
    {
        try
        {
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

#pragma warning disable CS8600
#pragma warning disable CA1416 // Validate platform compatibility
            var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\TanDeltaSystems_MOT.oils\shell\open\command",
                true);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CS8600

#pragma warning disable CS8600
#pragma warning disable CA1416 // Validate platform compatibility
            var command = (string)key?.GetValue(string.Empty, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CS8600

            if (command is null)
            {
#pragma warning disable CS8600
#pragma warning disable CA1416 // Validate platform compatibility
                key?.SetValue(string.Empty, $"\"{exePath}\" \"%1\"");
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CS8600
            }
            else if (!command.StartsWith("\""))
            {
                var index = command.IndexOf(".exe ");

                if (index >= 0)
                {
                    exePath = command.Substring(0, index + 4);
                    command = command.Remove(0, exePath.Length + 1);
                    var cells = command.Split(' ');

                    command = $"\"{exePath}\"";

                    foreach (var cell in cells)
                    {
                        var arg = cell;

                        if (!cell.StartsWith("\""))
                        {
                            arg = '"' + arg;
                        }

                        if (!cell.EndsWith("\""))
                        {
                            arg += '"';
                        }

                        command += $" {arg}";
                    }

#pragma warning disable CS8600
#pragma warning disable CA1416 // Validate platform compatibility
                    key?.SetValue(string.Empty, command);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CS8600
                }
            }
        }
        catch
        {
            // Ignore
        }

        _splashViewModel.RegistryUpdated = true;

        await Task.Delay(0);
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            await ApplicationStuff.GlobalSettings.LoadAsync(ApplicationStuff.GlobalSettingsSaveFile);
        }
        catch
        {
            try
            {
                await ApplicationStuff.GlobalSettings.SaveAsync(ApplicationStuff.GlobalSettingsSaveFile);
            }
            catch
            {
                // ignored
            }
        }

        Temperature.GlobalUnit =
            Temperature.AvailableUnits.FirstOrDefault(u => u?.Word == ApplicationStuff.GlobalSettings.TemperatureUnit);
        OilCondition.GlobalUnit =
            OilCondition.AvailableUnits.FirstOrDefault(u => u?.Word == ApplicationStuff.GlobalSettings.ConditionUnit);

        try
        {
            await ApplicationStuff.AppSettings.LoadAsync(ApplicationStuff.AppSettingsSaveFile);
        }
        catch
        {
            try
            {
                await ApplicationStuff.AppSettings.SaveAsync(ApplicationStuff.AppSettingsSaveFile);
            }
            catch
            {
                // Ignore
            }
        }

        _splashViewModel.SettingsLoaded = true;
    }

    private void LoadAlertPresets()
    {
        var sourceFile = new FileInfo(AlertPresetsInstallPath);
        var destinationFile = new FileInfo(AlertPresetsPath);

        if (sourceFile.LastWriteTime > destinationFile.LastWriteTime)
        {
            File.Copy(AlertPresetsInstallPath, AlertPresetsPath, true);
        }
    }

#if CheckIfDriverNeedsInstall
    public async Task CheckIfDriverNeedsInstallAndThenInstall()
    {
        /*********************************************************************************/
        /* The below is not fully implemented in this Avalonia application - TBD - RO    */
        /*********************************************************************************/

        // Check if the user wants us to install the driver or not
        var message = string.Format(
            version.Major == 0
                ? "Can't locate the driver for the configuration cable, would you like to install the driver v{0}?\r\n(Administrator permissions required)"
                : "A newer driver is available for the configuration cable, would you like to update from v{1} to v{0}?\r\n(Administrator permissions required)",
            newVersion, version);
        //var result = (DialogResult)Invoke(new Func<IWin32Window, string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>(MessageBox.Show),
        //    this, message, Properties.Resources.StrDriverInstall, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        //if (result != DialogResult.Yes)
        //    return;

        // Extract the installer file from the zip archive
        var tempDir = Directory.CreateDirectory(Path.GetTempPath());
        var tempFile = new FileInfo(tempDir.FullName + localDriver);

        try
        {
            var newFileName = tempFile.FullName + ".exe";
            if (File.Exists(tempFile.FullName))
                File.Delete(tempFile.FullName);
            if (File.Exists(newFileName))
                File.Delete(newFileName);

            System.IO.Compression.ZipFile.ExtractToDirectory(ZipPath, tempDir.FullName);
            tempFile.MoveTo(newFileName);
        }
        catch (Exception ex)
        {
            Common.Debug.DefaultLogger.Log($"{ex.GetType()} = {ex.Message}", "Handled Exception");
            //Invoke(new Func<IWin32Window, string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>(MessageBox.Show),
            //    this, Properties.Resources.StrDlgDriverExtractFail, Properties.Resources.StrFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Attempt to install the FTDI driver
        try
        {
            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo(tempFile.FullName) { },
            };
            proc.Start();
            //Click += OnClick;
            proc.WaitForExit();
            //Click -= OnClick;

            //void OnClick(object sender, EventArgs e)
            //{
            //    NativeMethods.SetForegroundWindow(proc.MainWindowHandle);
            //}
        }
        catch (Exception ex)
        {
            Common.Debug.DefaultLogger.Log($"{ex.GetType()} = {ex.Message}", "Handled Exception");
            //Invoke(new Func<IWin32Window, string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>(MessageBox.Show),
            //    this, Properties.Resources.StrDriverInstallFail, Properties.Resources.StrFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        File.Delete(ZipPath);
}
#endif

    private const string ZipName = "FTDI.zip";
    private static readonly string ZipPath = Path.Combine(Directory.GetCurrentDirectory(), "rsc", ZipName);
    private const string DriverQuery = "SELECT * FROM Win32_PnPSignedDriver WHERE DeviceClass LIKE '%PORTS%' AND DriverProviderName LIKE '%FTDI%'";
}