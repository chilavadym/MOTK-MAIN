using Avalonia.Controls;
using Common.Units;
using MOTK.Enums;
using MOTK.Events;
using MOTK.Helpers;
using MOTK.Helpers.Interfaces;
using MOTK.Models;
using MOTK.Notifications;
using MOTK.Services;
using MOTK.Services.Interfaces;
using MOTK.Statics;
using MOTK.Views;
using Prism.Events;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
namespace MOTK.ViewModels;

public class MotMainWindowViewModel : ViewModelBase
{
    private ViewModelBase? _rightWindowContent;
    private ERightWindow _currentRightWindow;
    private ERightWindow _newRightWindow;
    private bool _firstOptionChosen;

    private OilTest? _oilTestModel;

    private bool _locatorClicked;
    private bool _newTestClicked;
    private bool _savedTestClicked;
    private bool _settingsClicked;
    private bool _helpClicked;

    private bool _step2Future;
    private bool _step2Current;
    private bool _step2Past;

    private bool _step3Future;
    private bool _step3Current;
    private bool _step3Past;

    private bool _step4Future;
    private bool _step4Current;
    private bool _step4Past;

    private bool _step5Future;
    private bool _step5Current;
    private bool _step5Past;

    private string? _step2NumberColor;
    private string? _step3NumberColor;
    private string? _step4NumberColor;
    private string? _step5NumberColor;
    private string? _step6NumberColor;

    private string? _step2NameColor;
    private string? _step3NameColor;
    private string? _step4NameColor;
    private string? _step5NameColor;
    private string? _step6NameColor;

    private bool _runTestClicked;

    private bool _mobileOilTestKitEnabled;
    private bool _newTestEnabled;
    private bool _savedTestsEnabled;
    private bool _settingsEnabled;
    private bool _helpEnabled;

    private readonly OilTestResultNeedDisplaying? _oilTestCompleted;

    private ESortOrder _assetSortOrder;

    private ESortOrder _manufacturerSortOrder;
    private ESortOrder _oilNameSortOrder;
    private ESortOrder _oilTypeSortOrder;
    private ESortOrder _applicationSortOrder;
    private ESortOrder _maxTempSortOrder;
    private ESortOrder _minTempSortOrder;

    private ESortOrder _dateTimeSortOrder;
    private ESortOrder _resultSortOrder;
    private ESortOrder _assetIdSortOrder;
    private ESortOrder _testIdSortOrder;
    private string? _oilDatabaseVersion;
    private string? _oilDatabaseVersionString;

    private static readonly string PATH = Environment.CurrentDirectory + "/rsc/Docs/";

    private IEventAggregator @event = Bootstrapper.GetRequiredService<IEventAggregator>(Locator.Current);
    public MotMainWindowViewModel()
    {
    }

    public MotMainWindowViewModel(OilDatabase? oilDatabase, AppNumberWrapper? appNumberWrapper)
    {
        IGeneralSettingsDatabase db = new GeneralSettingsDatabase();
        db.ReadFromDatabase();
        if (db.GeneralSettings == null)
        {
            var generalSettings = new GeneralSettings();
            generalSettings.Celcius = true;
            generalSettings.TanDeltaNumber = true;
            db.WriteToDatabase(generalSettings);
        }
        AreYouSureDialog = new Interaction<AreYouSureViewModel, AreYouSureResponse?>();
        SaveNewSamplePointDialog = new Interaction<AddNewSamplePointViewModel, NewSamplePointResponse?>();
        SaveNewAssetDialog = new Interaction<AddNewAssetViewModel, NewAssetResponse?>();
        AddOrDeleteSamplePointDialog = new Interaction<AddOrDeleteSamplePointViewModel, SamplePointResponse?>();
        AreYouSureDeleteSamplePointDialog = new Interaction<AreYouSureDeleteSamplePointViewModel, AreYouSureResponse?>();
        AreYouSureDeleteAssetDialog = new Interaction<AreYouSureDeleteAssetViewModel, AreYouSureResponse?>();
        AreYouSureDeleteOilTestResultDialog = new Interaction<AreYouSureDeleteOilTestResultViewModel, AreYouSureResponse?>();
        ExportDataSuccessDialog = new Interaction<ExportDataSuccessViewModel, OkResponse?>();


        OilDatabase = oilDatabase;
        AppNumber = appNumberWrapper?.AppNumber;

        RightWindowContent = new MobileOilTestKitViewModel(); // To start off with
        //RightWindowContent = new AssetDetailsViewModel(false, false, _oilTestModel); // To start off with

        _currentRightWindow = ERightWindow.MobileOilTestKit;
        _firstOptionChosen = false;

        MobileOilTestKitEnabled = true;
        NewTestEnabled = true;
        SavedTestsEnabled = true;
        SettingsEnabled = true;
        HelpEnabled = true;

        _oilTestCompleted = new OilTestResultNeedDisplaying();

        _assetSortOrder = ESortOrder.Unsorted;

        _manufacturerSortOrder = ESortOrder.Unsorted;
        _oilNameSortOrder = ESortOrder.Unsorted;
        _oilTypeSortOrder = ESortOrder.Unsorted;
        _applicationSortOrder = ESortOrder.Unsorted;

        _dateTimeSortOrder = ESortOrder.Unsorted;
        _resultSortOrder = ESortOrder.Unsorted;
        _assetIdSortOrder = ESortOrder.Unsorted;
        _testIdSortOrder = ESortOrder.Unsorted;



        OilDatabaseVersion = OilDatabase?.ActiveDatabase?.Version?.ToString();
        OilDatabaseVersionString = $"Oil Database: v{OilDatabaseVersion}";
    }

    private void ReadTemperatureSettings()
    {
        IGeneralSettingsDatabase db = new GeneralSettingsDatabase();
        db.ReadFromDatabase();

        var generalSettings = db.GeneralSettings;

        if (generalSettings?.Celcius == true)
        {
            Temperature.GlobalUnit = Temperature.Celsius;
        }
        else if (generalSettings?.Fahrenheit == true)
        {
            Temperature.GlobalUnit = Temperature.Fahrenheit;
        }
    }

