// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Runtime.InteropServices;

namespace Serial;

public class BasicReadOnlyProperty<T> : ReadOnlyProperty<T> where T : struct
{
    public BasicReadOnlyProperty(CommsInfo comms, char wakeCode, char opCode, ushort address, PropertyReader? reader = null) :
        this(null, null, comms, wakeCode, opCode, address, reader)
    { }

    public BasicReadOnlyProperty(string? name, CommsInfo comms, char wakeCode, char opCode, ushort address, PropertyReader? reader = null) :
        this(null, name, comms, wakeCode, opCode, address, reader ?? DefaultPropertyReader)
    { }
    public BasicReadOnlyProperty(IsSupportedDelegate? IsSupported, CommsInfo comms, char wakeCode, char opCode, ushort address, PropertyReader? reader = null) :
        this(IsSupported, null, comms, wakeCode, opCode, address, reader ?? DefaultPropertyReader)
    { }
    public BasicReadOnlyProperty(IsSupportedDelegate? IsSupported, string? name, CommsInfo comms, char wakeCode, char opCode, ushort address, PropertyReader? reader = null) :
        base(IsSupported, name, comms, wakeCode, opCode, address, (byte)Marshal.SizeOf(typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T)), DefaultDeserializer, reader)
    { }

    public static Deserializer DefaultDeserializer { get; } = (data) =>
    {
        var castType = typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T);
            
        var sizeOfType = Marshal.SizeOf(castType);

        if (data.Length < sizeOfType)
        {
            throw new ArgumentException($"{nameof(data.Length)} is too small to contain {castType}", nameof(data));
        }

        data = data.Take(sizeOfType).ToArray();

        if (castType == typeof(uint))
        {
            data = data.Reverse().ToArray();
        }

        var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            
        var pBuffer = handle.AddrOfPinnedObject();

        var value = (T)Marshal.PtrToStructure(pBuffer, castType)!;
            
        handle.Free();
            
        return value;
    };
}