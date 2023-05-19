using Common;
using Common.Units;
using MOTK.Enums;
using MOTK.Models;
using MOTK.Notifications;
using MOTK.Services;
using MOTK.Services.Interfaces;
using MOTK.Statics;
using ReactiveUI;
using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MOTK.ViewModels;

public class NewTestResultsViewModel : ViewModelBase
{
    private CancellationTokenSource? _cancelSensorCheck;
    private readonly OilTest? _oilTestModel;
    private readonly OilTestResult? _oilTestResult;
    private Reading? _sensorCondition;
    private double[]? _alerts;

    private bool _zeroPercent;
    private bool _tenPercent;
    private bool _twentyPercent;
    private bool _thirtyPercent;
    private bool _fortyPercent;
    private bool _fiftyPercent;
    private bool _sixtyPercent;
    private bool _seventyPercent;
    private bool _eightyPercent;
    private bool _ninetyPercent;
    private bool _hundredPercent;

    private readonly OilTestResultNeedDisplaying? _oilTestCompleted;

    public NewTestResultsViewModel()
    {

    }

    public NewTestResultsViewModel(OilTest? oilTestModel, OilTestResultNeedDisplaying? oilTestCompleted)
    {
        _oilTestModel = oilTestModel;
        _oilTestCompleted = oilTestCompleted;

        _oilTestResult = new OilTestResult(_oilTestModel)
        {
            OilTest = _oilTestModel,
            DateCreated = DateTime.Now

        };

        ZeroPercent = true;

        DoSensorStuff().ConfigureAwait(true);
    }

    private Reading? SensorCondition
    {
        get => _sensorCondition;
        set
        {
            var prev = _sensorCondition;
            _sensorCondition = value;

            if (!Equals(prev, _sensorCondition))
            {
                OnSensorConditionChanged();
            }
        }
    }

    private async Task DoSensorStuff()
    {
        Serial.Sensor? sensor = null;

        _cancelSensorCheck = new CancellationTokenSource();

        var ports = (ApplicationStuff.GlobalSettings.AllowThirdPartyPorts ? SerialPort.GetPortNames() : Serial.Common.GetPorts()).ToArray();

        if (ports.Length == 0)
        {
            return;
        }

        try
        {
            foreach (var port in ports)
            {
                var tempSensor = new Serial.Sensor(port);

                try
                {
                    ZeroPercent = false;
                    TenPercent = true;

                    await tempSensor.Initialize(_cancelSensorCheck.Token).ConfigureAwait(true);
                    sensor = tempSensor;

                    TenPercent = false;
                    TwentyPercent = true;

                    await Task.Delay(1000).ConfigureAwait(true);
                    break;
                }
                catch
                {
                    tempSensor.Dispose();
                }
            }

            if (sensor is not null)
            {
                var result = sensor.OilTable.WriteValue(_cancelSensorCheck.Token, _oilTestModel?.SelectedOil?.Profile, true);

                while (!result.IsCompleted)
                {
                    if (NinetyPercent is false)
                    {
                        TwentyPercent = false;
                        ThirtyPercent = true;

                        //await result.WaitAsync(new TimeSpan(0, 0, 4)).ConfigureAwait(true); // Sorry Ben. Couldn't get this to work.
                        await Task.Delay(4000).ConfigureAwait(true);

                        ThirtyPercent = false;
                        FortyPercent = true;
                        //await result.WaitAsync(new TimeSpan(0, 0, 4)).ConfigureAwait(true); // Sorry Ben. Couldn't get this to work.
                        await Task.Delay(4000).ConfigureAwait(true);

                        FortyPercent = false;
                        FiftyPercent = true;
                        //await result.WaitAsync(new TimeSpan(0, 0, 4)).ConfigureAwait(true); // Sorry Ben. Couldn't get this to work.
                        await Task.Delay(4000).ConfigureAwait(true);

                        FiftyPercent = false;
                        SixtyPercent = true;
                        //await result.WaitAsync(new TimeSpan(0, 0, 4)).ConfigureAwait(true); // Sorry Ben. Couldn't get this to work.
                        await Task.Delay(4000).ConfigureAwait(true);

                        SixtyPercent = false;
                        SeventyPercent = true;
                        //await result.WaitAsync(new TimeSpan(0, 0, 4)).ConfigureAwait(true); // Sorry Ben. Couldn't get this to work.
                        await Task.Delay(4000).ConfigureAwait(true);

                        SeventyPercent = false;
                        EightyPercent = true;
                        //await result.WaitAsync(new TimeSpan(0, 0, 4)).ConfigureAwait(true); // Sorry Ben. Couldn't get this to work.
                        await Task.Delay(4000).ConfigureAwait(true);

                        EightyPercent = false;
                        NinetyPercent = true;
                        //await result.WaitAsync(new TimeSpan(0, 0, 4)).ConfigureAwait(true); // Sorry Ben. Couldn't get this to work.
                        await Task.Delay(4000).ConfigureAwait(true);
                    }
                }

                if (sensor.Measurement is not null)
                {
                    var sensorCond = await sensor.Measurement.ReadValue(_cancelSensorCheck.Token).ConfigureAwait(true);

                    NinetyPercent = false;
                    HundredPercent = true;
                    await Task.Delay(1000).ConfigureAwait(true);

                    if (!sensorCond.OilCond.HasValue)
                        throw new NotSupportedException("The connected sensor does not support loss factor output which is required for the MOT software.");
                    if (double.IsInfinity(sensorCond.OilCond.Value.BaseValue) || double.IsNaN(sensorCond.OilCond.Value.BaseValue))
                        throw new ArgumentOutOfRangeException("The sensor returned a value that the software was not expecting, please contact your device manufacturer for support.");

                    var lossFactor = sensorCond.OilCond.Value.BaseValue;

                    _alerts = AlertPresets.GetAlertLevels(_oilTestModel?.SelectedOil?.Application);

                    if (_alerts is not null)
                    {
                        if (lossFactor >= _alerts[2])
                            sensorCond.AlertState = (int)EAlertNumber.Four;
                        else if (lossFactor >= _alerts[1])
                            sensorCond.AlertState = (int)EAlertNumber.Three;
                        else if (lossFactor >= _alerts[0])
                            sensorCond.AlertState = (int)EAlertNumber.Two;
                        else if (lossFactor >= _alerts[0] / 2.0)
                            sensorCond.AlertState = (int)EAlertNumber.One;
                        else if (lossFactor > -5)
                            sensorCond.AlertState = (int)EAlertNumber.Zero;
                        else
                            sensorCond.AlertState = (int)EAlertNumber.MinusOne;

                        SensorCondition = sensorCond;
                    }
                }
            }
        }
        catch (Exception)
        {
            // Ignored

        }
        finally
        {
            sensor?.Dispose();

            _cancelSensorCheck?.Dispose();
            _cancelSensorCheck = null;
        }
    }