    #region RightWindowContent
    private async Task MobileOilTestKit()
    {
        _newRightWindow = ERightWindow.MobileOilTestKit;
        if (_newRightWindow == _currentRightWindow) return;


        if (_currentRightWindow == ERightWindow.Help || _currentRightWindow == ERightWindow.SavedTests)
        {
            RightWindowContent = new MobileOilTestKitViewModel();
            _currentRightWindow = ERightWindow.MobileOilTestKit;
        }
        else
        {
            var vm = new AreYouSureViewModel();
            var result = await AreYouSureDialog.Handle(vm);

            if (result != null && result.Response == EYesNo.Yes)
            {
                RightWindowContent = new MobileOilTestKitViewModel();
                _currentRightWindow = ERightWindow.MobileOilTestKit;
            }
        }



        _firstOptionChosen = false;

        MobileOilTestKitEnabled = false;
        NewTestClicked = false;
        SavedTestsClicked = false;
        SettingsClicked = false;
        HelpClicked = false;
    }

    private async Task NewTest()
    {
        _oilTestModel = new OilTest();

        _newRightWindow = ERightWindow.NewTest;
        if (_newRightWindow == _currentRightWindow) return;

        if (_firstOptionChosen)
        {
            if (_currentRightWindow == ERightWindow.Help || _currentRightWindow == ERightWindow.SavedTests)
            {

                RightWindowContent = new CfgLocDevViewModel(_oilTestModel);
                _currentRightWindow = ERightWindow.Locator;
            }
            else
            {
                var vm = new AreYouSureViewModel();
                var result = await AreYouSureDialog.Handle(vm);

                if (result != null && result.Response == EYesNo.Yes)
                {
                    RightWindowContent = new CfgLocDevViewModel(_oilTestModel);
                    _currentRightWindow = ERightWindow.Locator;
                }
            }

        }
        else
        {
            RightWindowContent = new CfgLocDevViewModel(_oilTestModel);
            _currentRightWindow = ERightWindow.Locator;
        }

        _firstOptionChosen = true;

        LocatorClicked = true;
        NewTestClicked = true;

        Step2Future = true;
        Step2Current = false;
        Step2Past = false;

        Step2NumberColor = Constants.StepItemGray;
        Step2NameColor = Constants.StepItemGray;
        Step3NumberColor = Constants.StepItemGray;
        Step3NameColor = Constants.StepItemGray;
        Step4NumberColor = Constants.StepItemGray;
        Step4NameColor = Constants.StepItemGray;
        Step5NumberColor = Constants.StepItemGray;
        Step5NameColor = Constants.StepItemGray;
        Step6NumberColor = Constants.StepItemGray;
        Step6NameColor = Constants.StepItemGray;

        Step3Future = true;
        Step3Current = false;
        Step3Past = false;

        Step4Future = true;
        Step4Current = false;
        Step4Past = false;

        Step5Future = true;
        Step5Current = false;
        Step5Past = false;

        RunTestClicked = false;

        SavedTestsClicked = false;
        SettingsClicked = false;
        HelpClicked = false;
    }

    private async Task SavedTests()
    {
        if (_oilTestCompleted != null)
        {
            _oilTestCompleted.PropertyChanged += DisplayOilTestResultDetails;

            _newRightWindow = ERightWindow.SavedTests;
            if (_newRightWindow == _currentRightWindow) return;

            if (_firstOptionChosen)
            {
                if (_currentRightWindow == ERightWindow.Help || _currentRightWindow == ERightWindow.SavedTests)
                {
                    RightWindowContent = new SavedTestsViewModel(_oilTestCompleted, null);
                    _currentRightWindow = ERightWindow.SavedTests;
                }
                else
                {
                    var vm = new AreYouSureViewModel();
                    var result = await AreYouSureDialog.Handle(vm);

                    if (result != null && result.Response == EYesNo.Yes)
                    {
                        RightWindowContent = new SavedTestsViewModel(_oilTestCompleted, null);
                        _currentRightWindow = ERightWindow.SavedTests;
                    }
                }

            }
            else
            {
                RightWindowContent = new SavedTestsViewModel(_oilTestCompleted, null);
                _currentRightWindow = ERightWindow.SavedTests;
            }
        }

        _firstOptionChosen = true;

        NewTestClicked = false;
        SavedTestsClicked = true;
        SettingsClicked = false;
        HelpClicked = false;
    }

    private async Task Settings()
    {
        _newRightWindow = ERightWindow.Settings;
        if (_newRightWindow == _currentRightWindow) return;

        if (_firstOptionChosen)
        {
            if (_currentRightWindow == ERightWindow.Help || _currentRightWindow == ERightWindow.SavedTests)
            {

                if (AppNumber != null) RightWindowContent = new SettingsViewModel(OilDatabaseVersion, true, _assetSortOrder);
                _currentRightWindow = ERightWindow.Settings;
            }
            else
            {
                var vm = new AreYouSureViewModel();
                var result = await AreYouSureDialog.Handle(vm);

                if (result != null && result.Response == EYesNo.Yes)
                {
                    if (AppNumber != null) RightWindowContent = new SettingsViewModel(OilDatabaseVersion, true, _assetSortOrder);
                    _currentRightWindow = ERightWindow.Settings;
                }
            }

        }
        else
        {
            if (AppNumber != null) RightWindowContent = new SettingsViewModel(OilDatabaseVersion, true, _assetSortOrder);
            _currentRightWindow = ERightWindow.Settings;
        }

        _firstOptionChosen = true;

        NewTestClicked = false;
        SavedTestsClicked = false;
        SettingsClicked = true;
        HelpClicked = false;
    }

