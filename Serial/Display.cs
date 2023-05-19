// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common;
using Common.Units;
using System.Runtime.CompilerServices;

namespace Serial;

public class Display : Device
{
    protected override char WakeCode { get; } = '?';
    public const int DefaultBaudRate = 115200;
    public const int LegacyBaudRate = 9600;
    public const uint LogPageSize = 256u;
    public const uint LogSize = 20u;
    public const uint LogSizeInt = 16u;
    public const uint LogsPerPage = 16u;
    public const uint LogsPerFrame = 64u;

    public byte PagesPerRequest {
        get => (byte)(SupportsBinaryMode ? 6 : 1);
    }
    public bool SupportsBinaryMode {
        get => FwVersion >= 3.3f;
    }

    public enum EOpCodes : byte
    {
        Config = (byte)'c',
        System = (byte)'s',
        Alarms = (byte)'a',
        Version = (byte)'v',
        DataPacket = (byte)'d',
        DataStream = (byte)'i',
        LogCount = (byte)'l',
        Reading = (byte)'r',
    }
    [Flags]
    public enum EConfigFlags : byte
    {
        Bluetooth = 0x01,
    }
    [Flags]
    public enum EUnits : byte
    {
        Celsius = 0x00,
        Farhenheit = 0x80,

        LossFactor = 0x00,
        OilQualityIndex = 0x20,
        TanDeltaNumber = 0x40,

        MaskTemperature = 0x80,
        MaskCondition = 0x60,
    }
    protected enum EMemLocation : ushort
    {
        PosTemperatureWarning = 0x04,   // Config page
        PosTemperatureAlarm = 0x08,     // Config page
        NegTemperatureWarning = 0x0C,   // Config page
        NegTemperatureAlarm = 0x10,     // Config page
        RateOfCondPeriod = 0x14,        // Config page
        ConfigFlags = 0x2D,             // Config page
        SerialNumber = 0x2E,            // Config page (3 Bytes BE)
        DisplayUnits = 0x33,            // Config page

        NegRateOfCondWarning = 0x00,    // Alarms page
        NegRateOfCondAlarm = 0x06,      // Alarms page
        PosConditionWarning = 0x0C,     // Alarms page
        PosConditionAlarm = 0x10,       // Alarms page
        NegConditionWarning = 0x14,     // Alarms page
        NegConditionAlarm = 0x18,       // Alarms page
        PosRateOfCondWarning = 0x1C,    // Alarms page
        PosRateOfCondAlarm = 0x20,      // Alarms page

        LogInterval = 0x00,             // System page
        TimeOffset = 0x02,              // System page

        LogCount = 0x00,                // LogCount OpCode
    }

