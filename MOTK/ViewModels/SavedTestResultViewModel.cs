using Avalonia;
using Avalonia.Controls;
using MOTK.Enums;
using MOTK.Events;
using MOTK.Models;
using MOTK.Services;
using MOTK.Services.Interfaces;
using MOTK.Statics;
using Prism.Events;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
namespace MOTK.ViewModels;

public class SavedTestResultViewModel : ViewModelBase
{
    private readonly OilTestResult? _oilTestResult;
    private readonly IAddressUserDatabase _db;

    private const int RemainingUsefulLifeAlertThreshold = 11;
    private const int RemainingUsefulLifeCautionThreshold = 21;


    private bool _isGridVisible;

    public bool IsGridVisible
    {
        get { return _isGridVisible; }
        set => this.RaiseAndSetIfChanged(ref _isGridVisible, value);
    }


    private ViewModelBase? _currentViewModel;
    public ViewModelBase? CurrentViewModel
    {
        get { return _currentViewModel; }
        set
        {

            this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }
    }
    private IEnumerable<OilTestResult> _testResults;
    private string _dateCreated;

    public string DateCreated
    {
        get { return _dateCreated; }
        set => this.RaiseAndSetIfChanged(ref _dateCreated, value);
    }

    //Stores Control View Model ViewModel data as backing field
    public ViewModelBase? ViewModelStore { get; set; }
    private string? _graphButtonText;

    public string? GraphButtonText
    {
        get { return _graphButtonText; }
        set => this.RaiseAndSetIfChanged(ref _graphButtonText, value);
    }


    private int _sliderMinValue;

    public int SliderMinValue
    {
        get { return _sliderMinValue; }
        set { _sliderMinValue = value; }
    }

    private int _sliderMaxValue;

    public int SliderMaxValue
    {
        get { return _sliderMaxValue; }
        set { _sliderMaxValue = value; }
    }

    private int _sliderValue;

    public int SliderValue
    {
        get { return _sliderValue; }
        set => this.RaiseAndSetIfChanged(ref _sliderValue, value);
    }




    private bool _showUpArrow;
    private OilTestResultForReporting? _oilTestResultForReporting;
    private XAxisRange _selectedRange;

    public XAxisRange SelectedRange
    {
        get { return _selectedRange; }
        set
        {

            this.RaiseAndSetIfChanged(ref _selectedRange, value);
            @event.GetEvent<GraphEventArgs>().Publish(SelectedRange);
        }
    }

    private int _selectedValue;

