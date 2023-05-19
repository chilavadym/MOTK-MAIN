// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common;
using System.Text;

namespace Serial;

public class ReadOnlyProperty<T> :
    IReadProperty<T>
{
    public delegate bool IsSupportedDelegate();
    public delegate T Deserializer(byte[] data);
    public delegate Task<T> PropertyReader(CancellationToken cancelToken, CommsInfo comms, ReadOnlyProperty<T> property);

    static ReadOnlyProperty()
    {
        DefaultPropertyReader = async (cancelToken, comms, property) =>
        {
            var request = new byte[] {
                (byte)property.WakeCode, //WakeCode
                0x09, //PacketLength
                0x00, //NodeID
                (byte)'R', //OpCode_Hi
                (byte)property.OpCode, //OpCode_Lo
                (byte)(property.Address >> 8), //Address_Hi
                (byte)(property.Address & byte.MaxValue), //Address_Lo
                property.Length, //Request Length
            };
                
            ushort checksum = 0xFFFF;
                
            foreach (byte b in request)
            {
                checksum -= b;
            }

            request = request.Concat(new byte[] {
                (byte)(checksum >> 8),
                (byte)(checksum & byte.MaxValue),
            }).ToArray();

            request = Hex.Encode(request);
            byte[] reply;

            for (int i = 0; i < Properties.Settings.Default.CommsAttempts; ++i)
            {
                cancelToken.ThrowIfCancellationRequested();
                try
                {
                    comms.DiscardBuffers();
                    await Common.CommsWrite(cancelToken, comms, request).ConfigureAwait(false);
                    reply = await Common.CommsRead(cancelToken, comms, (property.Length + 4) * 2).ConfigureAwait(false);
                    reply = Common.DecodePacket(Hex.Decode(reply));
                }
                catch
                {
                    if (i < Properties.Settings.Default.CommsAttempts - 1)
                        continue;
                    else
                        throw;
                }
                return property.Deserialize(reply);
            }
            throw new ArgumentOutOfRangeException("Serial port communication attempts is less than 1", nameof(Properties.Settings.Default.CommsAttempts));
        };
    }
    public ReadOnlyProperty(CommsInfo comms, char wakeCode, char opCode, ushort address, byte length, Deserializer deserializer, PropertyReader? reader = null) :
        this(null, null, comms, wakeCode, opCode, address, length, deserializer, reader)
    { }
    public ReadOnlyProperty(string? name, CommsInfo comms, char wakeCode, char opCode, ushort address, byte length, Deserializer deserializer, PropertyReader? reader = null) :
        this(null, name, comms, wakeCode, opCode, address, length, deserializer, reader)
    { }
    public ReadOnlyProperty(IsSupportedDelegate? IsSupported, CommsInfo comms, char wakeCode, char opCode, ushort address, byte length, Deserializer deserializer, PropertyReader? reader = null) :
        this(IsSupported, null, comms, wakeCode, opCode, address, length, deserializer, reader)
    { }
    public ReadOnlyProperty(IsSupportedDelegate? IsSupported, string? name, CommsInfo comms, char wakeCode, char opCode, ushort address, byte length, Deserializer deserializer, PropertyReader? reader = null)
    {
        _isSupported = IsSupported;
        Name = string.IsNullOrWhiteSpace(name) ? GetType().Name : name;
        Comms = comms;
        WakeCode = wakeCode;
        OpCode = opCode;
        Address = address;
        Length = length;
        Deserialize = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        Reader = reader ?? DefaultPropertyReader;
    }

    public event EventHandler? ActualChanged;
    protected void OnActualChanged(EventArgs e)
    {
        ActualChanged?.Invoke(this, e);
    }

    public async Task Read(CancellationToken cancellationToken)
    {
        await ReadValue(cancellationToken).ConfigureAwait(false);
    }
    public async Task<T> ReadValue(CancellationToken cancellationToken)
    {
        if (!IsSupported)
        {
            throw new NotSupportedException();
        }

        var msg = new StringBuilder(GetType().Name);

        if (!string.IsNullOrWhiteSpace(Name))
        {
            msg.Append($"[\"{Name}\"]");
        }

        msg.Append(" >> ");

        try
        {
            T result = await PerformRead(cancellationToken).ConfigureAwait(false);
            Debug.DefaultLogger.Log(msg + result!.ToString()); //-V3111
            Actual = result;
            return result;
        }
        catch (Exception ex)
        {
            Debug.DefaultLogger.Log(msg + ex.Message);
            throw;
        }
    }
    protected virtual async Task<T> PerformRead(CancellationToken cancellationToken)
    {
        return await Reader(cancellationToken, Comms, this).ConfigureAwait(false);
    }

    public static implicit operator T?(ReadOnlyProperty<T> property)
    {
        return Equals(property, default) ? default : property.Actual;
    }
    public override string ToString()
    {
        var value = Equals(Actual, null) ? "null" : Actual.ToString(); //-V3111

        if (string.IsNullOrWhiteSpace(Name)) return $"{{{value}}}";
        
        return $"{Name}={{{value}}}";
    }

    public char WakeCode { get; }
    public char OpCode { get; }
    public ushort Address { get; }
    public byte Length { get; }
    protected Deserializer Deserialize { get; }
    protected PropertyReader Reader { get; }
    protected CommsInfo Comms { get; }
    public string? Name { get; }

    private readonly IsSupportedDelegate? _isSupported;

    public bool IsSupported { get => _isSupported?.Invoke() ?? true; }

    private T? _actual;
    public virtual T? Actual
    {
        get => IsSupported ? _actual : throw new NotSupportedException();
        protected set
        {
            var prev = Actual;
            
            _actual = value;

            if (!Equals(prev, Actual))
            {
                OnActualChanged(EventArgs.Empty);
            }
        }
    }

    public static PropertyReader DefaultPropertyReader { get; }
}