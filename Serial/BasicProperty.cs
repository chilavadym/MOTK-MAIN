// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Runtime.InteropServices;

namespace Serial;

public class BasicProperty<T> : Property<T> where T : struct
{
    public BasicProperty(CommsInfo comms, char wakeCode, char opCode, ushort address, PropertyReader? reader = null, PropertyWriter? writer = null) :
        this(null, null, comms, wakeCode, opCode, address, reader, writer)
    { }
    public BasicProperty(IsSupportedDelegate? IsSupported, CommsInfo comms, char wakeCode, char opCode, ushort address, PropertyReader? reader = null, PropertyWriter? writer = null) :
        this(IsSupported, null, comms, wakeCode, opCode, address, reader, writer)
    { }

    public BasicProperty(string? name, CommsInfo comms, char wakeCode, char opCode, ushort address, PropertyReader? reader = null, PropertyWriter? writer = null) :
        this(null, name, comms, wakeCode, opCode, address, reader, writer)
    { }
    public BasicProperty(IsSupportedDelegate? IsSupported, string? name, CommsInfo comms, char wakeCode, char opCode, ushort address, PropertyReader? reader = null, PropertyWriter? writer = null) :
        base(IsSupported, name, comms, wakeCode, opCode, address, (byte)Marshal.SizeOf(typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T)), DefaultSerializer, DefaultDeserializer, reader, writer)
    { }

    public static Serializer DefaultSerializer { get; } = new Serializer((T value) =>
    {
        var isEnum = typeof(T).IsEnum;
        
        var castType = isEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T);
        
        var buffer = new byte[Marshal.SizeOf(castType)];
        
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        
        var pBuffer = handle.AddrOfPinnedObject();
        
        Marshal.StructureToPtr(isEnum ? Convert.ChangeType(value, castType) : value, pBuffer, false);
        
        handle.Free();

        if (castType != typeof(uint)) return buffer;
        
        return buffer.Reverse().ToArray();
    });
    public static Deserializer DefaultDeserializer { get; } = BasicReadOnlyProperty<T>.DefaultDeserializer;
}