// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common;
using Common.Units;
using System.Text;

namespace Serial;

public partial class Sensor : Device
{
    public partial class OilTableProperty { }
    public partial class OilPolynomialProperty { }

    protected override char WakeCode { get; } = '!';
    public const int DefaultBaudRate = 9600;
    public const int SensorNameMaxBytes = 32;

    [Flags]
    public enum ESerialType : byte
    {
        TanDeltaOnRs232 = 0,
        TanDeltaOnRs485 = 1,
        CanOpenOnCanbus = 2,
        ModbusOnRs485 = 3,
        ProfibusOnRs485 = 4,
        J1939OnCanbus = 5,

        Default = TanDeltaOnRs485,

        Flag_CanOpenSelfStart = 0x10,
    }
    public enum EBaudRateRs485 : byte
    {
        Baud9k600 = 0,
        Baud19k20,
        Baud38k40,
        Baud57k60,
        Baud115k2,
        Baud250k0,
        Baud500k0,
        Baud1000k,

        Default = Baud9k600,
    }
    public enum EBaudRateCanbus : byte
    {
        Baud1000k = 0,
        Baud800k,
        Baud500k,
        Baud250k = Baud500k + 2,
        Baud125k,
        Baud50k,
        Baud20k,

        Default = Baud125k,
    }
    public enum EBaudRateJ1939 : byte //-V3059
    {
        Baud500k = EBaudRateCanbus.Baud500k,
        Baud250k = EBaudRateCanbus.Baud250k,

        Default = Baud250k,
    }
    [Flags]
    public enum EFeatures : ushort
    {
        Comms_TanDelta = 0x01,
        Comms_ModbusRtu = 0x02,
        Comms_CanOpen = 0x04,
        Comms_J1939 = 0x08,
        // 0x10, 0x20, 0x40 = RESERVED
        Comms_CurrentLoop = 0x80,

        OilCondition = 0x0100,
        AlertState = 0x0200,
        RemainingLife = 0x0400,
        EncryptOutput = 0x0800,
        RateOfChange = 0x1000,
        // 0x2000, 0x4000, 0x8000 = RESERVED
    }

    [Flags]
    public enum EConfigFlags : byte
    {
        UseAmbTempForSensorNorm = 0x01,
        DisableOscTuning = 0x02,
        ZeroOhmThermResistor = 0x04,
        DisableCondOnLowTemp = 0x08,
    }
    public enum EOpCodes : byte
    {
        Config = (byte)'c',
        Polynomial = (byte)'p',
        Table = (byte)'t',
        Version = (byte)'v',
        Reading = (byte)'r',
        SecondConfig = (byte)'s',
    }
    protected enum EMemLocation : ushort
    {
        CalZero = 0x00,
        CalGain = 0x28,
        NodeId = 0x2C,
        SerialType = 0x2D,
        EndOfLife = 0x32,
        FilterSpikeReject = 0x36,
        FilterHysteresis = 0x3A,
        PassVariance = 0x3E,
        SerialNumber = 0x4E,
        OilPolynomial = 0x56,
        SensorName = 0xAF,
        FilterRollingAvg = 0xCF,
        BaudRate = 0xD0,
        BaudRateCanbus = 0xD1,
        ConfigBits = 0xD3,
        OilNumber = 0xD9,
        Varient = 0x2FE,
    }