    private void OnSensorConditionChanged()
    {
        if (SensorCondition is null) return;

        var cond = (SensorCondition.Value.OilCond ?? OilCondition.MaxValue).BaseValue;

        if (_oilTestResult is not null)
        {
            if (_alerts != null)
            {
                var eol = _alerts[1];
                var rul = 1.0 - (cond / eol);
                rul = Math.Max(0, Math.Min(1, rul));
                _oilTestResult.RemainingUsefulLife = rul.ToString("0%");
            }

            _oilTestResult.SensorCondition = SensorCondition;
            _oilTestResult.CanRepeat = true;
        }

        IOilTestResultDatabase db = new OilTestResultDatabase();

        if (_oilTestResult != null)
        {
            db.WriteToDatabase(_oilTestResult);

            if (_oilTestCompleted != null) _oilTestCompleted.OilTestResult = _oilTestResult;
        }
    }

    #region Percentage Image Properties
    public bool ZeroPercent
    {
        get => _zeroPercent;
        set => this.RaiseAndSetIfChanged(ref _zeroPercent, value);
    }

    public bool TenPercent
    {
        get => _tenPercent;
        set => this.RaiseAndSetIfChanged(ref _tenPercent, value);
    }

    public bool TwentyPercent
    {
        get => _twentyPercent;
        set => this.RaiseAndSetIfChanged(ref _twentyPercent, value);
    }

    public bool ThirtyPercent
    {
        get => _thirtyPercent;
        set => this.RaiseAndSetIfChanged(ref _thirtyPercent, value);
    }

    public bool FortyPercent
    {
        get => _fortyPercent;
        set => this.RaiseAndSetIfChanged(ref _fortyPercent, value);
    }

    public bool FiftyPercent
    {
        get => _fiftyPercent;
        set => this.RaiseAndSetIfChanged(ref _fiftyPercent, value);
    }

    public bool SixtyPercent
    {
        get => _sixtyPercent;
        set => this.RaiseAndSetIfChanged(ref _sixtyPercent, value);
    }

    public bool SeventyPercent
    {
        get => _seventyPercent;
        set => this.RaiseAndSetIfChanged(ref _seventyPercent, value);
    }

    public bool EightyPercent
    {
        get => _eightyPercent;
        set => this.RaiseAndSetIfChanged(ref _eightyPercent, value);
    }
    public bool NinetyPercent
    {
        get => _ninetyPercent;
        set => this.RaiseAndSetIfChanged(ref _ninetyPercent, value);
    }
    public bool HundredPercent
    {
        get => _hundredPercent;
        set => this.RaiseAndSetIfChanged(ref _hundredPercent, value);
    }

    #endregion
}