// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common;

namespace Serial;

partial class Sensor
{
    public partial class OilTableProperty : Property<byte[]>
    {
        private const byte NumSegments = 10;
        private const byte SegmentLength = 37;
        private byte CurrentSegment { get; set; } = 0;

        public OilTableProperty(Sensor sensor) :
            this(null, null, sensor)
        { }
        public OilTableProperty(string? name, Sensor sensor) :
            this(null, name, sensor)
        { }
        public OilTableProperty(IsSupportedDelegate? IsSupported, string? name, Sensor sensor) :
            base(IsSupported, name, sensor.CommsInfo, sensor.WakeCode, (char)EOpCodes.Table, 0, SegmentLength, SerializeDeserialze, SerializeDeserialze, DefaultPropertyReader, DefaultPropertyWriter)
        { }

        protected override async Task<byte[]> PerformRead(CancellationToken cancelToken)
        {
            await Task.Delay(0, cancelToken).ConfigureAwait(false);
            return null!;
        }
        protected override async Task PerformWrite(CancellationToken cancelToken)
        {
            for (CurrentSegment = 0; !cancelToken.IsCancellationRequested && CurrentSegment < NumSegments; ++CurrentSegment)
            {
                for (int i = 0; !cancelToken.IsCancellationRequested && Properties.Settings.Default.CommsAttempts == 0 || i < Properties.Settings.Default.CommsAttempts; ++i)
                {
                    try
                    {
                        await Writer(cancelToken, Comms, this).ConfigureAwait(false);
                        break;
                    }
                    catch
                    {
                        if (i < Properties.Settings.Default.CommsAttempts - 1)
                            continue;
                        throw;
                    }
                }
            }
        }

        protected new static PropertyReader? DefaultPropertyReader { get; } = async (cancelToken, _, _) =>
        {
            await Task.Delay(0, cancelToken).ConfigureAwait(false);
            
            return null!;
        };
        
        protected new static PropertyWriter? DefaultPropertyWriter { get; } = async (cancelToken, comms, property) =>
        {
            if (property is OilTableProperty)
            {
#pragma warning disable IDE0020 // Suppress "Use pattern matching" (Rosilyn bug)
                var oilProp = (OilTableProperty)property;
#pragma warning restore IDE0020

                if (oilProp.Target != null)
                {
                    var request = new byte[] {
                        (byte)oilProp.WakeCode,
                        (byte)(oilProp.Length + 0x09),
                        0x00,
                        (byte)'W',
                        (byte)oilProp.OpCode,
                        0x00,
                        oilProp.CurrentSegment,
                        oilProp.Length,
                    }.Concat(oilProp.Target.Skip(oilProp.CurrentSegment * (oilProp.Length + 1)).Take(oilProp.Length)).ToArray();
                    
                    ushort checksum = 0xFFFF;
                    
                    foreach (var b in request)
                    {
                        checksum -= b;
                    }

                    request = request.Concat(new[] {
                        (byte)(checksum >> 8),
                        (byte)(checksum & byte.MaxValue),
                    }).ToArray();
                    
                    request = Hex.Encode(request);

                    await Common.CommsWrite(cancelToken, comms, request).ConfigureAwait(false);
                        
                    var reply = await Common.CommsRead(cancelToken, comms, 0x04 * 2).ConfigureAwait(false);
                        
                    reply = Hex.Decode(reply);
                        
                    reply = Common.DecodePacket(reply);

                    if (reply.Length != 0)
                    {
                        throw new FormatException($"Expected 0 decoded data bytes, received {reply.Length}");
                    }
                }
            }
            else
            {
                throw new InvalidCastException($"{nameof(property)} must be of type {typeof(OilTableProperty)}.");
            }
        };

        protected static byte[] SerializeDeserialze(byte[]? data)
        {
            if (data == null) return new byte[0];
                
            return data;
        }
    }
}