// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common.Units;

using System.Runtime.InteropServices;
using System.Text;

namespace Common;

public class OilInfo : IEquatable<OilInfo>
{
    public OilInfo(SerialNumber oilSerial, string manufacturer, string oilName, Viscosity viscosity, Temperature minTemp, Temperature maxTemp, string application, EProfileType profileType, string profile, DateTime dateProfiled)
    {
        OilSerial = oilSerial;
        Manufacturer = manufacturer;
        OilName = oilName;
        Viscosity = viscosity;
        MinTemp = new Temperature(minTemp);
        MaxTemp = new Temperature(maxTemp);
        Application = application;
        ProfileType = profileType;
        
        switch (profileType)
        {
            case EProfileType.Gen1_0:
            case EProfileType.Gen1_1:
            {
                var data = Hex.Decode(Encoding.ASCII.GetBytes(profile));

                if (data.Length != 37)
                    throw new ArgumentException("Expected exactly 37 bytes", nameof(profile));

                Profile = data;
            }
                break;
            case EProfileType.Gen1_3:
            case EProfileType.Gen1_2:
            case EProfileType.Gen2_0:
            {
                var data = Hex.Decode(Encoding.ASCII.GetBytes(profile));

                //if (data.Length != 380)
                    //throw new ArgumentException("Expected exactly 380 bytes", nameof(profile));

                Profile = data;
            }
                break;
            default:
                throw new ArgumentException("ProfileType is not recognized", nameof(profileType));
        }
        DateProfiled = dateProfiled;
    }

    public SerialNumber OilSerial { get; }
    public string Manufacturer { get; }
    public string OilName { get; }
    public Viscosity Viscosity { get; }
    public string Application { get; }
    public Temperature MinTemp { get; }
    public Temperature MaxTemp { get; }
    public EProfileType ProfileType { get; }
    public byte[] Profile { get; }
    public DateTime DateProfiled { get; }