    public Sensor(string portName) :
        this(portName, DefaultBaudRate)
    { }
    public Sensor(string portName, int baudRate) :
        base(new CommsInfo(portName, baudRate))
    {
        // Readers & Writers
        var oilNumberReader = BasicProperty<OilInfo.SerialNumber>.DefaultPropertyReader;
        
        var oilNumberWriter = new BasicProperty<OilInfo.SerialNumber>.PropertyWriter(
            async (cancelToken, comms, prop) =>
            {
                if (FwVersion.Actual < 3.0f)
                {
                    await BasicProperty<OilInfo.SerialNumber>.DefaultPropertyWriter.Invoke(cancelToken, comms, prop);
                }
            });

        // Serialisers & Deserialisers
        var sensorNameSerializer =
            new Property<string>.Serializer(str =>
            {
                if (SensorName == null) return Array.Empty<byte>();
                    
                while (str != null && Encoding.UTF8.GetByteCount(str) > SensorName.Length)
                {
                    str = str.Substring(0, str.Length - 1);
                }

                if (str == null) return Array.Empty<byte>();
                    
                return Encoding.UTF8.GetBytes(str).Concat(Enumerable.Repeat<byte>(0, SensorName.Length)).Take(SensorName.Length).ToArray();
            });

        var sensorNameDeserializer =
            new Property<string>.Deserializer(bytes =>
            {
                if (SensorName == null) return string.Empty;
                    
                var len = SensorName.Length;
                        
                while (len > 0)
                {
                    try { return Encoding.UTF8.GetString(bytes, 0, len); }
                    catch (Exception ex) when (!(ex is ArgumentNullException)) { --len; }
                };

                return string.Empty;
            });

        var measurementDeserializer =
            new ReadOnlyProperty<Reading>.Deserializer(data =>
            {
                var r = new Reading
                {
                    Time = DateTime.Now,
                    OilTemp = new Temperature(BitConverter.ToSingle(data, 0), Temperature.BaseUnit),
                    AmbTemp = new Temperature(BitConverter.ToSingle(data, 4), Temperature.BaseUnit)
                };

                if (Features != null && ((Features.IsSupported && (Features & EFeatures.AlertState) == 0) || (!Features.IsSupported && FwVersion >= 3.06f)))
                {
                    r.AlertState = BitConverter.ToSingle(data, 32);
                }

                if (Features != null && (!Features.IsSupported || (Features & EFeatures.OilCondition) == 0))
                {
                    r.OilCond = new OilCondition(BitConverter.ToSingle(data, 8), OilCondition.BaseUnit);
                }

                return r;
            });

        // All common RW information
        NodeId = new BasicProperty<byte>(nameof(NodeId), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.NodeId);
        SerialType = new BasicProperty<ESerialType>(nameof(SerialType), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.SerialType);
        FilterRollingAvg = new BasicProperty<byte>(nameof(FilterRollingAvg), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.FilterRollingAvg);
        BaudRateRs485 = new BasicProperty<EBaudRateRs485>(nameof(BaudRateRs485), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.BaudRate);
        BaudRateCanbus = new BasicProperty<EBaudRateCanbus>(nameof(BaudRateCanbus), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.BaudRateCanbus);
        OilNumber = new BasicProperty<OilInfo.SerialNumber>(nameof(OilNumber), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.OilNumber, oilNumberReader, oilNumberWriter);
        SensorName = new Property<string>(nameof(SensorName), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.SensorName, SensorNameMaxBytes, sensorNameSerializer, sensorNameDeserializer);

        // All common RO information
        SerialNumber = new BasicReadOnlyProperty<uint>(nameof(SerialNumber), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.SerialNumber);

        // Firmware Dependent parameters
        OilPolynomial = new OilPolynomialProperty(() => FwVersion < 3.00f, nameof(OilPolynomial), this);
        OilTable = new OilTableProperty(() => 3.00f < FwVersion, nameof(OilTable), this);

        FilterSpikeReject = new BasicProperty<float>(() => 3.00f < FwVersion, nameof(FilterSpikeReject), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.FilterSpikeReject);
        FilterHysteresis = new BasicProperty<float>(() => 3.00f < FwVersion, nameof(FilterHysteresis), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.FilterHysteresis);
        PassVariance = new BasicProperty<float>(() => 3.00f < FwVersion, nameof(PassVariance), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.PassVariance);

        Features = new BasicReadOnlyProperty<EFeatures>(() => false, nameof(Features), CommsInfo, WakeCode, (char)EOpCodes.SecondConfig, (ushort)EMemLocation.Varient);

        // Configuration Dependent parameters
        EndOfLife = new Property<OilCondition>(() => Features.IsSupported ? (Features.Actual & EFeatures.AlertState) != 0 || (Features.Actual & EFeatures.RemainingLife) != 0 : 3.06f <= FwVersion,
            nameof(EndOfLife), CommsInfo, WakeCode, (char)EOpCodes.Config, (ushort)EMemLocation.EndOfLife, 4,
            value => BitConverter.GetBytes((float)value.BaseValue),
            data => new OilCondition(BitConverter.ToSingle(data, 0), OilCondition.BaseUnit));

        Measurement = new ReadOnlyProperty<Reading>(nameof(Measurement), CommsInfo, WakeCode, (char)EOpCodes.Reading, (ushort)0x0000, 36, measurementDeserializer);
    }

