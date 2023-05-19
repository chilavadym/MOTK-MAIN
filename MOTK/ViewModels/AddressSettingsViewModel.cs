using MOTK.Models;
using MOTK.Services;
using MOTK.Services.Interfaces;
using ReactiveUI;

namespace MOTK.ViewModels;

public class AddressSettingsViewModel : ViewModelBase
{
    private string? _siteName;
    private string? _siteAddress;
    private string? _settingsSavedMessage;
    private bool _textEntered;

    public AddressSettingsViewModel()
    {
        IAddressUserDatabase db = new AddressUserDatabase();

        db.ReadFromDatabase();

        var addressSettings = db.AddressSettings;

        if (addressSettings == null) return;

        SiteName = addressSettings.SiteName;
        SiteAddress = addressSettings.SiteAddress;

        TextEntered = false;
    }

    public void SaveAddressDetails()
    {
        if (SiteName == null)
        {
            SettingsSavedMessage = "Site Name Is Empty";
            return;
        }

        if (SiteAddress == null)
        {
            SettingsSavedMessage = "Site Address Is Empty";
            return;
        }

        IAddressUserDatabase db = new AddressUserDatabase();

        var addressSettings = new AddressSettings
        {
            SiteName = SiteName,
            SiteAddress = SiteAddress
        };

        var response = db.WriteToDatabase(addressSettings);

        SettingsSavedMessage = response ? "Address Settings Saved Successfully!" : "Failed To Save. Please Avoid Special Characters!";
    }

    public string? SiteName
    {
        get => _siteName;
        set
        {
            this.RaiseAndSetIfChanged(ref _siteName, value);

            if (TextEntered == false)
            {
                TextEntered = true;
            }
        }
    }

    public string? SiteAddress
    {
        get => _siteAddress;
        set
        {
            this.RaiseAndSetIfChanged(ref _siteAddress, value);

            if (TextEntered == false)
            {
                TextEntered = true;
            }
        }
    }
    public bool TextEntered
    {
        get => _textEntered;
        set => this.RaiseAndSetIfChanged(ref _textEntered, value);
    }

    public string? SettingsSavedMessage
    {
        get => _settingsSavedMessage;
        set => this.RaiseAndSetIfChanged(ref _settingsSavedMessage, value);
    }
}