    private async Task Help()
    {
        _newRightWindow = ERightWindow.Help;
        if (_newRightWindow == _currentRightWindow) return;

        if (_firstOptionChosen)
        {
            if (_currentRightWindow == ERightWindow.Help || _currentRightWindow == ERightWindow.SavedTests)
            {
                RightWindowContent = new HelpViewModel();
                _currentRightWindow = ERightWindow.Help;
            }
            else
            {
                var vm = new AreYouSureViewModel();
                var result = await AreYouSureDialog.Handle(vm);

                if (result != null && result.Response == EYesNo.Yes)
                {
                    RightWindowContent = new HelpViewModel();
                    _currentRightWindow = ERightWindow.Help;
                }
            }

        }
        else
        {
            RightWindowContent = new HelpViewModel();
            _currentRightWindow = ERightWindow.Help;
        }

        _firstOptionChosen = true;

        NewTestClicked = false;
        SavedTestsClicked = false;
        SettingsClicked = false;
        HelpClicked = true;
    }
    #endregion

    #region NewTestChildren

    public void LocatorStage()
    {
        RightWindowContent = new CfgLocDevViewModel(_oilTestModel);

        LocatorClicked = true;
        NewTestClicked = true;

        Step2Future = true;
        Step2Current = false;
        Step2Past = false;

        Step2NumberColor = Constants.StepItemGray;
        Step2NameColor = Constants.StepItemGray;

        Step3Future = true;
        Step3Current = false;
        Step3Past = false;

        Step3NumberColor = Constants.StepItemGray;
        Step3NameColor = Constants.StepItemGray;

        Step4Future = true;
        Step4Current = false;
        Step4Past = false;

        Step4NumberColor = Constants.StepItemGray;
        Step4NameColor = Constants.StepItemGray;

        Step5Future = true;
        Step5Current = false;
        Step5Past = false;

        Step5NumberColor = Constants.StepItemGray;
        Step5NameColor = Constants.StepItemGray;

        RunTestClicked = false;

        Step6NumberColor = Constants.StepItemGray;
        Step6NameColor = Constants.StepItemGray;
    }

    public void AssetDetailsStage()
    {
        RightWindowContent = new AssetDetailsViewModel(false, true, _oilTestModel);

        LocatorClicked = false;
        NewTestClicked = true;

        Step2Future = false;
        Step2Current = true;
        Step2Past = false;

        Step2NumberColor = "White";
        Step2NameColor = "Black";

        Step3Future = true;
        Step3Current = false;
        Step3Past = false;

        Step3NumberColor = Constants.StepItemGray;
        Step3NameColor = Constants.StepItemGray;

        Step4Future = true;
        Step4Current = false;
        Step4Past = false;

        Step4NumberColor = Constants.StepItemGray;
        Step4NameColor = Constants.StepItemGray;

        Step5Future = true;
        Step5Current = false;
        Step5Past = false;

        Step5NumberColor = Constants.StepItemGray;
        Step5NameColor = Constants.StepItemGray;

        RunTestClicked = false;
    }

    public void NewTestOilStage()
    {
        ReadTemperatureSettings();

        RightWindowContent = new OilDetailsViewModel(OilDatabase, _oilTestModel, null);

        LocatorClicked = false;
        NewTestClicked = true;

        Step2Future = false;
        Step2Current = false;
        Step2Past = true;

        Step2NumberColor = Constants.StepItemGray;
        Step2NameColor = "Black";

        Step3Future = false;
        Step3Current = true;
        Step3Past = false;

        Step3NumberColor = "White";
        Step3NameColor = "Black";

        Step4Future = true;
        Step4Current = false;
        Step4Past = false;

        Step4NumberColor = Constants.StepItemGray;
        Step4NameColor = Constants.StepItemGray;

        Step5Future = true;
        Step5Current = false;
        Step5Past = false;

        Step5NumberColor = Constants.StepItemGray;
        Step5NameColor = Constants.StepItemGray;

        RunTestClicked = false;
    }

    public void NewTestVisualCheckStage()
    {
        RightWindowContent = new VisualCheckViewModel(_oilTestModel);

        LocatorClicked = false;
        NewTestClicked = true;

        Step2Future = false;
        Step2Current = false;
        Step2Past = true;

        Step3Future = false;
        Step3Current = false;
        Step3Past = true;

        Step3NumberColor = Constants.StepItemGray;
        Step3NameColor = "Black";

        Step4Future = false;
        Step4Current = true;
        Step4Past = false;

        Step4NumberColor = "White";
        Step4NameColor = "Black";

        Step5Future = true;
        Step5Current = false;
        Step5Past = false;

        Step5NumberColor = Constants.StepItemGray;
        Step5NameColor = Constants.StepItemGray;

        RunTestClicked = false;
    }

    public void NewTestConfirmStage()
    {
        RightWindowContent = new ConfirmTestDetailsViewModel(_oilTestModel);

        LocatorClicked = false;
        NewTestClicked = true;

        Step2Future = false;
        Step2Current = false;
        Step2Past = true;

        Step3Future = false;
        Step3Current = false;
        Step3Past = true;

        Step4Future = false;
        Step4Current = false;
        Step4Past = true;

        Step5Future = false;
        Step5Current = true;
        Step5Past = false;

        Step5NumberColor = "White";
        Step5NameColor = "Black";

        RunTestClicked = false;
    }

    public void NewTestResultsStage()
    {
        if (_oilTestCompleted != null)
        {
            _oilTestCompleted.PropertyChanged += DisplayOilTestResultDetails;

            RightWindowContent = new NewTestResultsViewModel(_oilTestModel, _oilTestCompleted);
        }

        LocatorClicked = false;
        NewTestClicked = true;

        Step2Future = false;
        Step2Current = false;
        Step2Past = true;

        Step3Future = false;
        Step3Current = false;
        Step3Past = true;

        Step4Future = false;
        Step4Current = false;
        Step4Past = true;

        Step4Future = false;
        Step4Current = false;
        Step4Past = true;

        Step5Future = false;
        Step5Current = false;
        Step5Past = true;

        RunTestClicked = true;

        Step6NameColor = "Black";

        _currentRightWindow = ERightWindow.Reset;

        MobileOilTestKitEnabled = false;
        NewTestEnabled = false;
        SavedTestsEnabled = false;
        SettingsEnabled = false;
        HelpEnabled = false;
    }
    #endregion

