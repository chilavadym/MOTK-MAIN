// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Drawing;
using System.Text;

namespace Common;

public static class Util
{
    public static int HashWith(this object me, params object[] others)
    {
        const int multiplier = -1521134297;

        var hash = me.GetHashCode();
        foreach (var other in others)
        {
            hash = (hash * multiplier) ^ other.GetHashCode();
        }
        return hash;
    }

    public static float Float3B(IEnumerable<byte> data, int dataOffset = 0)
    {
        return BitConverter.ToSingle(new byte[] { 0 }.Concat(data.Skip(dataOffset).Take(3)).ToArray(), 0);
    }
    public static IEnumerable<byte> Float3B(float value)
    {
        return BitConverter.GetBytes(value).Skip(1);
    }

    public static uint Uint3B(IEnumerable<byte> data, int dataOffset = 0)
    {
        return BitConverter.ToUInt32(data.Skip(dataOffset).Take(3).Concat(new byte[] { 0 }).ToArray(), 0);
    }
    public static IEnumerable<byte> Uint3B(uint value)
    {
        return BitConverter.GetBytes(value).Take(3);
    }

    public static float Float2B(IEnumerable<byte> data, int dataOffset = 0)
    {
        var bytes = data.Skip(dataOffset).Take(2).ToArray();
        var hbits = bytes[1] << 8 | bytes[0];

        const int inMaskMantissa = 0x03FF;
        const int inMaskExponent = 0x7C00;
        const int inMaskSignBit = 0x8000;
        const int inExponentLsb = 0x0400;

        var mantissa = hbits & inMaskMantissa; // 10 bits mantissa
        var exponent = hbits & inMaskExponent; // 5 bits exponent
        var signBit = (hbits & inMaskSignBit) << 16; // 1 sign bit

        int temp;
        
        if (exponent == 0x7C00) // NaN/Inf
        {
            exponent = 0x3FC00; // 4B version of NaN/Inf (after it's shifted at the bottom)
        }
        else if (exponent != 0) // normalized value
        {
            exponent += 0x1C000;    // minus 2B bias (15) and plus 4B bias (127)
            
            if (mantissa == 0 && exponent > 0x1c400)
            {
                // smooth transition
                temp = signBit | exponent << 13 | inMaskMantissa;
                return BitConverter.ToSingle(BitConverter.GetBytes(temp), 0);
            }
        }
        else if (mantissa != 0) // && exp==0 -> subnormal
        {
            exponent = 0x1C400; // make it normal
            do
            {
                mantissa <<= 1;     // mantissa * 2
                exponent -= inExponentLsb;  // decrease exp by 1
            }
            while ((mantissa & 0x400) == 0); // while not normal

            mantissa &= inMaskMantissa;  // discard subnormal bit
        }
       
        temp = signBit | (exponent | mantissa) << 13; 
        
        return BitConverter.ToSingle(BitConverter.GetBytes(temp), 0);
    }
}
