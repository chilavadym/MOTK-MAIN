using MOTK.Enums;
using MOTK.Statics;
using ReactiveUI;

namespace MOTK.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private ViewModelBase? _rightWindowContent;
    private ESettingsRightWindow _currentRightWindow;
    private ESettingsRightWindow _newRightWindow;

    private bool _generalSelected;
    private bool _addressSelected;
    private bool _assetsSelected;

    private readonly string? _oilDatabaseVersion;
    private readonly ESortOrder _assetSortOrder;

    public SettingsViewModel()
    {

    }

    public SettingsViewModel(string? oilDatabaseVersion, bool generalSettings, ESortOrder assetSortOrder)
    {
        _oilDatabaseVersion = oilDatabaseVersion;
        _assetSortOrder = assetSortOrder;

        if (generalSettings)
        {
            RightWindowContent = new GeneralSettingsViewModel(_oilDatabaseVersion); // To start off with
            _currentRightWindow = ESettingsRightWindow.General;

            GeneralSelected = true;
            AddressSelected = false;
            AssetsSelected = false;
        }
        else
        {
            RightWindowContent = new AssetsViewModel(_assetSortOrder);
            _currentRightWindow = ESettingsRightWindow.Assets;

            GeneralSelected = false;
            AddressSelected = false;
            AssetsSelected = true;
        }
    }

    private void General()
    {
        _newRightWindow = ESettingsRightWindow.General;
        if (_newRightWindow == _currentRightWindow) return;

        RightWindowContent = new GeneralSettingsViewModel();
        _currentRightWindow = ESettingsRightWindow.General;

        GeneralSelected = true;
        AddressSelected = false;
        AssetsSelected = false;
    }

    private void Address()
    {
        _newRightWindow = ESettingsRightWindow.Address;
        if (_newRightWindow == _currentRightWindow) return;

        RightWindowContent = new AddressSettingsViewModel();
        _currentRightWindow = ESettingsRightWindow.Address;

        GeneralSelected = false;
        AddressSelected = true;
        AssetsSelected = false;
    }

    private void Assets()
    {
        _newRightWindow = ESettingsRightWindow.Assets;
        if (_newRightWindow == _currentRightWindow) return;

        RightWindowContent = new AssetsViewModel(_assetSortOrder);
        _currentRightWindow = ESettingsRightWindow.Assets;

        GeneralSelected = false;
        AddressSelected = false;
        AssetsSelected = true;
    }


    public string? MotBlueColorDarker { get; set; } = Constants.MotBlueColorDarker;

    public bool GeneralSelected
    {
        get => _generalSelected;
        set => this.RaiseAndSetIfChanged(ref _generalSelected, value);
    }

    public bool AddressSelected
    {
        get => _addressSelected;
        set => this.RaiseAndSetIfChanged(ref _addressSelected, value);
    }

    public bool AssetsSelected
    {
        get => _assetsSelected;
        set => this.RaiseAndSetIfChanged(ref _assetsSelected, value);
    }

    public ViewModelBase? RightWindowContent
    {
        get => _rightWindowContent;
        set => this.RaiseAndSetIfChanged(ref _rightWindowContent, value);
    }
}