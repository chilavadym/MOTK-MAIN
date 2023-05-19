using Common;
using MOTK.Enums;
using MOTK.Helpers;
using MOTK.Models;
using MOTK.Services;
using MOTK.Statics;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace MOTK.ViewModels;

public class OilDetailsViewModel : ViewModelBase
{
    private readonly OilTest? _oilTestModel;
    private readonly OilDatabase? _oilDatabase;
    private OilInfoForOilGrid? _selectedOil;
    private bool _oilSelected;
    private bool _searchButtonClicked;
    private string? _oilToSearch;
    private List<OilInfoForOilGrid>? _oils;

    public OilDetailsViewModel() { }

    public OilDetailsViewModel(OilDatabase? oilDatabase, OilTest? oilTestModel, GridSortingColumn? gridSortingColumn)
    {
        _oilTestModel = oilTestModel;
        _oilDatabase = oilDatabase;

        RestoreAnyPreviousState();

        Oils = new List<OilInfoForOilGrid>();

        if (oilDatabase is not null)
        {
            if (oilDatabase.Oils != null)
            {
                foreach (var oil in oilDatabase.Oils)
                {
                    var tempOil = new OilInfoForOilGrid
                    {
                        OilSerial = oil.OilSerial.ToString(),
                        Manufacturer = oil.Manufacturer,
                        OilName = oil.OilName,
                        Viscosity = oil.Viscosity.ToString(),
                        MaxTemp = oil.MaxTemp.ToString(),
                        MinTemp = oil.MinTemp.ToString(),
                        Application = oil.Application,
                        ProfileType = oil.ProfileType.ToString()
                    };

                    Oils.Add(tempOil);
                }
            }
        }

        if (gridSortingColumn is null) return;

        if (gridSortingColumn.Name == Constants.Manufacturer)
        {
            if (gridSortingColumn.SortOrder == ESortOrder.Ascending)
            {
                Oils = Oils.OrderBy(o => o.Manufacturer).ToList();
            }
            else if (gridSortingColumn.SortOrder == ESortOrder.Descending)
            {
                Oils = Oils.OrderByDescending(o => o.Manufacturer).ToList();
            }
        }
        else if (gridSortingColumn.Name == Constants.OilName)
        {
            if (gridSortingColumn.SortOrder == ESortOrder.Ascending)
            {
                Oils = Oils.OrderBy(o => o.OilName).ToList();
            }
            else if (gridSortingColumn.SortOrder == ESortOrder.Descending)
            {
                Oils = Oils.OrderByDescending(o => o.OilName).ToList();
            }
        }
        else if (gridSortingColumn.Name == Constants.Viscosity)
        {
            if (gridSortingColumn.SortOrder == ESortOrder.Ascending)
            {
                Oils = Oils.OrderBy(o => o.Viscosity).ToList();
            }
            else if (gridSortingColumn.SortOrder == ESortOrder.Descending)
            {
                Oils = Oils.OrderByDescending(o => o.Viscosity).ToList();
            }
        }
        else if (gridSortingColumn.Name == Constants.Application)
        {
            if (gridSortingColumn.SortOrder == ESortOrder.Ascending)
            {
                Oils = Oils.OrderBy(o => o.Application).ToList();
            }
            else if (gridSortingColumn.SortOrder == ESortOrder.Descending)
            {
                Oils = Oils.OrderByDescending(o => o.Application).ToList();
            }
        }
        else if (gridSortingColumn.Name == Constants.MaxTemp)
        {
            if (gridSortingColumn.SortOrder == ESortOrder.Ascending)
            {
                Oils = Oils.OrderBy(o => o.MaxTemp).ToList();
            }
            else if (gridSortingColumn.SortOrder == ESortOrder.Descending)
            {
                Oils = Oils.OrderByDescending(o => o.MaxTemp).ToList();
            }
        }
        else if (gridSortingColumn.Name == Constants.MinTemp)
        {
            if (gridSortingColumn.SortOrder == ESortOrder.Ascending)
            {
                Oils = Oils.OrderBy(o => o.MinTemp).ToList();
            }
            else if (gridSortingColumn.SortOrder == ESortOrder.Descending)
            {
                Oils = Oils.OrderByDescending(o => o.MinTemp).ToList();
            }
        }

        OilCount = $"{Oils.Count} Results";
    }