    #region NewTestChildrenRequests

    public async Task AddNewAsset()
    {
        try
        {
            var vm = new AddNewAssetViewModel();

            var result = await SaveNewAssetDialog.Handle(vm);

            if (result != null && result.Response == ESaveCancel.Save)
            {
                if (result.Name == null || result.Description == null) return;

                IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();
                db.AddNewAsset(result.Name, result.Description);
                var asset = new
                {
                    Name = result.Name,
                    Description = result.Description
                };
                @event.GetEvent<NewAssetArgs>().Publish(asset);
                if (_oilTestModel != null)
                {
                    _oilTestModel.SelectedAssetName = result.Name;
                    RightWindowContent = new AssetDetailsViewModel(true, false, _oilTestModel);
                }
            }
        }
        catch (Exception ex)
        {

            this.Log().Write(ex.Message, LogLevel.Error);
        }
    }

    public async Task AddNewSamplePoint()
    {
        try
        {
            var vm = new AddNewSamplePointViewModel(_oilTestModel?.SelectedAssetName);

            var result = await SaveNewSamplePointDialog.Handle(vm);

            if (result != null && result.Response == ESaveCancel.Save)
            {
                if (result.Name == null) return;

                IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();

                var assetName = _oilTestModel?.SelectedAssetName;

                if (assetName != null)
                {
                    db.AddNewSamplePoint(assetName, result.Name);

                    if (_oilTestModel != null)
                    {
                        _oilTestModel.SelectedSamplePointName = result.Name;
                        @event.GetEvent<SamplePointsEventArgs>().Publish(result.Name);
                        RightWindowContent = new AssetDetailsViewModel(true, true, _oilTestModel);
                    }
                }
            }
        }
        catch (Exception ex)
        {

            this.Log<MotMainWindowViewModel>().Error(ex.Message);
        }
    }

    public void ToggleSortOrderManufacturer()
    {
        var gridSortingColumn = new GridSortingColumn(Constants.Manufacturer);

        if (_manufacturerSortOrder == ESortOrder.Unsorted)
        {
            _manufacturerSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }
        else if (_manufacturerSortOrder == ESortOrder.Ascending)
        {
            _manufacturerSortOrder = gridSortingColumn.SortOrder = ESortOrder.Descending;
        }
        else if (_manufacturerSortOrder == ESortOrder.Descending)
        {
            _manufacturerSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }

        RightWindowContent = new OilDetailsViewModel(OilDatabase, _oilTestModel, gridSortingColumn);

        _currentRightWindow = ERightWindow.NewTest;
    }

    public void ToggleSortOrderOilName()
    {
        var gridSortingColumn = new GridSortingColumn(Constants.OilName);

        if (_oilNameSortOrder == ESortOrder.Unsorted)
        {
            _oilNameSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }
        else if (_oilNameSortOrder == ESortOrder.Ascending)
        {
            _oilNameSortOrder = gridSortingColumn.SortOrder = ESortOrder.Descending;
        }
        else if (_oilNameSortOrder == ESortOrder.Descending)
        {
            _oilNameSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }

        RightWindowContent = new OilDetailsViewModel(OilDatabase, _oilTestModel, gridSortingColumn);

        _currentRightWindow = ERightWindow.NewTest;
    }

    public void ToggleSortOrderViscosity()
    {
        var gridSortingColumn = new GridSortingColumn(Constants.Viscosity);

        if (_oilTypeSortOrder == ESortOrder.Unsorted)
        {
            _oilTypeSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }
        else if (_oilTypeSortOrder == ESortOrder.Ascending)
        {
            _oilTypeSortOrder = gridSortingColumn.SortOrder = ESortOrder.Descending;
        }
        else if (_oilTypeSortOrder == ESortOrder.Descending)
        {
            _oilTypeSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }

        RightWindowContent = new OilDetailsViewModel(OilDatabase, _oilTestModel, gridSortingColumn);

        _currentRightWindow = ERightWindow.NewTest;
    }

    public void ToggleSortOrderApplication()
    {
        var gridSortingColumn = new GridSortingColumn(Constants.Application);

        if (_applicationSortOrder == ESortOrder.Unsorted)
        {
            _applicationSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }
        else if (_applicationSortOrder == ESortOrder.Ascending)
        {
            _applicationSortOrder = gridSortingColumn.SortOrder = ESortOrder.Descending;
        }
        else if (_applicationSortOrder == ESortOrder.Descending)
        {
            _applicationSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }

        RightWindowContent = new OilDetailsViewModel(OilDatabase, _oilTestModel, gridSortingColumn);

        _currentRightWindow = ERightWindow.NewTest;
    }

    public void ToggleSortOrderMaxTemp()
    {
        var gridSortingColumn = new GridSortingColumn(Constants.MaxTemp);

        if (_maxTempSortOrder == ESortOrder.Unsorted)
        {
            _maxTempSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }
        else if (_maxTempSortOrder == ESortOrder.Ascending)
        {
            _maxTempSortOrder = gridSortingColumn.SortOrder = ESortOrder.Descending;
        }
        else if (_maxTempSortOrder == ESortOrder.Descending)
        {
            _maxTempSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }

        RightWindowContent = new OilDetailsViewModel(OilDatabase, _oilTestModel, gridSortingColumn);

        _currentRightWindow = ERightWindow.NewTest;
    }

    public void ToggleSortOrderMinTemp()
    {
        var gridSortingColumn = new GridSortingColumn(Constants.MinTemp);

        if (_minTempSortOrder == ESortOrder.Unsorted)
        {
            _minTempSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }
        else if (_minTempSortOrder == ESortOrder.Ascending)
        {
            _minTempSortOrder = gridSortingColumn.SortOrder = ESortOrder.Descending;
        }
        else if (_minTempSortOrder == ESortOrder.Descending)
        {
            _minTempSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }

        RightWindowContent = new OilDetailsViewModel(OilDatabase, _oilTestModel, gridSortingColumn);

        _currentRightWindow = ERightWindow.NewTest;
    }
    #endregion

