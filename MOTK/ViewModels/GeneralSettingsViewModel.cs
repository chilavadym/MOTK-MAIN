using MOTK.Models;
using MOTK.Services;
using MOTK.Services.Interfaces;
using ReactiveUI;

namespace MOTK.ViewModels;

public class GeneralSettingsViewModel : ViewModelBase
{
    private bool _fahrenheitSelected;
    private bool _celciusSelected;
    private bool _tanDeltaNumberSelected;
    private bool _lossFactorSelected;
    private bool _allowThirdPartySerialPortsSelected;
    private bool _newOptionChosen;
    private string? _settingsSavedMessage;



    public GeneralSettingsViewModel()
    {

    }

    public GeneralSettingsViewModel(string? oilDatabaseVersion)
    {
        IGeneralSettingsDatabase db = new GeneralSettingsDatabase();
        db.ReadFromDatabase();

        var generalSettings = db.GeneralSettings;

        if (generalSettings == null)
        {
            CelciusSelected = true;
            TanDeltaNumberSelected = true;
            SaveGeneralSettings();
            return;

        }

        FahrenheitSelected = generalSettings.Fahrenheit;
        CelciusSelected = generalSettings.Celcius;
        TanDeltaNumberSelected = generalSettings.TanDeltaNumber;
        LossFactorSelected = generalSettings.LossFactor;
        AllowThirdPartySerialPortsSelected = generalSettings.AllowThirdPartySerialPorts;

        SettingsSavedMessage = string.Empty;
        NewOptionChosen = false;
    }
    public void SaveGeneralSettings()
    {
        var generalSettings = new GeneralSettings
        {
            Fahrenheit = FahrenheitSelected,
            Celcius = CelciusSelected,
            TanDeltaNumber = TanDeltaNumberSelected,
            LossFactor = LossFactorSelected,
            AllowThirdPartySerialPorts = AllowThirdPartySerialPortsSelected,
        };

        IGeneralSettingsDatabase db = new GeneralSettingsDatabase();

        var response = db.WriteToDatabase(generalSettings);

        SettingsSavedMessage = response ? "General Settings Saved Successfully!" : "Failed To Save New General Settings!";

        NewOptionChosen = false;
    }

    #region Properties
    public bool FahrenheitSelected
    {
        get => _fahrenheitSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _fahrenheitSelected, value);
            NewOptionChosen = true;
        }
    }

    public bool CelciusSelected
    {
        get => _celciusSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _celciusSelected, value);
            NewOptionChosen = true;
        }
    }

    public bool TanDeltaNumberSelected
    {
        get => _tanDeltaNumberSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _tanDeltaNumberSelected, value);
            NewOptionChosen = true;
        }
    }
    #endregion

    public bool LossFactorSelected
    {
        get => _lossFactorSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _lossFactorSelected, value);
            NewOptionChosen = true;
        }
    }

    public bool AllowThirdPartySerialPortsSelected
    {
        get => _allowThirdPartySerialPortsSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _allowThirdPartySerialPortsSelected, value);
            NewOptionChosen = true;
        }
    }

    public string? SettingsSavedMessage
    {
        get => _settingsSavedMessage;
        set => this.RaiseAndSetIfChanged(ref _settingsSavedMessage, value);
    }

    public bool NewOptionChosen
    {
        get => _newOptionChosen;
        set => this.RaiseAndSetIfChanged(ref _newOptionChosen, value);
    }
}