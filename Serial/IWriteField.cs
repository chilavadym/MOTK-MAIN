// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace Serial;

internal interface IWriteField : IReadField
{
    Task Write(CancellationToken cancelToken, bool forceWrite = false);
    void Clear();
}

internal interface IWriteProperty<T> : IReadProperty<T>, IWriteField
{
    Task WriteValue(CancellationToken cancelToken, T? value, bool forceWrite = false);
    T? Target { get; set; }

    event EventHandler TargetChanged;
}