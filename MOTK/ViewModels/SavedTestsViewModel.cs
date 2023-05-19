using MOTK.Enums;
using MOTK.Helpers;
using MOTK.Models;
using MOTK.Notifications;
using MOTK.Services;
using MOTK.Services.Interfaces;
using MOTK.Statics;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOTK.ViewModels;

public class SavedTestsViewModel : ViewModelBase
{
    private SavedTestRow? _selectedSavedTest;
    private bool _searchButtonClicked;
    private readonly OilTestResultNeedDisplaying? _oilTestCompleted;
    private List<SavedTestRow>? _savedTestRows;
    private string? _savedTestToSearch;

    private readonly IOilTestResultDatabase _db;

    public SavedTestsViewModel()
    {
        _db = new OilTestResultDatabase();
    }

    public SavedTestsViewModel(OilTestResultNeedDisplaying? oilTestCompleted, GridSortingColumn? gridSortingColumn)
    {
        _oilTestCompleted = oilTestCompleted;
        SavedTestRows = new List<SavedTestRow>();

        _db = new OilTestResultDatabase();
        _db.ReadFromDatabase();

        if (_db.OilTestResultsObservable != null)
        {
            foreach (var oilTestResult in _db.OilTestResultsObservable)
            {
                if (oilTestResult.SensorCondition != null)
                {
                    var condition = GeneralConditionInfo.GetGeneralConditionInfo(oilTestResult);

                    var savedTestRow = new SavedTestRow
                    {
                        DateOfTest = ConvertToRegularDateTime(oilTestResult.SensorCondition.Value.Time),
                        Result = condition,
                        AssetId = oilTestResult.OilTest?.SelectedAssetName,
                        AssetDescription = oilTestResult.OilTest?.SelectedAssetDescription,
                        TestId = oilTestResult.OilTest?.TestReferenceName,
                        ResultIsAlert = condition == EOilCondition.Alert,
                        ResultIsCaution = condition == EOilCondition.Caution,
                        ResultIsOkay = condition == EOilCondition.Okay,
                        OilCondition = oilTestResult.SensorCondition.Value.OilCond.Value.ToString()

                    };

                    SavedTestRows.Add(savedTestRow);
                }
            }
        }

        if (gridSortingColumn is null) return;

        if (gridSortingColumn.Name == Constants.DateOfTest)
        {
            if (gridSortingColumn.SortOrder == ESortOrder.Ascending)
            {
                SavedTestRows = SavedTestRows.OrderBy(o => o.DateOfTest).ToList();
            }
            else if (gridSortingColumn.SortOrder == ESortOrder.Descending)
            {
                SavedTestRows = SavedTestRows.OrderByDescending(o => o.DateOfTest).ToList();
            }
        }
        else if (gridSortingColumn.Name == Constants.Result)
        {
            if (gridSortingColumn.SortOrder == ESortOrder.Ascending)
            {
                SavedTestRows = SavedTestRows.OrderBy(o => o.Result.ToString()).ToList();
            }
            else if (gridSortingColumn.SortOrder == ESortOrder.Descending)
            {
                SavedTestRows = SavedTestRows.OrderByDescending(o => o.Result.ToString()).ToList();
            }
        }
        else if (gridSortingColumn.Name == Constants.AssetId)
        {
            if (gridSortingColumn.SortOrder == ESortOrder.Ascending)
            {
                SavedTestRows = SavedTestRows.OrderBy(o => o.AssetId).ToList();
            }
            else if (gridSortingColumn.SortOrder == ESortOrder.Descending)
            {
                SavedTestRows = SavedTestRows.OrderByDescending(o => o.AssetId).ToList();
            }
        }
        else if (gridSortingColumn.Name == Constants.TestId)
        {
            if (gridSortingColumn.SortOrder == ESortOrder.Ascending)
            {
                SavedTestRows = SavedTestRows.OrderBy(o => o.TestId).ToList();
            }
            else if (gridSortingColumn.SortOrder == ESortOrder.Descending)
            {
                SavedTestRows = SavedTestRows.OrderByDescending(o => o.TestId).ToList();
            }
        }

        SavedTestsCount = $"{SavedTestRows.Count} Results";
    }

    private string ConvertToRegularDateTime(DateTime timeOfResult)
    {
        var timeString = timeOfResult.ToString("dd/MM/yyyy\r\nHH:mm:ss");

        return timeString;
    }

