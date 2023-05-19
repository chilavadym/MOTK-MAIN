// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace Common;

public class ValueChangedEventArgs : EventArgs
{
    public ValueChangedEventArgs(string itemName, object? previousValue, object? currentValue)
    {
        ItemName = itemName;
        PreviousValue = previousValue;
        CurrentValue = currentValue;
    }

    public string ItemName { get; }
    public object? PreviousValue { get; }
    public object? CurrentValue { get; }
}