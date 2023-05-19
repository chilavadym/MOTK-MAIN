// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common;

namespace Serial;

public abstract class Device : IDisposable
{
    protected abstract char WakeCode { get; }

    protected Device(CommsInfo comms)
    {
        CommsInfo = comms;
        FwVersion = new BasicReadOnlyProperty<float>(nameof(FwVersion), comms, WakeCode, 'v', 0x00);
    }
    public virtual async Task Initialize(CancellationToken cancellationToken)
    {
        await FwVersion.Read(cancellationToken).ConfigureAwait(false);

        if (FwVersion.Actual < 2.0f)
            throw new NotSupportedException($"This device firmware version is not supported");

        if (SerialNumber != null) await SerialNumber.Read(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task Read(CancellationToken cancellationToken, IProgress<double>? progress = null)
    {
        progress?.Report(0);
        
        await Initialize(cancellationToken).ConfigureAwait(false);

        var readProps =
            from p in GetType().GetProperties()
            where p.PropertyType.GetInterfaces().Contains(typeof(IReadField)) && p.GetValue(this) is not null
            select p.GetValue(this) as IReadField;

        // The Inialize counts as step 1 so we add 1 to the count and progress value every time we report an update.
        // The adding 1 also prevents any divide by zero issues.
        var count = readProps.Count();
        
        var maxProg = count + 1.0;
        
        progress?.Report(1.0 / (count + 1.0));

        for (var i = 0; i < count; ++i)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var itemToRead = readProps.ElementAt(i);

            if (itemToRead.IsSupported)
            {
                await itemToRead.Read(cancellationToken).ConfigureAwait(false);
            }

            // +2.0 here to account for the fact that i isn't incremented until the start of the next loop
            progress?.Report((i + 2.0) / maxProg);
        }
    }
    public virtual async Task Write(CancellationToken cancelToken, bool forceWrite = false, IProgress<double>? progress = null)
    {
        progress?.Report(0);
        
        await Initialize(cancelToken).ConfigureAwait(false);

        var writeProps =
            from p in GetType().GetProperties()
            where p.PropertyType.GetInterfaces().Contains(typeof(IWriteField)) && p.GetValue(this) is not null
            select p.GetValue(this) as IWriteField;

        // The Inialize counts as step 1 so we add 1 to the count and progress value every time we report an update.
        // The adding 1 also prevents any divide by zero issues.
        var count = writeProps.Count();
        
        var maxProg = count + 1.0;
        
        progress?.Report(1.0 / maxProg);

        for (var i = 0; i < count; ++i)
        {
            cancelToken.ThrowIfCancellationRequested();

            var itemToWrite = writeProps.ElementAt(i);
            
            if (itemToWrite.IsSupported)
            {
                await itemToWrite.Write(cancelToken, forceWrite).ConfigureAwait(false);
            }

            // +2.0 here to account for the fact that i isn't incremented until the star the next loop
            progress?.Report((i + 2.0) / maxProg);
        }
    }
    public virtual void Clear()
    {
        var clearProps =
            from p in GetType().GetProperties()
            where p.PropertyType.GetInterfaces().Contains(typeof(IWriteField)) && p.GetValue(this) is not null
            select p.GetValue(this) as IWriteField;

        foreach (var prop in clearProps)
        {
            prop.Clear();
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            CommsInfo.Dispose();
        }
    }

    public string PortName {
        get => CommsInfo.PortName;
        set => CommsInfo.PortName = value;
    }
    public CommsInfo CommsInfo { get; protected set; }

    public BasicReadOnlyProperty<float> FwVersion { get; protected set; }
    public ReadOnlyProperty<uint>? SerialNumber { get; protected set; }

    public ReadOnlyProperty<Reading>? Measurement { get; protected set; }
}