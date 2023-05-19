// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common;

namespace Serial;

public class AlertGroup<T> where T : struct
{
    public AlertGroup()
    {
        _posWarning = default;
        _posAlarm = default;
        _negWarning = default;
        _negAlarm = default;
    }
    public AlertGroup(AlertGroup<T> copy)
    {
        PositiveWarning = copy.PositiveWarning;
        PositiveAlarm = copy.PositiveAlarm;
        NegativeWarning = copy.NegativeWarning;
        NegativeAlarm = copy.NegativeAlarm;
    }
    public AlertGroup(T positiveWarning, T positiveAlarm, T negativeWarning, T negativeAlarm)
    {
        _posWarning = positiveWarning;
        _posAlarm = positiveAlarm;
        _negWarning = negativeWarning;
        _negAlarm = negativeAlarm;
    }

    public T PositiveWarning {
        get => _posWarning;
        set {
            var prev = PositiveWarning;
            _posWarning = value;
            if (!Equals(PositiveWarning, prev))
                OnPositiveWarningChanged(prev);
        }
    }
    public T PositiveAlarm {
        get => _posAlarm;
        set {
            var prev = PositiveAlarm;
            _posAlarm = value;
            if (!Equals(PositiveAlarm, prev))
                OnPositiveAlarmChanged(prev);
        }
    }
    public T NegativeWarning {
        get => _negWarning;
        set {
            var prev = NegativeWarning;
            _negWarning = value;
            if (!Equals(NegativeWarning, prev))
                OnNegativeWarningChanged(prev);
        }
    }
    public T NegativeAlarm {
        get => _negAlarm;
        set {
            var prev = NegativeAlarm;
            _negAlarm = value;
            if (!Equals(NegativeAlarm, prev))
                OnNegativeAlarmChanged(prev);
        }
    }

    void OnPositiveWarningChanged(T previous)
    {
        PositiveWarningChanged?.Invoke(this, new ValueChangedEventArgs(nameof(PositiveWarning), previous, PositiveWarning));
    }
    void OnPositiveAlarmChanged(T previous)
    {
        PositiveAlarmChanged?.Invoke(this, new ValueChangedEventArgs(nameof(PositiveAlarm), previous, PositiveAlarm));
    }
    void OnNegativeWarningChanged(T previous)
    {
        NegativeWarningChanged?.Invoke(this, new ValueChangedEventArgs(nameof(NegativeWarning), previous, NegativeWarning));
    }
    void OnNegativeAlarmChanged(T previous)
    {
        NegativeAlarmChanged?.Invoke(this, new ValueChangedEventArgs(nameof(NegativeAlarm), previous, NegativeAlarm));
    }

    public event EventHandler<ValueChangedEventArgs>? PositiveWarningChanged;
    public event EventHandler<ValueChangedEventArgs>? PositiveAlarmChanged;
    public event EventHandler<ValueChangedEventArgs>? NegativeWarningChanged;
    public event EventHandler<ValueChangedEventArgs>? NegativeAlarmChanged;

    private T _posWarning;
    private T _posAlarm;
    private T _negWarning;
    private T _negAlarm;
}