    #region SavedTestChildren
    public void DisplayOilTestResultDetails(object? myObject, OilTestResultNeedDisplayingEventArgs e)
    {
        if (e.PropertyName == Constants.OilTestResultNeedDisplaying && e.OilTestResult != null)
        {
            IOilTestResultDatabase db = new OilTestResultDatabase();
            db.ReadFromDatabase();
            if (e.OilTestResult.OilTest != null)
            {
                //e.OilTestResult.OilTest.TestReferenceName = $"{DateTime.Now:s}";

                if (db.OilTestResultsObservable != null)
                {
                    var testResult = db.OilTestResultsObservable.Where(s =>
                        s.OilTest.SelectedSamplePointName == e.OilTestResult.OilTest.SelectedSamplePointName && s.OilTest.SelectedOil.OilName == e.OilTestResult.OilTest.SelectedOil.OilName
                        && s.OilTest.SelectedAssetName == e.OilTestResult.OilTest.SelectedAssetName).OrderByDescending(s => s.DateCreated);

                    RightWindowContent = new SavedTestResultViewModel(e.OilTestResult, testResult);
                }
            }

            _firstOptionChosen = true;

            _currentRightWindow = ERightWindow.Reset;

            if (_oilTestCompleted != null) _oilTestCompleted.PropertyChanged -= DisplayOilTestResultDetails;
        }

        NewTestClicked = false;
        SavedTestsClicked = true;
        SettingsClicked = false;
        HelpClicked = false;

        MobileOilTestKitEnabled = true;
        NewTestEnabled = true;
        SavedTestsEnabled = true;
        SettingsEnabled = true;
        HelpEnabled = true;
    }

    public void ToggleSortOrderDateTime()
    {
        var gridSortingColumn = new GridSortingColumn(Constants.DateOfTest);

        if (_dateTimeSortOrder == ESortOrder.Unsorted)
        {
            _dateTimeSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }
        else if (_dateTimeSortOrder == ESortOrder.Ascending)
        {
            _dateTimeSortOrder = gridSortingColumn.SortOrder = ESortOrder.Descending;
        }
        else if (_dateTimeSortOrder == ESortOrder.Descending)
        {
            _dateTimeSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }

        RightWindowContent = new SavedTestsViewModel(_oilTestCompleted, gridSortingColumn);

        _currentRightWindow = ERightWindow.SavedTests;
    }

    public void ToggleSortOrderResult()
    {
        var gridSortingColumn = new GridSortingColumn(Constants.Result);

        if (_resultSortOrder == ESortOrder.Unsorted)
        {
            _resultSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }
        else if (_resultSortOrder == ESortOrder.Ascending)
        {
            _resultSortOrder = gridSortingColumn.SortOrder = ESortOrder.Descending;
        }
        else if (_resultSortOrder == ESortOrder.Descending)
        {
            _resultSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }

        RightWindowContent = new SavedTestsViewModel(_oilTestCompleted, gridSortingColumn);

        _currentRightWindow = ERightWindow.SavedTests;
    }

    public void ToggleSortOrderAssetId()
    {
        var gridSortingColumn = new GridSortingColumn(Constants.AssetId);

        if (_assetIdSortOrder == ESortOrder.Unsorted)
        {
            _assetIdSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }
        else if (_assetIdSortOrder == ESortOrder.Ascending)
        {
            _assetIdSortOrder = gridSortingColumn.SortOrder = ESortOrder.Descending;
        }
        else if (_assetIdSortOrder == ESortOrder.Descending)
        {
            _assetIdSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }

        RightWindowContent = new SavedTestsViewModel(_oilTestCompleted, gridSortingColumn);

        _currentRightWindow = ERightWindow.SavedTests;
    }

    public void ToggleSortOrderTestId()
    {
        var gridSortingColumn = new GridSortingColumn(Constants.TestId);

        if (_testIdSortOrder == ESortOrder.Unsorted)
        {
            _testIdSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }
        else if (_testIdSortOrder == ESortOrder.Ascending)
        {
            _testIdSortOrder = gridSortingColumn.SortOrder = ESortOrder.Descending;
        }
        else if (_testIdSortOrder == ESortOrder.Descending)
        {
            _testIdSortOrder = gridSortingColumn.SortOrder = ESortOrder.Ascending;
        }

        RightWindowContent = new SavedTestsViewModel(_oilTestCompleted, gridSortingColumn);

        _currentRightWindow = ERightWindow.SavedTests;
    }
    #endregion

    #region SettingsChildren
    public async Task AddNewAssetInAssetsView()
    {
        var vm = new AddNewAssetViewModel();

        var result = await SaveNewAssetDialog.Handle(vm);

        if (result != null && result.Response == ESaveCancel.Save)
        {
            if (result.Name == null) return;

            IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();
            db.AddNewAsset(result.Name, result.Description);

            RightWindowContent = new SettingsViewModel(OilDatabaseVersion, false, _assetSortOrder);
        }
    }

    public async Task DeleteAssetInAssetView(Asset? asset)
    {
        var vm = new AreYouSureDeleteAssetViewModel();

        var result = await AreYouSureDeleteAssetDialog.Handle(vm);

        if (result?.Response == EYesNo.Yes)
        {
            IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();
            db.ReadFromDatabase();

            db.DeleteChosenAsset(asset);

            RightWindowContent = new SettingsViewModel(OilDatabaseVersion, false, _assetSortOrder);
        }
    }