    static Display()
    {
        TemperatureSerializer = val => BitConverter.GetBytes((float)val.BaseValue);
        TemperatureDeserializer = data => new Temperature(BitConverter.ToSingle(data, 0), Temperature.BaseUnit);
        ConditionSerializer = val => BitConverter.GetBytes((float)val.BaseValue);
        ConditionDeserializer = data => new OilCondition(BitConverter.ToSingle(data, 0), OilCondition.BaseUnit);
        RocSerializer = val => BitConverter.GetBytes((float)val.BaseValue);
        RocDeserializer = data => new RateOfCondition(BitConverter.ToSingle(data, 0), RateOfCondition.BaseUnit);
    }
    public Display(string PortName) :
        base(new CommsInfo(PortName, DefaultBaudRate))
    {
        //All common information
        SerialNumber = new ReadOnlyProperty<uint>(nameof(SerialNumber), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.SerialNumber, sizeof(uint),
            serial =>
            {
                if (Enumerable.SequenceEqual(serial, BitConverter.GetBytes(FwVersion)))
                    throw new IOException("OQDe startup delay");
                var buffer = new byte[] { 0 }.Concat(serial.Take(sizeof(uint) - 1)).Reverse().ToArray();
                return BitConverter.ToUInt32(buffer, 0);
            });

        Measurement = new ReadOnlyProperty<Reading>(nameof(Measurement), CommsInfo, WakeCode, (char)EOpCodes.Reading, (ushort)0x0000, 12,
            data => new Reading
            {
                Time = DateTime.Now,
                OilTemp = new Temperature(BitConverter.ToSingle(data, 0), Temperature.BaseUnit),
                AmbTemp = new Temperature(BitConverter.ToSingle(data, 4), Temperature.BaseUnit),
                OilCond = new OilCondition(BitConverter.ToSingle(data, 8), OilCondition.BaseUnit)
            });

        TimeOffset = new Property<TimeSpan>(nameof(TimeOffset), CommsInfo, WakeCode, (char)EOpCodes.System, (ushort)EMemLocation.TimeOffset, 4,
            offset =>
            {
                var datum = new DateTime(2000, 1, 1);
                var timeSpan = DateTime.Now + offset - datum;
                var numDays = (ushort)Math.Floor(timeSpan.TotalDays);
                var numMins = (ushort)Math.Round(timeSpan.TotalMinutes - (numDays * 1440.0), MidpointRounding.AwayFromZero);
                return BitConverter.GetBytes(numDays).Concat(BitConverter.GetBytes(numMins)).ToArray();
            },
            bytes =>
            {
                var datum = new DateTime(2000, 1, 1);
                var numDays = BitConverter.ToUInt16(bytes, 0);
                var numMins = BitConverter.ToUInt16(bytes, 2);
                var time = datum.AddDays(numDays).AddMinutes(numMins);
                return new TimeSpan(0, (int)Math.Round((time - DateTime.Now).TotalMinutes, MidpointRounding.AwayFromZero), 0);
            });

        LogInterval = new BasicProperty<short>(nameof(LogInterval), CommsInfo, WakeCode, (char)EOpCodes.System, (ushort)EMemLocation.LogInterval);
        
        LogCount = new BasicReadOnlyProperty<int>(nameof(LogCount), CommsInfo, WakeCode, (char)EOpCodes.LogCount, (ushort)EMemLocation.LogCount);
        
        LogDateStart = new ReadOnlyProperty<DateTime>(nameof(LogDateStart), CommsInfo, WakeCode, (char)EOpCodes.DataPacket, 0, (byte)LogSize,
            data => new DateTime(0),
            async (cancelToken, _, _) =>
            {
                if (await LogCount.ReadValue(cancelToken).ConfigureAwait(false) == 0)
                {
                    return new DateTime(0, DateTimeKind.Utc); // Get the empty date
                }
                    
                var vals = await PerformReadLogs(cancelToken, 0, 1).ConfigureAwait(false);

                if (vals == null) return DateTime.MinValue;

                return vals.First().DateTime;
            });

        LogDateEnd = new ReadOnlyProperty<DateTime>(nameof(LogDateEnd), CommsInfo, WakeCode, (char)EOpCodes.DataPacket, 0, (byte)LogSize,
            bytes => new DateTime(0),
            async (cancelToken, _, _) =>
            {
                if (await LogCount.ReadValue(cancelToken).ConfigureAwait(false) == 0)
                {
                    return new DateTime(0, DateTimeKind.Utc);
                }

                var vals = await PerformReadLogs(cancelToken, (uint)(LogCount.Actual - 1), 1).ConfigureAwait(false);

                if (vals == null) return DateTime.MinValue;
                    
                return vals.First().DateTime;
            });

        // Firmware Dependent Parameters
        ConfigFlags = new BasicProperty<EConfigFlags>(() => 3.0f <= FwVersion, nameof(ConfigFlags), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.ConfigFlags);
        RateOfCondPeriod = new BasicProperty<float>(() => 3.0f <= FwVersion, nameof(RateOfCondPeriod), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.RateOfCondPeriod);
        DisplayUnits = new BasicProperty<EUnits>(() => 3.0f <= FwVersion, nameof(DisplayUnits), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.DisplayUnits);

        PosTemperatureWarning = new Property<Temperature>(() => 3.0f <= FwVersion, nameof(PosTemperatureWarning), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.PosTemperatureWarning, sizeof(float), TemperatureSerializer, TemperatureDeserializer);
        PosTemperatureAlarm = new Property<Temperature>(() => 3.0f <= FwVersion, nameof(PosTemperatureAlarm), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.PosTemperatureAlarm, sizeof(float), TemperatureSerializer, TemperatureDeserializer);
        NegTemperatureWarning = new Property<Temperature>(() => 3.0f <= FwVersion, nameof(NegTemperatureWarning), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.NegTemperatureWarning, sizeof(float), TemperatureSerializer, TemperatureDeserializer);
        NegTemperatureAlarm = new Property<Temperature>(() => 3.0f <= FwVersion, nameof(NegTemperatureAlarm), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.NegTemperatureAlarm, sizeof(float), TemperatureSerializer, TemperatureDeserializer);

        PosConditionWarning = new Property<OilCondition>(() => 3.0f <= FwVersion, nameof(PosConditionWarning), CommsInfo, WakeCode, (char)EOpCodes.Alarms, (ushort)EMemLocation.PosConditionWarning, sizeof(float), ConditionSerializer, ConditionDeserializer);
        PosConditionAlarm = new Property<OilCondition>(() => 3.0f <= FwVersion, nameof(PosConditionAlarm), CommsInfo, WakeCode, (char)EOpCodes.Alarms, (ushort)EMemLocation.PosConditionAlarm, sizeof(float), ConditionSerializer, ConditionDeserializer);
        NegConditionWarning = new Property<OilCondition>(() => 3.0f <= FwVersion, nameof(NegConditionWarning), CommsInfo, WakeCode, (char)EOpCodes.Alarms, (ushort)EMemLocation.NegConditionWarning, sizeof(float), ConditionSerializer, ConditionDeserializer);
        NegConditionAlarm = new Property<OilCondition>(() => 3.0f <= FwVersion, nameof(NegConditionAlarm), CommsInfo, WakeCode, (char)EOpCodes.Alarms, (ushort)EMemLocation.NegConditionAlarm, sizeof(float), ConditionSerializer, ConditionDeserializer);

        PosRateOfCondWarning = new Property<RateOfCondition>(() => 3.0f <= FwVersion, nameof(PosRateOfCondWarning), CommsInfo, WakeCode, (char)EOpCodes.Alarms, (ushort)EMemLocation.PosRateOfCondWarning, sizeof(float), RocSerializer, RocDeserializer);
        PosRateOfCondAlarm = new Property<RateOfCondition>(() => 3.0f <= FwVersion, nameof(PosRateOfCondAlarm), CommsInfo, WakeCode, (char)EOpCodes.Alarms, (ushort)EMemLocation.PosRateOfCondAlarm, sizeof(float), RocSerializer, RocDeserializer);
        NegRateOfCondWarning = new Property<RateOfCondition>(() => 3.0f <= FwVersion, nameof(NegRateOfCondWarning), CommsInfo, WakeCode, (char)EOpCodes.Alarms, (ushort)EMemLocation.NegRateOfCondWarning, sizeof(float), RocSerializer, RocDeserializer);
        NegRateOfCondAlarm = new Property<RateOfCondition>(() => 3.0f <= FwVersion, nameof(NegRateOfCondAlarm), CommsInfo, WakeCode, (char)EOpCodes.Alarms, (ushort)EMemLocation.NegRateOfCondAlarm, sizeof(float), RocSerializer, RocDeserializer);
    }

