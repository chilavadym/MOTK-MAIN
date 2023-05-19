// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace Serial;

internal interface IReadField : IFieldBase
{
    Task Read(CancellationToken cancelToken);
}

internal interface IReadProperty<T> :
    IReadField
{
    Task<T> ReadValue(CancellationToken cancelToken);
    T? Actual { get; }

    event EventHandler ActualChanged;
}