    public async Task AddOrDeleteSamplePointInAssetsView(Asset? asset)
    {
        var vm1 = new AddOrDeleteSamplePointViewModel(asset);

        var result1 = await AddOrDeleteSamplePointDialog.Handle(vm1);

        var continueDeleting = true;

        do
        {
            if (result1?.Response == EDeleteAddOrClose.Close) return;

            if (result1?.Response == EDeleteAddOrClose.Delete && continueDeleting)
            {
                var vm = new AreYouSureDeleteSamplePointViewModel(result1.AssetName, result1.SamplePointName);

                var result = await AreYouSureDeleteSamplePointDialog.Handle(vm);

                if (result?.Response == EYesNo.No)
                {
                    continueDeleting = false;
                    continue;
                }

                var chosenSamplePoint = new SamplePoint { Name = result1.SamplePointName };

                IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();
                db.ReadFromDatabase();

                var assetSamplePointsObservable = db.AssetSamplePointsObservable;

                if (assetSamplePointsObservable != null)
                {
                    foreach (var assetSamplePoints in assetSamplePointsObservable)
                    {
                        if (assetSamplePoints.Asset?.AssetName != null && assetSamplePoints.Asset != null &&
                            result1.AssetName != null && assetSamplePoints.Asset.AssetName.Contains(result1.AssetName))
                        {
                            db.DeleteChosenSamplePoint(assetSamplePoints, chosenSamplePoint);
                            break;
                        }
                    }
                }
            }
            else if (result1?.Response == EDeleteAddOrClose.Add)
            {
                var vm2 = new AddNewSamplePointViewModel(result1.AssetName);

                var result2 = await SaveNewSamplePointDialog.Handle(vm2);

                IAssetSamplePointsDatabase db = new AssetSamplePointsDatabase();

                if (result1.AssetName != null)
                {
                    if (result2?.Name != null)
                    {
                        db.AddNewSamplePoint(result1.AssetName, result2.Name);
                    }
                }
            }

            vm1 = new AddOrDeleteSamplePointViewModel(asset);

            result1 = await AddOrDeleteSamplePointDialog.Handle(vm1);

            continueDeleting = true;
        }
        while (result1?.Response != EDeleteAddOrClose.Close);
    }

    public void ToggleSortOrder()
    {
        if (_assetSortOrder == ESortOrder.Unsorted)
        {
            _assetSortOrder = ESortOrder.Ascending;
        }
        else if (_assetSortOrder == ESortOrder.Ascending)
        {
            _assetSortOrder = ESortOrder.Descending;
        }
        else if (_assetSortOrder == ESortOrder.Descending)
        {
            _assetSortOrder = ESortOrder.Ascending;
        }

        RightWindowContent = new SettingsViewModel(OilDatabaseVersion, false, _assetSortOrder);
        _currentRightWindow = ERightWindow.Settings;
    }
    #endregion

    #region SavedTestResult
    public async Task BackToSavedTests(bool firstOptionChosen)
    {
        _firstOptionChosen = false;
        await SavedTests();
    }

    public async Task DiscardTest(OilTestResult? oilTestResult)
    {
        var vm = new AreYouSureDeleteOilTestResultViewModel(oilTestResult?.OilTest?.TestReferenceName);

        var result = await AreYouSureDeleteOilTestResultDialog.Handle(vm);

        if (result?.Response == EYesNo.Yes)
        {
            IOilTestResultDatabase db = new OilTestResultDatabase();
            if (oilTestResult != null) db.DeleteFromDatabase(oilTestResult);

            _firstOptionChosen = false;
            await SavedTests();
        }
    }

