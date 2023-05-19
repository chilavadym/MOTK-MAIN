// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace Common;

public class ValueMutatedEventArgs : EventArgs
{
    public ValueMutatedEventArgs(object source, ValueMutatedEventArgs mutationInner, ValueMutatedEventArgs innerMutation, ValueChangedEventArgs innerValueChange)
    {
        Source = source;
        InnerMutation = mutationInner ?? throw new ArgumentNullException(nameof(mutationInner));
        InnerValueChange = innerValueChange;
    }
    public ValueMutatedEventArgs(object source, ValueChangedEventArgs valueChangeInner, ValueMutatedEventArgs innerMutation, ValueChangedEventArgs innerValueChange)
    {
        Source = source;
        InnerValueChange = valueChangeInner ?? throw new ArgumentNullException(nameof(valueChangeInner));
        InnerMutation = innerMutation;
    }

    public object Source { get; }
    public ValueMutatedEventArgs InnerMutation { get; }
    public ValueChangedEventArgs InnerValueChange { get; }
}