    public virtual async IAsyncEnumerable<DisplayLog> DownloadLogsAsync([EnumeratorCancellation] CancellationToken cancelToken, IProgress<double>? progress = null)
    {
        await foreach (var log in DownloadLogsAsync(cancelToken, 0u, uint.MaxValue, progress).ConfigureAwait(false))
        {
            yield return log;
        }
    }

    public virtual async IAsyncEnumerable<DisplayLog> DownloadLogsAsync([EnumeratorCancellation] CancellationToken cancelToken, uint address, uint uCount, IProgress<double>? progress = null)
    {
        if (address > LogCount)
        {
            progress?.Report(1.0);
            yield break;
        }

        if (address + uCount > LogCount)
            uCount = (uint)LogCount.Actual - address;

        if (uCount == 0)
        {
            progress?.Report(1.0);
            yield break;
        }

        var conseqSectionsMissed = 0;
        
        progress?.Report(0.0);
        
        for (var addr = address; addr < uCount; addr += LogsPerPage * PagesPerRequest)
        {
            cancelToken.ThrowIfCancellationRequested();

            var count = LogsPerPage * PagesPerRequest;

            if (addr + count > address + uCount)
            {
                count = (address + uCount) - addr;
            }

            IEnumerable<DisplayLog>? results = null;
            
            try
            {
                results = await PerformReadLogs(cancelToken, addr, (byte)count).ConfigureAwait(false);
                conseqSectionsMissed = 0;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (++conseqSectionsMissed > Properties.Settings.Default.CommsAttempts)
                {
                    throw new IOException($"Too many consequitive failed comms attempts.", ex);
                }
            }

            if (results != null)
            {
                foreach (var log in results)
                {
                    yield return new DisplayLog(log) { Address = log.Address + addr };
                }
            }

            progress?.Report((addr + count) / (double)uCount);
        }
    }