    public void SearchClicked()
    {
        SearchButtonClicked = true;

        if (_db.OilTestResultsObservable != null)
        {
            var tempSavedTestRows = new List<SavedTestRow>();

            foreach (var oilTestResult in _db.OilTestResultsObservable)
            {
                if (oilTestResult.OilTest?.SelectedAssetName != null &&
                    oilTestResult.OilTest is not null &&
                    oilTestResult.OilTest.TestReferenceName is not null &&
                    SavedTestToSearch is not null &&
                    oilTestResult.OilTest.SelectedAssetName.Contains(SavedTestToSearch))
                {
                    if (oilTestResult.SensorCondition != null)
                    {
                        var condition = GeneralConditionInfo.GetGeneralConditionInfo(oilTestResult);

                        var savedTestRow = new SavedTestRow
                        {
                            DateOfTest = ConvertToRegularDateTime(oilTestResult.SensorCondition.Value.Time),
                            Result = condition,
                            AssetId = oilTestResult.OilTest?.SelectedAssetName,
                            AssetDescription = oilTestResult.OilTest?.SelectedAssetDescription,
                            TestId = oilTestResult.OilTest?.TestReferenceName,
                            ResultIsAlert = condition == EOilCondition.Alert,
                            ResultIsCaution = condition == EOilCondition.Caution,
                            ResultIsOkay = condition == EOilCondition.Okay,
                            OilCondition = oilTestResult.SensorCondition.Value.OilCond.Value.ToString()

                        };

                        tempSavedTestRows.Add(savedTestRow);
                    }
                }
            }

            SavedTestRows = new List<SavedTestRow>();

            foreach (var savedTestRow in tempSavedTestRows)
            {
                SavedTestRows.Add(savedTestRow);
            }
        }
    }

    public void SearchCleared()
    {
        SearchButtonClicked = false;

        if (_db.OilTestResultsObservable != null)
        {
            SavedTestRows = new List<SavedTestRow>();

            foreach (var oilTestResult in _db.OilTestResultsObservable)
            {
                if (oilTestResult.SensorCondition != null)
                {
                    var condition = GeneralConditionInfo.GetGeneralConditionInfo(oilTestResult);

                    var savedTestRow = new SavedTestRow
                    {
                        DateOfTest = ConvertToRegularDateTime(oilTestResult.SensorCondition.Value.Time),
                        Result = condition,
                        AssetId = oilTestResult.OilTest?.SelectedAssetName,
                        TestId = oilTestResult.OilTest?.TestReferenceName,
                        AssetDescription = oilTestResult.OilTest?.SelectedAssetDescription,
                        ResultIsAlert = condition == EOilCondition.Alert,
                        ResultIsCaution = condition == EOilCondition.Caution,
                        ResultIsOkay = condition == EOilCondition.Okay,
                        OilCondition = oilTestResult.SensorCondition.Value.OilCond.Value.ToString()

                    };

                    SavedTestRows?.Add(savedTestRow);
                }
            }
        }

        SavedTestToSearch = string.Empty;
    }

    public List<SavedTestRow>? SavedTestRows
    {
        get => _savedTestRows;
        set => this.RaiseAndSetIfChanged(ref _savedTestRows, value);
    }

    public string? SavedTestToSearch
    {
        get => _savedTestToSearch;
        set => this.RaiseAndSetIfChanged(ref _savedTestToSearch, value);
    }

    public string? SavedTestsCount { get; set; }

    public SavedTestRow? SelectedSavedTest
    {
        get => _selectedSavedTest;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSavedTest, value);

            OilTestResult? tempResult = null;

            if (_oilTestCompleted is not null && _db.OilTestResultsObservable is not null)
            {
                foreach (var oilTestResult in _db.OilTestResultsObservable)
                {
                    if (oilTestResult.OilTest?.TestReferenceName == _selectedSavedTest?.TestId)
                    {
                        tempResult = oilTestResult;
                        tempResult.CanRepeat = false;
                        break;
                    }
                }

                if (tempResult is not null)
                {
                    _oilTestCompleted.OilTestResult = tempResult;
                }
            }
        }
    }

    public bool SearchButtonClicked
    {
        get => _searchButtonClicked;
        set => this.RaiseAndSetIfChanged(ref _searchButtonClicked, value);
    }
}