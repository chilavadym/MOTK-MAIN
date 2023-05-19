using MOTK.Enums;
using MOTK.Models;
using ReactiveUI;

namespace MOTK.ViewModels;

public class VisualCheckViewModel : ViewModelBase
{
    private readonly OilTest? _oilTestModel;
    private EOilVisualCheck _selectedOilVisualCheck;

    private bool _clearAndBrightSelected;
    private bool _darkSelected;
    private bool _cloudyHazySelected;
    private bool _emulsifiedSelected;
    private bool _solidDebrisSelected;

    private bool _visualCheckSelected;

    public VisualCheckViewModel() { }

    public VisualCheckViewModel(OilTest? oilTestModel)
    {
        _oilTestModel = oilTestModel;

        RestoreAnyPreviousState();
    }

    private void RestoreAnyPreviousState()
    {
        if (_oilTestModel == null) return;

        if (_oilTestModel.SelectedOilVisualCheck == EOilVisualCheck.None) return;
        
        SelectedOilVisualCheck = _oilTestModel.SelectedOilVisualCheck;

        if (_oilTestModel.SelectedOilVisualCheck == EOilVisualCheck.ClearAndBright) ClearAndBrightSelected = true;
        else if (_oilTestModel.SelectedOilVisualCheck == EOilVisualCheck.Dark) DarkSelected = true;
        else if (_oilTestModel.SelectedOilVisualCheck == EOilVisualCheck.CloudyHazy) CloudyHazySelected = true;
        else if (_oilTestModel.SelectedOilVisualCheck == EOilVisualCheck.Emulsified) EmulsifiedSelected = true;
        else if (_oilTestModel.SelectedOilVisualCheck == EOilVisualCheck.SolidDebris) SolidDebrisSelected = true;
    }

    private string FormattedVisualCheck(EOilVisualCheck? visualCheck)
    {
        switch (visualCheck)
        {
            case EOilVisualCheck.ClearAndBright:
                return "Clear And Bright";
            case EOilVisualCheck.CloudyHazy:
                return "Cloudy/Hazy";
            case EOilVisualCheck.Dark:
                return "Dark";
            case EOilVisualCheck.Emulsified:
                return "Emulsified and/or Free Water";
            case EOilVisualCheck.SolidDebris:
                return "Solid Debris";
            default:
                return "None";
        }
    }

    public void ClearAndBright()
    {
        SelectedOilVisualCheck = EOilVisualCheck.ClearAndBright;

        if (_oilTestModel != null)
        {
            _oilTestModel.SelectedOilVisualCheck = EOilVisualCheck.ClearAndBright;
            _oilTestModel.VisualCheck = FormattedVisualCheck(EOilVisualCheck.ClearAndBright);
        }

        ClearAndBrightSelected = true;
        DarkSelected = false;
        CloudyHazySelected = false;
        EmulsifiedSelected = false;
        SolidDebrisSelected = false;
    }

    public void Dark()
    {
        SelectedOilVisualCheck = EOilVisualCheck.Dark;

        if (_oilTestModel != null)
        {
            _oilTestModel.SelectedOilVisualCheck = EOilVisualCheck.Dark;
            _oilTestModel.VisualCheck = FormattedVisualCheck(EOilVisualCheck.Dark);
        }

        ClearAndBrightSelected = false;
        DarkSelected = true;
        CloudyHazySelected = false;
        EmulsifiedSelected = false;
        SolidDebrisSelected = false;
    }

    public void CloudyHazy()
    {
        SelectedOilVisualCheck = EOilVisualCheck.CloudyHazy;

        if (_oilTestModel != null)
        {
            _oilTestModel.SelectedOilVisualCheck = EOilVisualCheck.CloudyHazy;
            _oilTestModel.VisualCheck = FormattedVisualCheck(EOilVisualCheck.CloudyHazy);
        }

        ClearAndBrightSelected = false;
        DarkSelected = false;
        CloudyHazySelected = true;
        EmulsifiedSelected = false;
        SolidDebrisSelected = false;
    }

    public void Emulsified()
    {
        SelectedOilVisualCheck = EOilVisualCheck.Emulsified;

        if (_oilTestModel != null)
        {
            _oilTestModel.SelectedOilVisualCheck = EOilVisualCheck.Emulsified;
            _oilTestModel.VisualCheck = FormattedVisualCheck(EOilVisualCheck.Emulsified);
        }

        ClearAndBrightSelected = false;
        DarkSelected = false;
        CloudyHazySelected = false;
        EmulsifiedSelected = true;
        SolidDebrisSelected = false;
    }

    public void SolidDebris()
    {
        SelectedOilVisualCheck = EOilVisualCheck.SolidDebris;

        if (_oilTestModel != null)
        {
            _oilTestModel.SelectedOilVisualCheck = EOilVisualCheck.SolidDebris;
            _oilTestModel.VisualCheck = FormattedVisualCheck(EOilVisualCheck.SolidDebris);
        }

        ClearAndBrightSelected = false;
        DarkSelected = false;
        CloudyHazySelected = false;
        EmulsifiedSelected = false;
        SolidDebrisSelected = true;
    }

    public EOilVisualCheck SelectedOilVisualCheck
    {
        get => _selectedOilVisualCheck;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedOilVisualCheck, value);

            if (_oilTestModel != null)
            {
                _oilTestModel.SelectedOilVisualCheck = _selectedOilVisualCheck;
                _oilTestModel.VisualCheck = FormattedVisualCheck(_selectedOilVisualCheck);
            }

            VisualCheckSelected = true;
        }
    }

    public bool ClearAndBrightSelected
    {
        get => _clearAndBrightSelected;
        set => this.RaiseAndSetIfChanged(ref _clearAndBrightSelected, value);
    }

    public bool DarkSelected
    {
        get => _darkSelected;
        set => this.RaiseAndSetIfChanged(ref _darkSelected, value);
    }
    public bool CloudyHazySelected
    {
        get => _cloudyHazySelected;
        set => this.RaiseAndSetIfChanged(ref _cloudyHazySelected, value);
    }
    public bool EmulsifiedSelected
    {
        get => _emulsifiedSelected;
        set => this.RaiseAndSetIfChanged(ref _emulsifiedSelected, value);
    }
    public bool SolidDebrisSelected
    {
        get => _solidDebrisSelected;
        set => this.RaiseAndSetIfChanged(ref _solidDebrisSelected, value);
    }

    public bool VisualCheckSelected
    {
        get => _visualCheckSelected;
        set => this.RaiseAndSetIfChanged(ref _visualCheckSelected, value);
    }
}