    public virtual async IAsyncEnumerable<DisplayLog> DownloadLogsAsync([EnumeratorCancellation] CancellationToken CancelToken, DateTime DateBegin, DateTime DateEnd, IProgress<double>? Progress = null)
    {
        if (!SupportsBinaryMode)
            throw new NotSupportedException("This device does not support downloading logs via date-range.");
            
        if (DateBegin < LogDateStart)
            DateBegin = LogDateStart;
        else if (LogDateEnd < DateBegin)
            DateBegin = LogDateEnd;

        if (DateEnd < LogDateStart)
            DateEnd = LogDateStart;
        else if (LogDateEnd < DateEnd)
            DateEnd = LogDateEnd;
            
        var asyncResult = PerformReadLogStream(CancelToken, DateBegin, DateEnd, Progress);
        await foreach (var log in asyncResult)
            yield return log;
    }

    private async Task<IEnumerable<DisplayLog>?> PerformReadLogs(
        CancellationToken CancelToken,
        uint Address,
        byte LogCount
    ) {
        bool useBinaryMode = SupportsBinaryMode;

        for (int attempt = 1; attempt <= Properties.Settings.Default.CommsAttempts; ++attempt)
        {
            CancelToken.ThrowIfCancellationRequested();
            try
            {
                var bufferOut = new byte[] {
                    (byte)WakeCode, 0x0A, 0x00, 0x52, 0x64,
                    (byte)(Address >> 16), (byte)(Address >> 8), (byte)Address,
                    (byte)LogCount,
                };
                ushort checksum = 0xFFFF;
                foreach (byte b in bufferOut)
                    checksum -= b;
                bufferOut = Hex.Encode(bufferOut.Concat(BitConverter.GetBytes(checksum).Reverse()).ToArray());

                int expectedReadLength = (int)(4 + (LogCount * LogSize));
                if (!useBinaryMode)
                    expectedReadLength *= 2;

                await Common.CommsWrite(CancelToken, CommsInfo, bufferOut).ConfigureAwait(false);
                byte[] bufferIn = await Common.CommsRead(CancelToken, CommsInfo, expectedReadLength).ConfigureAwait(false);

                if (!useBinaryMode)
                    bufferIn = Hex.Decode(bufferIn);

                return DecodeLogPacket(bufferIn);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.DefaultLogger.Log($"{nameof(PerformReadLogs)}(Addr={Address}, Cnt={LogCount})[Try={(attempt + 1)}] >> FAILED, {ex.Message}");
                if (attempt < Properties.Settings.Default.CommsAttempts)
                    continue;
                throw new IOException($"Too many comms attempts, last error: {ex.Message}", ex);
            }
        }
        throw new IOException("Maximum comms attempts reached, no fail description provided.");
    }