    public async Task CheckForAppUpdates()
    {
        try
        {
            var path = Environment.CurrentDirectory + "/" + "Updates" +
                       "/" + "WyUpdate/wyUpdate.exe";

            var appFile = new FileInfo(path);

            if (!appFile.Exists)
                throw new FileNotFoundException("Not Found");
            Updater.CheckForUpdates(appFile.FullName);
            //var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
            //.GetMessageBoxStandardWindow("Info", $"Successfully updated application version.");
            //await messageBoxStandardWindow.Show();
        }
        catch (Exception ex)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Error", ex.Message);
            await messageBoxStandardWindow.Show();
        }
    }

    public async Task CheckForDbUpdates()
    {
        try
        {
            var dlg = new OpenFileDialog()
            {
                Directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>() { new FileDialogFilter { Extensions = new List<string> { "oils" } } }
            };

            if (MotMainWindowView.Instance != null)
            {
                var path = await dlg.ShowAsync(MotMainWindowView.Instance);

                if (path is null) return;

                var dbFile = new FileInfo(path[0]);

                StreamReader? streamReader = null;
                string? versionOfNewDb = null;
                double? versionOfNewDbDouble = null;

                try
                {
                    streamReader = dbFile.OpenText();
                    var firstLine = await streamReader.ReadLineAsync();
                    var firstLineComponents = firstLine?.Split(',');
                    if (firstLineComponents != null) versionOfNewDb = firstLineComponents[0];
                    versionOfNewDbDouble = Convert.ToDouble(versionOfNewDb);
                }
                catch (Exception ex)
                {
                    var errorMessageBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error", ex.Message);
                    await errorMessageBox.Show();
                }
                finally
                {
                    streamReader?.Close();
                    streamReader?.Dispose();
                }

                var versionOfCurrentDbDouble = Convert.ToDouble(OilDatabase?.ActiveDatabase?.Version?.ToString());

                if (versionOfCurrentDbDouble < versionOfNewDbDouble)
                {
                    try
                    {
                        OilDatabase.Update(dbFile);

                        if (OilDatabase?.DefaultDatabase.Version != null)
                        {
                            ApplicationStuff.GlobalSettings.DbPromptGen2 = OilDatabase?.DefaultDatabase.Version;
                        }

                        await ApplicationStuff.GlobalSettings.SaveAsync(ApplicationStuff.GlobalSettingsSaveFile);

                        OilDatabaseVersion = OilDatabase?.ActiveDatabase?.Version?.ToString();

                        var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow("Info", $"Successfully updated database to version {OilDatabase?.ActiveDatabase?.Version}");
                        await messageBoxStandardWindow.Show();
                    }
                    catch (Exception ex)
                    {
                        var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow("Info", $"Error Updating. Message: {ex.Message}");
                        await messageBoxStandardWindow.Show();
                    }
                }
                else
                {
                    var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow("Info", $"UPDATE NOT DONE. New oil database version {versionOfNewDbDouble} is less than or equal to current oil version {OilDatabase?.ActiveDatabase?.Version}");
                    await messageBoxStandardWindow.Show();
                }
            }
        }
        catch (Exception ex)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Error", ex.Message);
            await messageBoxStandardWindow.Show();
        }
    }


    public async Task ExportData(OilTestResultForReporting? oilTestResultForReporting)
    {

        var dlg = new OpenFolderDialog()
        {
            Directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Title = "Open Folder Dialog"
        };

        if (MotMainWindowView.Instance != null)
        {
            var location = await SaveDialog.SaveTSV("Export TSV", GetFormattedFileName(oilTestResultForReporting.TestReferenceName));


            IExportDataHelper helper = new ExportAndCopyDataHelper(oilTestResultForReporting, location);
            var success = helper.WriteToFile();

            if (success)
            {
                var vm2 = new ExportDataSuccessViewModel(true);

                var result2 = await ExportDataSuccessDialog.Handle(vm2);

                if (result2?.Response == true)
                {
                    IOilTestResultDatabase db = new OilTestResultDatabase();
                    db.ReadFromDatabase();

                    if (db.OilTestResultsObservable != null)
                    {
                        var testResult = db.OilTestResultsObservable.Where(s => s.OilTest?.SelectedAssetName == oilTestResultForReporting?.AssetName);

                        RightWindowContent = new SavedTestResultViewModel(oilTestResultForReporting?.OilTestResult, testResult);
                    }
                }
            }
        }
        string GetFormattedFileName(string? name)
        {
            try
            {
                var then = Convert.ToDateTime(name);
                var time = then.Year + "_" + then.Month + "_" + then.Day + "_" + then.Hour + "_" + then.Minute + "_" + then.Second;
                return $"{time}.tsv";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

    }

    public void RepeatTest(OilTest? oilTestModel)
    {
        if (_oilTestCompleted != null)
        {
            _oilTestCompleted.PropertyChanged += DisplayOilTestResultDetails;

            RightWindowContent = new NewTestResultsViewModel(oilTestModel, _oilTestCompleted);

            LocatorClicked = false;
            NewTestClicked = true;

            Step2Future = false;
            Step2Current = false;
            Step2Past = true;

            Step3Future = false;
            Step3Current = false;
            Step3Past = true;

            Step4Future = false;
            Step4Current = false;
            Step4Past = true;

            Step4Future = false;
            Step4Current = false;
            Step4Past = true;

            Step5Future = false;
            Step5Current = false;
            Step5Past = true;

            RunTestClicked = true;

            Step2NameColor = "Black";
            Step3NameColor = "Black";
            Step4NameColor = "Black";
            Step5NameColor = "Black";
            Step6NameColor = "Black";

            MobileOilTestKitEnabled = false;
            NewTestEnabled = false;
            SavedTestsEnabled = false;
            SettingsEnabled = false;
            HelpEnabled = false;
        }
    }

    public async Task CopyToClipboard(OilTestResultForReporting? oilTestResultForReporting)
    {
        ICopyToClipboardHelper helper = new ExportAndCopyDataHelper(oilTestResultForReporting);
        var success = helper.CopyToClipboard().Result;

        if (success)
        {
            var vm = new ExportDataSuccessViewModel(false);

            var result = await ExportDataSuccessDialog.Handle(vm);

            if (result?.Response == true)
            {

                IOilTestResultDatabase db = new OilTestResultDatabase();
                db.ReadFromDatabase();

                if (db.OilTestResultsObservable != null)
                {
                    var testResult = db.OilTestResultsObservable.Where(s => s.OilTest?.SelectedAssetName == oilTestResultForReporting?.AssetName);

                    RightWindowContent = new SavedTestResultViewModel(oilTestResultForReporting?.OilTestResult, testResult);
                }
            }
        }
    }



    private async Task QuickStart()
    {
        string path = PATH + "QuickStart.pdf";

        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandardWindow("Error", "Failed to find file");
            await messageBoxStandardWindow.Show();
        }


    }


    private async Task UserManual()
    {
        string path = PATH + "FullUserManual.pdf";

        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandardWindow("Error", "Failed to find file");
            await messageBoxStandardWindow.Show();
        }
    }
    public void Print()
    {
    }
    #endregion

    #region Properties
    public OilDatabase? OilDatabase { get; set; }
    public string? Title { get; set; } = Constants.MotWindowTitle;
    public string? MotBlueColor { get; set; } = Constants.MotBlueColor;
    public string? MotBlueColorDarker { get; set; } = Constants.MotBlueColorDarker;
    public string? DefaultRightBackground { get; set; } = Constants.DefaultRightBackground;
    public string? GridLineColor { get; set; } = Constants.GridLineColor;
    public string? QuickStartGuideText { get; set; } = Constants.QuickStartGuideText;
    public string? FullUserManualText { get; set; } = Constants.FullUserManualText;
    public string? MobileOilTestKitText { get; set; } = Constants.MobileOilTestKitText;
    public string? AppNumber { get; } = Constants.AppVersion;
    public string AppNumberString => $"App: v{AppNumber}";
    public string? OilDatabaseVersion
    {
        get => _oilDatabaseVersion;
        set
        {
            this.RaiseAndSetIfChanged(ref _oilDatabaseVersion, value);
            OilDatabaseVersionString = $"Oil Database: v{_oilDatabaseVersion}";
        }
    }

    public string? OilDatabaseVersionString
    {
        get => _oilDatabaseVersionString;
        set => this.RaiseAndSetIfChanged(ref _oilDatabaseVersionString, value);
    }
    public string? BackButtonColor { get; set; } = Constants.BackButtonColor;
    public string? ConfirmationWarning { get; set; } = Constants.ConfirmationWarning;

    public ViewModelBase? RightWindowContent
    {
        get => _rightWindowContent;
        set => this.RaiseAndSetIfChanged(ref _rightWindowContent, value);
    }

    public bool LocatorClicked
    {
        get => _locatorClicked;
        set => this.RaiseAndSetIfChanged(ref _locatorClicked, value);
    }

    public bool NewTestClicked
    {
        get => _newTestClicked;
        set => this.RaiseAndSetIfChanged(ref _newTestClicked, value);
    }

    public bool SavedTestsClicked
    {
        get => _savedTestClicked;
        set => this.RaiseAndSetIfChanged(ref _savedTestClicked, value);
    }

    public bool SettingsClicked
    {
        get => _settingsClicked;
        set => this.RaiseAndSetIfChanged(ref _settingsClicked, value);
    }

    public bool HelpClicked
    {
        get => _helpClicked;
        set => this.RaiseAndSetIfChanged(ref _helpClicked, value);
    }

    public bool Step2Future
    {
        get => _step2Future;
        set => this.RaiseAndSetIfChanged(ref _step2Future, value);
    }

    public bool Step2Current
    {
        get => _step2Current;
        set => this.RaiseAndSetIfChanged(ref _step2Current, value);
    }

    public bool Step2Past
    {
        get => _step2Past;
        set => this.RaiseAndSetIfChanged(ref _step2Past, value);
    }

    public bool Step3Future
    {
        get => _step3Future;
        set => this.RaiseAndSetIfChanged(ref _step3Future, value);
    }

    public bool Step3Current
    {
        get => _step3Current;
        set => this.RaiseAndSetIfChanged(ref _step3Current, value);
    }

    public bool Step3Past
    {
        get => _step3Past;
        set => this.RaiseAndSetIfChanged(ref _step3Past, value);
    }

    public bool Step4Future
    {
        get => _step4Future;
        set => this.RaiseAndSetIfChanged(ref _step4Future, value);
    }

    public bool Step4Current
    {
        get => _step4Current;
        set => this.RaiseAndSetIfChanged(ref _step4Current, value);
    }

    public bool Step4Past
    {
        get => _step4Past;
        set => this.RaiseAndSetIfChanged(ref _step4Past, value);
    }

    public bool Step5Future
    {
        get => _step5Future;
        set => this.RaiseAndSetIfChanged(ref _step5Future, value);
    }

    public bool Step5Current
    {
        get => _step5Current;
        set => this.RaiseAndSetIfChanged(ref _step5Current, value);
    }

    public bool Step5Past
    {
        get => _step5Past;
        set => this.RaiseAndSetIfChanged(ref _step5Past, value);
    }

    public bool RunTestClicked
    {
        get => _runTestClicked;
        set => this.RaiseAndSetIfChanged(ref _runTestClicked, value);
    }

    public string? Step2NumberColor
    {
        get => _step2NumberColor;
        set => this.RaiseAndSetIfChanged(ref _step2NumberColor, value);
    }

    public string? Step2NameColor
    {
        get => _step2NameColor;
        set => this.RaiseAndSetIfChanged(ref _step2NameColor, value);
    }

    public string? Step3NumberColor
    {
        get => _step3NumberColor;
        set => this.RaiseAndSetIfChanged(ref _step3NumberColor, value);
    }

    public string? Step3NameColor
    {
        get => _step3NameColor;
        set => this.RaiseAndSetIfChanged(ref _step3NameColor, value);
    }

    public string? Step4NumberColor
    {
        get => _step4NumberColor;
        set => this.RaiseAndSetIfChanged(ref _step4NumberColor, value);
    }

    public string? Step4NameColor
    {
        get => _step4NameColor;
        set => this.RaiseAndSetIfChanged(ref _step4NameColor, value);
    }

    public string? Step5NumberColor
    {
        get => _step5NumberColor;
        set => this.RaiseAndSetIfChanged(ref _step5NumberColor, value);
    }

    public string? Step5NameColor
    {
        get => _step5NameColor;
        set => this.RaiseAndSetIfChanged(ref _step5NameColor, value);
    }

    public string? Step6NumberColor
    {
        get => _step6NumberColor;
        set => this.RaiseAndSetIfChanged(ref _step6NumberColor, value);
    }

    public string? Step6NameColor
    {
        get => _step6NameColor;
        set => this.RaiseAndSetIfChanged(ref _step6NameColor, value);
    }

    public bool MobileOilTestKitEnabled
    {
        get => _mobileOilTestKitEnabled;
        set => this.RaiseAndSetIfChanged(ref _mobileOilTestKitEnabled, value);
    }

    public bool NewTestEnabled
    {
        get => _newTestEnabled;
        set => this.RaiseAndSetIfChanged(ref _newTestEnabled, value);
    }

    public bool SavedTestsEnabled
    {
        get => _savedTestsEnabled;
        set => this.RaiseAndSetIfChanged(ref _savedTestsEnabled, value);
    }

    public bool SettingsEnabled
    {
        get => _settingsEnabled;
        set => this.RaiseAndSetIfChanged(ref _settingsEnabled, value);
    }

    public bool HelpEnabled
    {
        get => _helpEnabled;
        set => this.RaiseAndSetIfChanged(ref _helpEnabled, value);
    }

    #endregion

    #region Interactions
    public Interaction<AreYouSureViewModel, AreYouSureResponse?> AreYouSureDialog { get; set; } = null!;
    public Interaction<AddNewSamplePointViewModel, NewSamplePointResponse?> SaveNewSamplePointDialog { get; set; } = null!;
    public Interaction<AddNewAssetViewModel, NewAssetResponse?> SaveNewAssetDialog { get; set; } = null!;
    public Interaction<AddOrDeleteSamplePointViewModel, SamplePointResponse?> AddOrDeleteSamplePointDialog { get; set; } = null!;
    public Interaction<AreYouSureDeleteSamplePointViewModel, AreYouSureResponse?> AreYouSureDeleteSamplePointDialog { get; set; } = null!;
    public Interaction<AreYouSureDeleteAssetViewModel, AreYouSureResponse?> AreYouSureDeleteAssetDialog { get; set; } = null!;
    public Interaction<AreYouSureDeleteOilTestResultViewModel, AreYouSureResponse?> AreYouSureDeleteOilTestResultDialog { get; set; } = null!;
    public Interaction<ExportDataSuccessViewModel, OkResponse?> ExportDataSuccessDialog { get; set; } = null!;

    #endregion
}