    public override string ToString()
    {
        return new[]
            {
                Manufacturer,
                OilName,
                Viscosity.ToString()
            }.Where(s => !string.IsNullOrWhiteSpace(s))
            .Aggregate((s1, s2) => $"{s1}, {s2}");
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as OilInfo);
    }
    public bool Equals(OilInfo? other)
    {
        if (other is null)
            return base.Equals(other);

        return Equals(OilSerial, other.OilSerial)
               && Equals(Manufacturer, other.Manufacturer)
               && Equals(OilName, other.OilName)
               && Equals(Viscosity, other.Viscosity)
               && Equals(Application, other.Application)
               && Equals(MinTemp, other.MinTemp)
               && Equals(MaxTemp, other.MaxTemp)
               && Equals(ProfileType, other.ProfileType)
               && Enumerable.SequenceEqual(Profile, other.Profile);
    }
    public override int GetHashCode()
    {
        const int initialHash = 595335368;

        return initialHash.HashWith(
            OilSerial,
            Manufacturer,
            OilName,
            Viscosity,
            Application,
            MinTemp,
            MaxTemp,
            ProfileType,
            Profile
        );
    }

    public enum EProfileFlags : byte
    {
        None,
    }
    public enum EViscosityGrade : uint //-V3059
    { //-V3059
        Unknown = 0x00,
        Iso = 0x10,
        Sae = 0x20,
    }
    public enum ESourceTemperature : uint //-V3059
    { //-V3059
        Celsius = 0x00,
        Fahrenheit = 0x40
    }
    public enum EProfileType : byte
    {
        Unsupported = 0x00,
        Gen1_0, // Polynomial
        Gen1_1, // Polynomial with inductor
        Gen1_2, // Matrix with inductor
        Gen1_3, // Matrix
        Gen2_0, // Matrix for Gen2.0 (normalised with tabs)
        Gen2_1  // Matrix for Gen2.1 (normalised with tubes)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 14)]
    public struct SerialNumber : IComparable<SerialNumber>, IComparable, IEquatable<SerialNumber>
    {
        public static int Length { get => Marshal.SizeOf(typeof(SerialNumber)); }

        public ushort Manufacturer;
        public ushort Range;
        public ushort Flavor;
        public ushort Viscosity;
        public byte Reserved0;
        public byte Reserved1;
        public byte Reserved2;
        public EProfileFlags Flags;
        public EProfileType ProfileType;
        public byte Iteration;

        public byte[] ToBytes()
        {
            var buffer = new byte[Length];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pBuffer = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr(this, pBuffer, false);
            handle.Free();
            return buffer;
        }
        public override string ToString()
        {
            return ToString(false);
        }
        public string ToString(bool upperCase)
        {
            var builder = new StringBuilder();
            var buffer = ToBytes();
            var formatter = upperCase ? "X2" : "x2";

            foreach (byte b in buffer)
            {
                builder.Append(b.ToString(formatter));
            }

            return builder.ToString();
        }
        public static SerialNumber Parse(byte[] serialNum)
        {
            if (serialNum.Length != Length)
            {
                throw new ArgumentException($"{nameof(serialNum)} must be exactly {Length} bytes, received {serialNum.Length}", nameof(serialNum));
            }

            var handle = GCHandle.Alloc(serialNum, GCHandleType.Pinned);
            
            var pBuffer = handle.AddrOfPinnedObject();
            
            var value = (SerialNumber)(Marshal.PtrToStructure(pBuffer, typeof(SerialNumber)) ?? throw new InvalidOperationException());
            
            handle.Free();
            
            return value;
        }
        public static SerialNumber Parse(string value)
        {
            if (Encoding.ASCII.GetByteCount(value) == Length)
            {
                return Parse(Encoding.ASCII.GetBytes(value));
            }

            if (Encoding.ASCII.GetByteCount(value) != Length * 2)
            {
                throw new FormatException("Invalid oil serial number format");
            }

            var buffer = new byte[Length];
                
            //TODO: Fix oil serial number and remove the second condition in this loop
            for (var i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = byte.Parse(value.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
                
            return Parse(buffer);
        }

        public int CompareTo(SerialNumber other)
        {
            var myBytes = ToBytes();
            
            var otherBytes = other.ToBytes();
            
            //Mask off last 2 bytes until new serial format is ready.
            for (var i = 0; i < myBytes.Length - 2; ++i)
            {
                var cmp = myBytes[i].CompareTo(otherBytes[i]);
                
                if (cmp == 0) continue;
                
                return cmp;
            }
            
            return 0;
        }
        public int CompareTo(object? obj)
        {
            if (obj is SerialNumber sn)
            {
                return CompareTo(sn);
            }

            throw new InvalidCastException($"Can't compare type of '{obj?.GetType()}' with '{typeof(SerialNumber)}'");
        }

        public override bool Equals(object? obj)
        {
            if (obj is SerialNumber sn)
            {
                return Equals(sn);
            }

            return base.Equals(obj);
        }
        public bool Equals(SerialNumber other)
        {
            return CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            // Start with pointer hash and then use the contents
            var bytes = ToBytes();
            return bytes.HashWith(bytes);
        }

        public static bool operator ==(SerialNumber val0, SerialNumber val1)
        {
            return val0.CompareTo(val1) == 0;
        }
        public static bool operator !=(SerialNumber val0, SerialNumber val1)
        {
            return val0.CompareTo(val1) != 0;
        }
        public static bool operator <(SerialNumber val0, SerialNumber val1)
        {
            return val0.CompareTo(val1) < 0;
        }
        public static bool operator >(SerialNumber val0, SerialNumber val1)
        {
            return val0.CompareTo(val1) > 0;
        }
        public static bool operator <=(SerialNumber val0, SerialNumber val1)
        {
            return val0.CompareTo(val1) <= 0;
        }
        public static bool operator >=(SerialNumber val0, SerialNumber val1)
        {
            return val0.CompareTo(val1) >= 0;
        }
    }
}