    private async IAsyncEnumerable<DisplayLog> PerformReadLogStream(
        [EnumeratorCancellation] CancellationToken cancelToken,
        DateTime dateStart,
        DateTime dateEnd,
        IProgress<double>? progress
    ) {
        var epoch = new DateTime(1970, 1, 1);
        uint tsStart = (uint)(dateStart.Date - epoch).TotalDays;
        uint tsEnd= (uint)(dateEnd.Date - epoch).TotalDays;

        if (tsEnd < tsStart)
        {
            var tmp = tsEnd;
            tsEnd = tsStart;
            tsStart = tmp;
        }

        var bufferOut = new byte[] {
            (byte)WakeCode, 0x0C, 0x00, 0x52, 0x69,
            0x00, (byte)(tsStart >> 8), (byte)(tsStart),
            0x00, (byte)(tsEnd >> 8), (byte)(tsEnd),
        };
        
        ushort checksum = 0xFFFF;

        foreach (byte b in bufferOut)
        {
            checksum -= b;
        }

        bufferOut = Hex.Encode(bufferOut.Concat(BitConverter.GetBytes(checksum).Reverse()).ToArray());

        await Common.CommsWrite(cancelToken, CommsInfo, bufferOut).ConfigureAwait(false);
        
        var inputStream = Common.CommsReadStream(cancelToken, CommsInfo);

        await foreach(var log in DecodeLogStream(inputStream, progress))
        {
            yield return log;
        }

        progress?.Report(1.0);
    }

    private IEnumerable<DisplayLog>? DecodeLogPacket(byte[] Data)
    {
        if (Data.Length == 0)
            throw new TimeoutException("No data received");

        int startIndex = 0;
        while (Data[startIndex] != (byte)Common.EWakeCode.ReplyAck && ++startIndex < Data.Length)
        {
            // Do nothing, we're using the while loop to search for the start index
        }
        int index = startIndex + 1;

        if (index >= Data.Length)
            throw new FormatException("Wake character not found");

        var logCount = Data[index++];
        if (Data.Length - 4 < logCount * LogSize) // -1 for wake, -1 for logCount, -2 for checksum.
            throw new TimeoutException("Read timed out before message was completed");
        index = (int)(logCount * LogSize) + 2;

        ushort checksum = (ushort)((Data[index] << 8) | Data[index + 1]);
        while (--index >= startIndex)
            checksum += Data[index];
        if (checksum != 0xFFFF)
            throw new FormatException($"Checksum mismatch, difference = {((ushort)(checksum - 0xFFFF)):X4}");

        var results = new List<DisplayLog>();
        for (int i = 0; i < logCount; ++i)
        {
            var offset = (int)(startIndex + 2 + i * LogSize);
            results.Add(new DisplayLog(
                (uint)i + 1,
                Util.Uint3B(Data, offset),
                Util.Float3B(Data, offset + 6),
                Util.Float3B(Data, offset + 9),
                Util.Float3B(Data, offset + 12),
                Util.Float3B(Data, offset + 3),
                Util.Float3B(Data, offset + 15),
                BitConverter.ToUInt16(Data.Skip(offset + 18).Take(sizeof(ushort)).Reverse().ToArray(), 0)
            ));
        }

        return results;
    }