    public int SelectedValue
    {
        get { return _selectedValue; }
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedValue, value);
            SelectedRange = (XAxisRange)value;
        }
    }

    public IEnumerable<KeyValuePair<XAxisRange, string>> XAxisRanges

    {
        get
        {
            return Enum.GetValues(typeof(XAxisRange))
            .Cast<XAxisRange>()
                .Select(e => new KeyValuePair<XAxisRange, string>(e, GetEnumDescription(e)));
        }
    }
    private KeyValuePair<XAxisRange, string> _selectedXAxisRange;

    public KeyValuePair<XAxisRange, string> SelectedXAxisRange
    {
        get { return _selectedXAxisRange; }
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedXAxisRange, value);
            SelectedRange = (XAxisRange)value.Key;
        }
    }

    Dictionary<DateTime, double>? oilConditions { get; set; }
    private IEventAggregator @event = Bootstrapper.GetRequiredService<IEventAggregator>(Locator.Current);
    public SavedTestResultViewModel()
    {
        _db = new AddressUserDatabase();
        _db.ReadFromDatabase();
    }
    public SavedTestResultViewModel(OilTestResult? oilTestResult, IEnumerable<OilTestResult> results)
    {
        _oilTestResult = oilTestResult;
        if (_testResults == null)
        {
            _testResults = results ?? new List<OilTestResult>();
        }

        if (oilTestResult != null) CanRepeat = oilTestResult.CanRepeat;
        _db = new AddressUserDatabase();
        _db.ReadFromDatabase();
        IsGridVisible = true;
        ShowInRecommendation = true;
        CanRepeat = oilTestResult.CanRepeat;
        if (_oilTestResult is not null)
        {
            GetGeneralCondition();

            GetObservationAndRecommendation();

            GetRemainingUsefulLifeIndicator();

            GetVisualIndicatorColor();

            GetOilConditionSlider();

            SaveOilTestResultsForReporting();
            InitializeTdnOrLF(_oilTestResult);
            var oilConditionValue = _oilTestResult.SensorCondition.Value.OilCond.Value.Value;
            var data = new GeneralSettingsDatabase();
            data.ReadFromDatabase();
            if (data == null || data.GeneralSettings == null)
            {
                return;
            }
            if (data != null && data.GeneralSettings.LossFactor)
            {
                IsLossFactor = true;
                MinValue = GeneralConditionInfo.MaxValue;
                MaxValue = GeneralConditionInfo.MinValue;
            }
            else
            {
                IsLossFactor = false;
                MinValue = GeneralConditionInfo.MaxValue;
                MaxValue = GeneralConditionInfo.MinValue;
            }

            if (IsLossFactor)
            {
                if (oilConditionValue > 45)
                {
                    oilConditionValue = 45;
                }
                if (oilConditionValue < -15)
                {
                    oilConditionValue = -15;
                }
                IndicatorMinValue = -15;
                IndicatorMaxValue = 45;
                SliderValue = Convert.ToInt32(oilConditionValue);
            }
            else
            {
                IndicatorMinValue = 0;
                IndicatorMaxValue = 1200;
                if (oilConditionValue > 1200)
                {
                    oilConditionValue = 1200;
                }
                if (oilConditionValue < 0)
                {
                    oilConditionValue = 0;
                }
                // Second state: 1200 to 0
                double normalizedPosition = (double)(oilConditionValue - 1200) / 1200; // Normalize to range 0-1

                // Reverse the normalized position
                double reversedPosition = 1 - normalizedPosition;

                // Map the reversed normalized position back to the desired range: 1200 to 0
                var reversedValue = (int)((1 - reversedPosition) * 1200);
                reversedValue = Math.Abs(reversedValue);
                if (reversedValue < 0)
                {
                    reversedValue = 0;
                }
                if (reversedValue > 1200)
                {
                    reversedValue = 1200;
                }

                SliderValue = reversedValue;

            }

        }
        GraphButtonText = "Table";
        oilConditions = GetGraphData();
        CurrentViewModel = new GraphControlViewModel(oilConditions);
        DateCreated = _oilTestResult?.DateCreated.ToString("dd/MM/yyyy");
    }

    private string GetEnumDescription(XAxisRange value)
    {
        var field = value.GetType().GetField(value.ToString());

        if (field != null)
        {
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attribute != null ? attribute.Description : value.ToString();
        }

        return string.Empty;
    }
    private Dictionary<DateTime, double> GetGraphData()
    {
        Dictionary<DateTime, double> graphValues = new Dictionary<DateTime, double>();

        if (_oilTestResult is not null)
        {

            if (_testResults is not null)
            {
                foreach (var result in _testResults)
                {
                    if (result.SensorCondition != null)
                    {
                        double? value = GetOilConditionOrLfValue(result);

                        if (value != null)
                        {
                            graphValues.Add(result.DateCreated, (double)value);
                        }
                    }

                }
            }
        }

        return graphValues;
    }

    private void ChangeView()
    {
        if (CurrentViewModel is GraphControlViewModel graph)
        {
            if (_testResults != null)
                CurrentViewModel = ViewModelStore is TableControlViewModel table
                    ? table
                    : new TableControlViewModel(_oilTestResult);
            GraphButtonText = "Graph";
            ViewModelStore = graph;
        }
        else if (CurrentViewModel is TableControlViewModel tablView)
        {
            if (oilConditions != null)
                CurrentViewModel = ViewModelStore is GraphControlViewModel graphView
                ? graphView
                    : new GraphControlViewModel(oilConditions);
            GraphButtonText = "Table";
            ViewModelStore = tablView;
        }
    }

    private async Task Print(ScrollViewer control)
    {
        try
        {
            IsGridVisible = false;
            Size maximumPageSize = new Size(2000, 2000);
            var fileName = await SaveDialog.Save("Chose destination", (OilTestResult.OilTest.TestReferenceName + ".pdf").Replace(':', '-'));

            // Replace the Button controls with a placeholder Border control
            var fullContent = from content in control.FindAllVisuals<ScrollViewer>()
                              select content.Content as Control;

            List<Control> controls = new List<Control>(fullContent);

            PrintHelper.ToFile(fileName, controls.Layout(maximumPageSize));

            //control.Content = backupContent;
            IsGridVisible = true;

            Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });


        }
        catch (Exception ex)
        {
            // Handle exceptions
            this.Log<SavedTestResultViewModel>().Error(ex.Message);
        }


    }
    [Obsolete("Used for removing buttons beofre printing")]
    private void RemoveButtons(Grid parentGrid)
    {
        for (int i = parentGrid.Children.Count - 1; i >= 0; i--)
        {
            IControl childControl = parentGrid.Children[i];
            if (childControl is StackPanel sp)
            {
                if (sp.Children.Count > 0)
                {
                    for (int d = sp.Children.Count - 1; d >= 0; d--)
                    {
                        if (sp.Children[d] is Button button)
                        {
                            int index = sp.Children.IndexOf(button);
                            var border = new Border
                            {
                                Width = button.Width,
                                Height = button.Height,
                                Margin = button.Margin,
                                Background = button.Background,
                            };
                            sp.Children[index] = border;
                        }
                    }
                }

            }
            else if (childControl is Grid childGrid)
            {
                RemoveButtons(childGrid);
            }
        }
    }
    private void InitializeTdnOrLF(OilTestResult result)
    {

        var db = new GeneralSettingsDatabase();
        db.ReadFromDatabase();

        if (db.GeneralSettings != null && db.GeneralSettings.LossFactor)
        {
            IsLossFactor = true;
            string? numberString = result.SensorCondition?.OilCond?.Value.ToString(CultureInfo.InvariantCulture);
            numberString = numberString?.Replace("TDN", "");
            double number = double.Parse(numberString ?? string.Empty);
            if (number < -15)
            {
                number = -15;
            }
            if (number > 45)
            {
                number = 45;
            }
            TdnOrLossFactor = Convert.ToInt32(number).ToString() + " %LF";


        }
        else if (db.GeneralSettings != null && db.GeneralSettings.TanDeltaNumber)
        {
            string? numberString = result.SensorCondition?.OilCond.ToString();
            numberString = numberString?.Replace("TDN", "");


            double number = double.Parse(numberString ?? string.Empty);
            if (number < 0)
            {
                number = 0;
            }
            if (number > 1200)
            {
                number = 1200;
            }

            TdnOrLossFactor = Convert.ToInt32(number).ToString() + " TDN";
        }
        else
        {
            string? numberString = result.SensorCondition?.OilCond.ToString();
            numberString = numberString?.Replace("TDN", "");
            double number = double.Parse(numberString ?? string.Empty);
            if (number < 0)
            {
                number = 0;

            }
            if (number > 1200)
            {
                number = 1200;
            }
            TdnOrLossFactor = Convert.ToInt32(number).ToString() + " TDN";
        }
    }

    // Based on settings gets Oil Condition or Loss Factor Value
    private double? GetOilConditionOrLfValue(OilTestResult result)
    {
        var db = new GeneralSettingsDatabase();
        db.ReadFromDatabase();

        if (db.GeneralSettings != null && db.GeneralSettings.LossFactor)
        {
            IsLossFactor = true;
            string? numberString = result.SensorCondition?.OilCond?.Value.ToString(CultureInfo.InvariantCulture);
            numberString = numberString?.Replace("TDN", "");
            double number = double.Parse(numberString ?? string.Empty);
            if (number < -15)
            {
                number = -15;
            }
            if (number > 45)
            {
                number = 45;
            }


            return number;
        }
        else if (db.GeneralSettings != null && db.GeneralSettings.TanDeltaNumber)
        {
            string? numberString = result.SensorCondition?.OilCond.ToString();
            numberString = numberString?.Replace("TDN", "");


            double number = double.Parse(numberString ?? string.Empty);
            if (number < 0)
            {
                number = 0;
            }
            if (number > 1200)
            {
                number = 1200;
            }
            return number;
        }
        else
        {
            string? numberString = result.SensorCondition?.OilCond.ToString();
            numberString = numberString?.Replace("TDN", "");
            double number = double.Parse(numberString ?? string.Empty);
            if (number < 0)
            {
                number = 0;

            }
            if (number > 1200)
            {
                number = 1200;
            }
            return number;
        }

    }
    private void GetGeneralCondition()
    {
        var result = GeneralConditionInfo.GetGeneralConditionInfo(_oilTestResult);

        if (result == EOilCondition.Alert)
        {
            AlertResult = true;
            TdnBackgroundColor = Constants.TdnRed;
        }
        else if (result == EOilCondition.Caution)
        {
            CautionResult = true;
            TdnBackgroundColor = Constants.TdnAmber;
        }
        else if (result == EOilCondition.Okay)
        {
            OkayResult = true;
            TdnBackgroundColor = Constants.TdnGreen;
        }
    }

    private void GetObservationAndRecommendation()
    {
        if (_oilTestResult?.SensorCondition is not null)
        {
            if (_oilTestResult.SensorCondition.Value.AlertState is not null)
            {
                var alert = _oilTestResult.SensorCondition.Value.AlertState;

                if ((int)alert == (int)EAlertNumber.MinusOne)
                {
                    MessageHeading = Constants.InAirHeading;
                    ShowInRecommendation = false;
                    ObservationMessage = Constants.InAirObservation;
                }
                else if ((int)alert == (int)EAlertNumber.Zero)
                {
                    MessageHeading = Constants.GoodHeading;
                    ObservationMessage = Constants.GoodObservation;
                    RecommendationMessage = Constants.GoodRecommendation;
                }
                else if ((int)alert == (int)EAlertNumber.One)
                {
                    MessageHeading = Constants.SlightWearHeading;
                    ObservationMessage = Constants.SlightWearObservation;
                    RecommendationMessage = Constants.SlightWearRecommendation;
                }
                else if ((int)alert == (int)EAlertNumber.Two)
                {
                    MessageHeading = Constants.WarningHeading;
                    ObservationMessage = Constants.WarningObservation;
                    RecommendationMessage = Constants.WarningRecommendation;
                }
                else if ((int)alert == (int)EAlertNumber.Three)
                {
                    MessageHeading = Constants.AlarmHeading;
                    ObservationMessage = Constants.AlarmObservation;
                    RecommendationMessage = Constants.AlarmRecommendation;
                }
                else if ((int)alert == (int)EAlertNumber.Four)
                {
                    MessageHeading = Constants.CriticalHeading;
                    ObservationMessage = Constants.CriticalObservation;
                    RecommendationMessage = Constants.CriticalRecommendation;
                }
            }
        }
    }

    private void GetRemainingUsefulLifeIndicator()
    {
        var v1 = _oilTestResult?.RemainingUsefulLife?.IndexOf('%');

        var index = v1 ?? default;

        var v2 = _oilTestResult?.RemainingUsefulLife?.Substring(0, index);

        var v3 = v2 == null ? string.Empty : Convert.ToString(v2);

        var percentage = 0;

        if (v3 != string.Empty)
        {
            percentage = Convert.ToInt32(v3);
        }

        if (percentage < RemainingUsefulLifeAlertThreshold)
        {
            RemainingUseFulLifeIsAlert = true;
            RemainingUseFulLifeIsCaution = false;
            RemainingUseFulLifeIsOkay = false;
        }
        else if (percentage < RemainingUsefulLifeCautionThreshold)
        {
            RemainingUseFulLifeIsAlert = false;
            RemainingUseFulLifeIsCaution = true;
            RemainingUseFulLifeIsOkay = false;
        }
        else
        {
            RemainingUseFulLifeIsAlert = false;
            RemainingUseFulLifeIsCaution = false;
            RemainingUseFulLifeIsOkay = true;
        }
    }

    private void GetVisualIndicatorColor()
    {
        if (_oilTestResult is not null)
        {
            switch (_oilTestResult.OilTest?.SelectedOilVisualCheck)
            {
                case EOilVisualCheck.Emulsified:
                case EOilVisualCheck.SolidDebris:
                    VisualIndicationIsAlert = true;
                    VisualIndicationIsCaution = false;
                    VisualIndicationIsOkay = false;
                    break;
                case EOilVisualCheck.CloudyHazy:
                    VisualIndicationIsAlert = false;
                    VisualIndicationIsCaution = true;
                    VisualIndicationIsOkay = false;
                    break;
                case EOilVisualCheck.ClearAndBright:
                case EOilVisualCheck.Dark:
                    VisualIndicationIsAlert = false;
                    VisualIndicationIsCaution = false;
                    VisualIndicationIsOkay = true;
                    break;
            }
        }
    }

    private void GetOilConditionSlider()
    {
        bool applicationIsAvailable = false;

        var availableApplications = GeneralConditionInfo.AvailableApplications;

        if (availableApplications is not null)
        {
            foreach (var application in availableApplications)
            {
                if (_oilTestResult?.OilTest?.SelectedOil?.Application.ToUpper() == application)
                {
                    applicationIsAvailable = true;
                    break;
                }
            }
        }

        if (applicationIsAvailable is false)
        {
            ScaleIsEngine = true; // This will be the default scale image
            return;
        }

        // In a future version, we should be able to dynamically pick an image name based
        // on application name provided that it matches with available applications. The
        // method here defeats the purpose of having an alert presets file that can be
        // dynamically modified.

        ScaleIsEngine = _oilTestResult?.OilTest?.SelectedOil?.Application.ToUpper() == "ENGINE";
        ScaleIsGearBox = _oilTestResult?.OilTest?.SelectedOil?.Application.ToUpper() == "GEAR";
        ScaleIsHydraulic = _oilTestResult?.OilTest?.SelectedOil?.Application.ToUpper() == "HYDRAULIC";
        ScaleIsCompressor = _oilTestResult?.OilTest?.SelectedOil?.Application.ToUpper() == "COMPRESSOR";
        ScaleIsTransformer = _oilTestResult?.OilTest?.SelectedOil?.Application.ToUpper() == "TRANSFORMER";
    }

    public void SelectActions()
    {
        if (ShowUpArrow)
        {
            ShowUpArrow = false;
        }
        else
        {
            ShowUpArrow = true;
        }
    }

    public void SaveOilTestResultsForReporting()
    {
        _oilTestResultForReporting = new OilTestResultForReporting
        {
            OilTestResult = _oilTestResult,
            SiteName = SiteName,
            AssetName = AssetId,
            AssetDescription = AssetDescription,
            SamplePointName = SelectedSamplePointName,
            TestReferenceName = _oilTestResult?.OilTest?.TestReferenceName,
            Application = Application,
            Hours = Hours,
            Manufacturer = Manufacturer,
            OilName = OilName,
            Viscosity = Viscosity,
            SensorSerialNumber = SensorSerialNumber,
            TdnOrLossFactor = TdnOrLossFactor,
            RemainingUsefulLife = RemainingUsefulLife,
            VisualIndication = VisualIndication
        };
    }

    #region Properties
    public OilTestResult? OilTestResult => _oilTestResult;
    public string TestId => $"Test ID: {_oilTestResult?.OilTest?.TestReferenceName}";
    public bool AlertResult { get; set; }
    public bool CautionResult { get; set; }
    public bool OkayResult { get; set; }

    private string? _tdnOrLossFactor;
    public string? TdnOrLossFactor
    {
        get => _tdnOrLossFactor;
        set
        {
            this.RaiseAndSetIfChanged(ref _tdnOrLossFactor, value);
        }
    }
    public string? MessageHeading { get; set; }
    public string? ObservationMessage { get; set; }
    public string? RecommendationMessage { get; set; }
    public bool ShowInRecommendation { get; set; }
    public string? AssetId => _oilTestResult?.OilTest?.SelectedAssetName;
    public string? AssetDescription => _oilTestResult?.OilTest?.SelectedAssetDescription;
    public string? Manufacturer => _oilTestResult?.OilTest?.SelectedOil?.Manufacturer;
    public string? SelectedSamplePointName => _oilTestResult?.OilTest?.SelectedSamplePointName;
    public string? OilName => _oilTestResult?.OilTest?.SelectedOil?.OilName;
    public string? Application => _oilTestResult?.OilTest?.SelectedOil?.Application;
    public string? Viscosity => _oilTestResult?.OilTest?.SelectedOil?.Viscosity.Value;
    public int? Hours => _oilTestResult?.OilTest?.OilHours;
    public string? SensorSerialNumber => _oilTestResult?.OilTest?.SensorSerialNumber;
    public string? SiteName => _db.AddressSettings?.SiteName;
    public string? SiteAddress => _db.AddressSettings?.SiteAddress;
    public string? RemainingUsefulLife => _oilTestResult?.RemainingUsefulLife;
    private bool _isLossFactor;

    public bool IsLossFactor
    {
        get { return _isLossFactor; }
        set
        {
            if (value)
            {
                IndicatorMinValue = -15;
                IndicatorMaxValue = 45;
            }
            else
            {
                IndicatorMinValue = 0;
                IndicatorMaxValue = 1200;
            }
            this.RaiseAndSetIfChanged(ref _isLossFactor, value);
        }
    }


    //public string RateOfChange => "830 TDN"; // Needs to be calculated
    public string? VisualIndication => _oilTestResult?.OilTest?.VisualCheck;

    //public string TimeToMaintenance => "2 Days"; // Needs to be calculated

    public bool RemainingUseFulLifeIsAlert { get; set; }
    public bool RemainingUseFulLifeIsCaution { get; set; }
    public bool RemainingUseFulLifeIsOkay { get; set; }

    public bool RateOfChangeIsAlert { get; set; }
    public bool RateOfChangeIsCaution { get; set; }
    public bool RateOfChangeIsOkay { get; set; }

    public bool VisualIndicationIsAlert { get; set; }
    public bool VisualIndicationIsCaution { get; set; }
    public bool VisualIndicationIsOkay { get; set; }

    public bool TimeToMaintenanceIsAlert { get; set; }
    public bool TimeToMaintenanceIsCaution { get; set; }
    public bool TimeToMaintenanceIsOkay { get; set; }

    public string? TdnBackgroundColor { get; set; }

    public bool ScaleIsEngine { get; set; }
    public bool ScaleIsGearBox { get; set; }
    public bool ScaleIsHydraulic { get; set; }
    public bool ScaleIsCompressor { get; set; }
    public bool ScaleIsTransformer { get; set; }
    public string? MaxValue { get; set; } = GeneralConditionInfo.MaxValue;
    public string? MinValue { get; set; } = GeneralConditionInfo.MinValue;
    public bool CanRepeat { get; set; }
    public bool ShowUpArrow
    {
        get => _showUpArrow;
        set => this.RaiseAndSetIfChanged(ref _showUpArrow, value);
    }

    public OilTestResultForReporting? OilTestResultForReporting => _oilTestResultForReporting;



    private int _indicatorMinValue;

    public int IndicatorMinValue
    {
        get { return _indicatorMinValue; }
        set
        {
            this.RaiseAndSetIfChanged(ref _indicatorMinValue, value);
        }
    }

    private int _indicatorMaxValue;

    public int IndicatorMaxValue

    {
        get { return _indicatorMaxValue; }
        set
        {
            this.RaiseAndSetIfChanged(ref _indicatorMaxValue, value);
        }
    }



    #endregion
}