    public override async Task Initialize(CancellationToken cancellationToken)
    {
        await base.Initialize(cancellationToken).ConfigureAwait(false);

        // Optional features
        if (Features.IsSupported)
        {
            await Features.Read(cancellationToken);
        }

        _isInitialized = true;
    }

    public IEnumerable<ESerialType> GetSupportedSerialTypes()
    {
        if (!_isInitialized)
        {
            throw new ArgumentException("Device has not been initialized.");
        }

        if (Features.IsSupported)
        {
            var comms = new List<ESerialType>();

            if ((Features & EFeatures.Comms_TanDelta) == 0)
                comms.Add(ESerialType.TanDeltaOnRs485);
            if ((Features & EFeatures.Comms_ModbusRtu) == 0)
                comms.Add(ESerialType.ModbusOnRs485);
            if ((Features & EFeatures.Comms_CanOpen) == 0)
                comms.Add(ESerialType.CanOpenOnCanbus);
            if ((Features & EFeatures.Comms_J1939) == 0)
                comms.Add(ESerialType.J1939OnCanbus);
            return comms;
        }

        if (2.00f <= FwVersion && FwVersion < 3.04)
        {
            return new ESerialType[]
            {
                ESerialType.TanDeltaOnRs485,
                ESerialType.ModbusOnRs485,
                ESerialType.CanOpenOnCanbus,
            };
        }

        if (3.04f <= FwVersion)
        {
            return new ESerialType[]
            {
                ESerialType.TanDeltaOnRs485,
                ESerialType.ModbusOnRs485,
                ESerialType.CanOpenOnCanbus,
                ESerialType.J1939OnCanbus
            };
        }

        throw new NotSupportedException($"{nameof(FwVersion)} {FwVersion.Actual} is not supported.");
    }
    public OilInfo.EProfileType GetOilProfileType()
    {
        // This serial number range is an inductor sensor.
        if (SerialNumber != null && 1001390 <= SerialNumber && SerialNumber < 1003000)
        {
            if (2.0f <= FwVersion && FwVersion < 3.0f)
                return OilInfo.EProfileType.Gen1_1;
            if (3.0f <= FwVersion && FwVersion < 4.0f)
                return OilInfo.EProfileType.Gen1_2;
        }
        else
        {
            if (2.0f <= FwVersion && FwVersion < 3.0f)
                return OilInfo.EProfileType.Gen1_0;
            if (3.0f <= FwVersion && FwVersion < 4.0f)
                return OilInfo.EProfileType.Gen1_3;
            if (4.0f <= FwVersion)
                return OilInfo.EProfileType.Gen2_0;
        }
        return OilInfo.EProfileType.Unsupported;
    }
    public BasicReadOnlyProperty<EFeatures> Features { get; protected set; }

    public BasicProperty<byte> NodeId { get; protected set; }

    public BasicProperty<byte> FilterRollingAvg { get; protected set; }
    public BasicProperty<float> FilterSpikeReject { get; protected set; }
    public BasicProperty<float> FilterHysteresis { get; protected set; }
    public BasicProperty<float> PassVariance { get; protected set; }

    public BasicProperty<EConfigFlags>? ConfigFlags { get; protected set; }
    public BasicProperty<ESerialType> SerialType { get; protected set; }
    public BasicProperty<EBaudRateRs485> BaudRateRs485 { get; protected set; }
    public BasicProperty<EBaudRateCanbus> BaudRateCanbus { get; protected set; }

    public BasicProperty<float>? CalThermTemp0 { get; protected set; }
    public BasicProperty<float>? CalThermTemp1 { get; protected set; }
    public BasicProperty<float>? CalThermGrad0 { get; protected set; }
    public BasicProperty<float>? CalThermGrad1 { get; protected set; }
    public BasicProperty<float>? CalThermGrad2 { get; protected set; }

    public Property<string> SensorName { get; protected set; }

    public Property<OilCondition> EndOfLife { get; protected set; }

    public OilPolynomialProperty OilPolynomial { get; protected set; }
    public OilTableProperty OilTable { get; private set; }
    public BasicProperty<OilInfo.SerialNumber> OilNumber { get; private set; }

    private bool _isInitialized;
}