    private async IAsyncEnumerable<DisplayLog> DecodeLogStream(
        IAsyncEnumerable<byte> inputStream,
        IProgress<double>? progress = null
    ) {
        int state = 0;
        // 0 = Searching for Wake char
        // 1 = Parse log count for this frame
        // 2-3 = Parse remaining frame count
        // 4-19 = Parse datalog, then reset to 0/4 depending on cntFrame

        uint seenLogs = 0;
        uint seenFrames = 0;
        byte cntLogs = 0;
        ushort cntFrame = 0;
        var buffer = new byte[LogSizeInt];

        await foreach(byte b in inputStream)
        {
            switch (state)
            {
                case 0:
                    if (b != (byte)Common.EWakeCode.ReplyAck)
                        continue; // Don't allow state to increment at bottom of loop

                    ++seenFrames;
                    break;
                case 1:
                    cntLogs = b;
                    break;
                case 2:
                    cntFrame = (ushort)(b << 8);
                    break;
                case 3:
                    cntFrame |= b;

                    if (cntLogs == 0xFE || cntFrame == 0xFEFE)
                    {
                        // End of transmission
                        yield break;
                    }
                    else if (cntLogs == 0x00 || cntLogs == 0xFF)
                    {
                        // Frame contains no data
                        if (seenFrames >= cntFrame)
                            yield break; // If last frame, end enumeration

                        // Else reset state to parse next frame
                        state = 0;
                        continue;
                    }
                        
                    // Continue to parse the data from the frame
                    break;
                case 3 + (int)LogSizeInt:
                    buffer[LogSizeInt - 1] = b;

                    DisplayLog? log = null;
                    try
                    {
                        log = new DisplayLog(
                            ++seenLogs,
                            Util.Uint3B(buffer, 0),
                            Util.Float2B(buffer, 6),
                            Util.Float2B(buffer, 8),
                            Util.Float2B(buffer, 10),
                            Util.Float2B(buffer, 4),
                            Util.Float2B(buffer, 12),
                            BitConverter.ToUInt16(buffer.Skip(14).Take(sizeof(ushort)).Reverse().ToArray(), 0)
                        );
                    }
                    catch
                    {
                        // Ignore
                    }

                    if (log.HasValue)
                        yield return log.Value;

                    if (--cntLogs == 0)
                    {
                        if (seenFrames >= cntFrame)
                            yield break; // If last frame, end enumeration

                        progress?.Report(seenLogs / (double)(cntFrame * LogsPerFrame));
                        state = 0;
                    }
                    else
                        state = 4;

                    continue; // Don't allow state to increment at bottom of loop
                default:
                    buffer[state - 4] = b;
                    break;
            }
                
            ++state;
        }
    }

    private static Property<Temperature>.Serializer TemperatureSerializer { get; }
    private static Property<Temperature>.Deserializer TemperatureDeserializer { get; }
    private static Property<OilCondition>.Serializer ConditionSerializer { get; }
    private static Property<OilCondition>.Deserializer ConditionDeserializer { get; }
    private static Property<RateOfCondition>.Serializer RocSerializer { get; }
    private static Property<RateOfCondition>.Deserializer RocDeserializer { get; }

    public Property<TimeSpan> TimeOffset { get; }
    public BasicProperty<short> LogInterval { get; }
    public BasicReadOnlyProperty<int> LogCount { get; }
    public ReadOnlyProperty<DateTime> LogDateStart { get; }
    public ReadOnlyProperty<DateTime> LogDateEnd { get; }

    public BasicProperty<EConfigFlags> ConfigFlags { get; }
    public BasicProperty<EUnits> DisplayUnits { get; }
    public BasicProperty<float> RateOfCondPeriod { get; }

    public double RateOfCondTimeDivisorOld { get => new TimeSpan(30, 0, 0, 0).TotalMinutes / RateOfCondPeriod.Actual; }
    public double RateOfCondTimeDivisorNew { get => new TimeSpan(30, 0, 0, 0).TotalMinutes / RateOfCondPeriod.Target; }

    public Property<Temperature> PosTemperatureWarning { get; protected set; }
    public Property<Temperature> PosTemperatureAlarm { get; protected set; }
    public Property<Temperature> NegTemperatureWarning { get; protected set; }
    public Property<Temperature> NegTemperatureAlarm { get; protected set; }

    public Property<OilCondition> PosConditionWarning { get; protected set; }
    public Property<OilCondition> PosConditionAlarm { get; protected set; }
    public Property<OilCondition> NegConditionWarning { get; protected set; }
    public Property<OilCondition> NegConditionAlarm { get; protected set; }

    public Property<RateOfCondition> PosRateOfCondWarning { get; protected set; }
    public Property<RateOfCondition> PosRateOfCondAlarm { get; protected set; }
    public Property<RateOfCondition> NegRateOfCondWarning { get; protected set; }
    public Property<RateOfCondition> NegRateOfCondAlarm { get; protected set; }
}