// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com


using Common;

namespace Serial
{
    public class Property<T> : ReadOnlyProperty<T>, IWriteProperty<T>
    {
        static Property()
        {
            DefaultPropertyWriter = async (cancelToken, comms, property) =>
            {
                var request = new byte[] {
                    (byte)property.WakeCode, //WakeCode
                    (byte)(property.Length + 0x09), //PacketLength
                    0x00, //NodeID
                    (byte)'W', //OpCode_Hi
                    (byte)property.OpCode, //OpCode_Lo
                    (byte)(property.Address >> 8), //Address_Hi
                    (byte)(property.Address & byte.MaxValue), //Address_Lo
                    property.Length, //Data Length
                }.Concat(property.Serialize(property.Target)).ToArray();
                
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
                
                for (var i = 0; !cancelToken.IsCancellationRequested && i < Properties.Settings.Default.CommsAttempts; ++i)
                {
                    try
                    {
                        comms.DiscardBuffers();
                        
                        await Common.CommsWrite(cancelToken, comms, request).ConfigureAwait(false);
                        
                        var reply = await Common.CommsRead(cancelToken, comms, 0x04 * 2).ConfigureAwait(false);
                        
                        reply = Hex.Decode(reply);
                        
                        reply = Common.DecodePacket(reply);

                        if (reply.Length != 0)
                        {
                            throw new FormatException($"Expected 0 decoded data bytes, received {reply.Length}");
                        }
                    }
                    catch
                    {
                        if (i < Properties.Settings.Default.CommsAttempts - 1)
                        {
                            continue;
                        }

                        throw;
                    }
                    return;
                }
            };
        }
        
        public delegate byte[] Serializer(T? value);
        
        public delegate Task PropertyWriter(CancellationToken cancelToken, CommsInfo comms, Property<T> property);

        public Property(CommsInfo comms, char wakeCode, char opCode, ushort address, byte length, Serializer serializer, Deserializer deserializer, PropertyReader? reader = null, PropertyWriter? writer = null) :
            this(null, null, comms, wakeCode, opCode, address, length, serializer, deserializer, reader, writer)
        { }
        public Property(IsSupportedDelegate? IsSupported, CommsInfo comms, char wakeCode, char opCode, ushort address, byte length, Serializer serializer, Deserializer deserializer, PropertyReader? reader = null, PropertyWriter? writer = null) :
            this(IsSupported, null, comms, wakeCode, opCode, address, length, serializer, deserializer, reader, writer)
        { }
        public Property(string? name, CommsInfo comms, char wakeCode, char opCode, ushort address, byte length, Serializer serializer, Deserializer deserializer, PropertyReader? reader = null, PropertyWriter? writer = null) :
            this(null, name, comms, wakeCode, opCode, address, length, serializer, deserializer, reader, writer)
        { }
        public Property(IsSupportedDelegate? IsSupported, string? name, CommsInfo comms, char wakeCode, char opCode, ushort address, byte length, Serializer serializer, Deserializer deserializer, PropertyReader? reader = null, PropertyWriter? writer = null) :
            base(IsSupported, name, comms, wakeCode, opCode, address, length, deserializer, reader)
        {
            Serialize = serializer;
            Writer = writer ?? DefaultPropertyWriter;
        }

        public event EventHandler? TargetChanged;
        protected void OnTargetChanged(EventArgs e)
        {
            Debug.DefaultLogger.Log($"{Name}.{nameof(Target)} << {Target}");
            TargetChanged?.Invoke(this, e);
        }

        public void Clear()
        {
            if (!IsSupported) return;
            
            var prev = Target;
            
            _targetSet = false;

            if (!Equals(prev, Target))
            {
                OnTargetChanged(EventArgs.Empty);
            }
        }

        public async Task Write(CancellationToken cancelToken, bool forceWrite = false)
        {
            if (!IsSupported)
            {
                throw new NotSupportedException();
            }

            var msg = GetType().Name;

            if (!string.IsNullOrWhiteSpace(Name))
            {
                msg += $"[\"{Name}\"]";
            }

            msg += " << ";

            try
            {
                if (forceWrite || !Equals(Actual, Target))
                {
                    await PerformWrite(cancelToken).ConfigureAwait(false);
                    Debug.DefaultLogger.Log(msg + (Target?.ToString() ?? "null")); //-V3111
                    Actual = Target;
                }
            }
            catch (Exception ex)
            {
                Debug.DefaultLogger.Log(msg + ex.Message);
                throw;
            }
        }
        public async Task WriteValue(CancellationToken cancelToken, T? target, bool forceWrite = false)
        {
            Target = target;
            await Write(cancelToken, forceWrite).ConfigureAwait(false);
        }

        protected virtual async Task PerformWrite(CancellationToken cancelToken)
        {
            await Writer(cancelToken, Comms, this).ConfigureAwait(false);
        }

        public override string ToString()
        {
            var value = Equals(Actual, null) ? "null" : Actual.ToString(); //-V3111
            
            var newValue = Equals(Target, null) ? "null" : Target.ToString(); //-V3111

            if (string.IsNullOrWhiteSpace(Name)) return $"{{{value} | {newValue}}}";

            return $"{Name}={{{value} | {newValue}}}";
        }

        protected Serializer Serialize { get; }
        
        protected PropertyWriter Writer { get; }

        private bool _targetSet;
        
        private T? _target;
        public virtual T? Target 
        {
            get => _targetSet ? _target : Actual;
            set
            {
                if (!IsSupported)
                {
                    throw new NotSupportedException();
                }

                var prev = Target;

                _targetSet = true;

                _target = value;

                if (!Equals(prev, Target))
                {
                    OnTargetChanged(EventArgs.Empty);
                }
            }
        }

        public static PropertyWriter DefaultPropertyWriter { get; }
    }
}