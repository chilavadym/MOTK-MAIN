// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common;

namespace Serial;

partial class Sensor
{
    public partial class OilPolynomialProperty : Property<byte[]>
    {
        private const byte segmentLength = 37;

        public OilPolynomialProperty(Sensor sensor) :
            this(null, null, sensor)
        { }
        public OilPolynomialProperty(string? name, Sensor sensor) :
            this(null, name, sensor)
        { }
        public OilPolynomialProperty(IsSupportedDelegate? IsSupported, string? name, Sensor sensor) :
            base(IsSupported, name, sensor.CommsInfo, sensor.WakeCode, (char)EOpCodes.Config, 0, segmentLength, SerializeDeserialze, SerializeDeserialze, DefaultPropertyReader, DefaultPropertyWriter)
        { }

        protected new static PropertyWriter? DefaultPropertyWriter { get; } = new PropertyWriter(async (cancelToken, comms, property) =>
        {
            if (property is OilPolynomialProperty)
            {
#pragma warning disable IDE0020 // Suppress "Use pattern matching" (Rosilyn bug)
                var oilProp = (OilPolynomialProperty)property;
#pragma warning restore IDE0020

                if (oilProp.Target != null)
                {
                    var request = new byte[] {
                        (byte)oilProp.WakeCode,
                        (byte)(oilProp.Length + 0x09),
                        0x00,
                        (byte)'W',
                        (byte)EOpCodes.Polynomial,
                        0x00,
                        (byte)(oilProp.Address >> 8),
                        oilProp.Length,
                    }.Concat(oilProp.Target).ToArray();
                    
                    ushort checksum = 0xFFFF;

                    foreach (byte b in request)
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
                        throw new FormatException($"Expected 0 decoded data bytes, received {reply.Length}");
                }

                //If we got this far, we're good!
            }
            else
                throw new InvalidCastException($"{nameof(property)} must be of type {typeof(OilTableProperty)}.");
        });

        protected static byte[] SerializeDeserialze(byte[]? data)
        {
            if (data == null) return new byte[0];

            return data;
        }
    }
}