    public void SearchClicked()
    {
        SearchButtonClicked = true;

        if (_oilDatabase?.Oils != null)
        {
            var tempOilList = new List<OilInfoForOilGrid>();

            foreach (var oil in _oilDatabase.Oils)
            {
                if (OilToSearch is not null && oil.Manufacturer.Contains(OilToSearch))
                {
                    var tempOil = new OilInfoForOilGrid
                    {
                        OilSerial = oil.OilSerial.ToString(),
                        Manufacturer = oil.Manufacturer,
                        OilName = oil.OilName,
                        Viscosity = oil.Viscosity.ToString(),
                        MaxTemp = oil.MaxTemp.ToString(),
                        MinTemp = oil.MinTemp.ToString(),
                        Application = oil.Application,
                        ProfileType = oil.ProfileType.ToString()
                    };

                    tempOilList.Add(tempOil);
                }
            }

            Oils = new List<OilInfoForOilGrid>();

            foreach (var oil in tempOilList)
            {
                Oils.Add(oil);
            }
        }
    }

    public void SearchCleared()
    {
        SearchButtonClicked = false;

        if (_oilDatabase is not null)
        {
            if (_oilDatabase.Oils != null)
            {
                Oils = new List<OilInfoForOilGrid>();

                foreach (var oil in _oilDatabase.Oils)
                {
                    var tempOil = new OilInfoForOilGrid
                    {
                        OilSerial = oil.OilSerial.ToString(),
                        Manufacturer = oil.Manufacturer,
                        OilName = oil.OilName,
                        Viscosity = oil.Viscosity.ToString(),
                        MaxTemp = oil.MaxTemp.ToString(),
                        MinTemp = oil.MinTemp.ToString(),
                        Application = oil.Application,
                        ProfileType = oil.ProfileType.ToString()
                    };

                    Oils.Add(tempOil);
                }
            }
        }

        OilToSearch = string.Empty;
    }

    private void RestoreAnyPreviousState()
    {
        if (_oilTestModel == null) return;

        if (_oilTestModel.SelectedOil != null)
        {
            SelectedOil = FindCorrespondingOil(_oilTestModel.SelectedOil);
        }
    }

    public List<OilInfoForOilGrid>? Oils
    {
        get => _oils;
        set => this.RaiseAndSetIfChanged(ref _oils, value);
    }

    public string? OilToSearch
    {
        get => _oilToSearch;
        set => this.RaiseAndSetIfChanged(ref _oilToSearch, value);
    }

    public OilInfoForOilGrid? SelectedOil
    {
        get => _selectedOil;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedOil, value);
                
            if (_oilTestModel != null && _selectedOil?.Application != null)
            {
                _oilTestModel.SelectedOil = FindCorrespondingOilInfo(_selectedOil);
            }

            OilSelected = true;
        }
    }

    private OilInfoForOilGrid? FindCorrespondingOil(OilInfo oilInfo)
    {
        Oils = new List<OilInfoForOilGrid>();

        if (_oilDatabase is not null)
        {
            if (_oilDatabase.Oils != null)
            {
                foreach (var oil in _oilDatabase.Oils)
                {
                    var tempOil = new OilInfoForOilGrid
                    {
                        OilSerial = oil.OilSerial.ToString(),
                        Manufacturer = oil.Manufacturer,
                        OilName = oil.OilName,
                        Viscosity = oil.Viscosity.ToString(),
                        MaxTemp = oil.MaxTemp.ToString(),
                        MinTemp = oil.MinTemp.ToString(),
                        Application = oil.Application,
                        ProfileType = oil.ProfileType.ToString()
                    };

                    Oils.Add(tempOil);
                }
            }
        }

        foreach (var oilInfoForOilGrid in Oils)
        {
            if (oilInfoForOilGrid.OilName == oilInfo.OilName)
            {
                return oilInfoForOilGrid;
            }
        }

        return null;
    }

    private OilInfo? FindCorrespondingOilInfo(OilInfoForOilGrid oilInfoForOilGrid)
    {
        if (_oilDatabase is not null && _oilDatabase.Oils is not null)
        {
            foreach (var oil in _oilDatabase.Oils)
            {
                if (oilInfoForOilGrid.OilName == oil.OilName)
                {
                    return oil;
                }
            }
        }

        return null;
    }

    public string? OilCount { get; set; }

    public bool OilSelected
    {
        get => _oilSelected;
        set => this.RaiseAndSetIfChanged(ref _oilSelected, value);
    }

    public bool SearchButtonClicked
    {
        get => _searchButtonClicked;
        set => this.RaiseAndSetIfChanged(ref _searchButtonClicked, value);
    }
}